using Microsoft.CodeAnalysis;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using SharpPad.Shared.Models.Nuget;
using System.IO.Compression;

namespace SharpPad.Server.Services.Nugets
{
    public class NugetPackageService(ILogger<NugetPackageService> logger) : INugetPackageService
    {
        private const string PackageSourceUrl = "https://api.nuget.org/v3/index.json";
        private const string DefaultTargetFramework = "netstandard2.0";

        private readonly ILogger<NugetPackageService> _logger = logger;

        
        public async Task<List<MetadataReference>> GetMetadataReferencesAsync(
            List<NugetPackage> packages,
            string targetFramework = DefaultTargetFramework)
        {
            _logger.LogInformation("Getting metadata references for {Count} packages", packages.Count);
            var references = new List<MetadataReference>();

            if (packages == null || packages.Count == 0)
            {
                _logger.LogInformation("No packages found");
                return references;
            }

            // Track processed packages ("Id:Version") to avoid duplicate work.
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            NuGet.Common.ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3(PackageSourceUrl);

            // Process each package.
            foreach (var package in packages)
            {
                if (!NuGetVersion.TryParse(package.Version, out var packageVersion))
                {
                    // Skip if the version string cannot be parsed.
                    continue;
                }

                await ProcessPackageAsync(
                    package.Id,
                    packageVersion,
                    references,
                    processed,
                    cache,
                    logger,
                    cancellationToken,
                    repository,
                    targetFramework);
            }

            _logger.LogInformation("Got {Count} metadata references", references.Count);

            return references;
        }

        /// <summary>
        /// Downloads (or reuses a cached copy) and processes a package:
        ///   - Downloads the .nupkg file if it isn’t cached.
        ///   - Extracts the package (if not already extracted).
        ///   - Uses NuGet.Frameworks to choose the best matching target framework folder.
        ///   - Loads DLLs from that folder.
        ///   - Resolves transitive dependencies recursively.
        /// </summary>
        private async Task ProcessPackageAsync(
            string packageId,
            NuGetVersion packageVersion,
            List<MetadataReference> references,
            HashSet<string> processed,
            SourceCacheContext cache,
            NuGet.Common.ILogger logger,
            CancellationToken cancellationToken,
            SourceRepository repository,
            string targetFramework)
        {
            // Create a unique key for this package to avoid reprocessing it.
            string key = $"{packageId}:{packageVersion.ToNormalizedString()}";
            if (processed.Contains(key))
            {
                return;
            }
            processed.Add(key);

            try
            {
                // Get the resource to download the package.
                var findPackageResource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

                // Build a temporary directory for this package.
                string tempDir = Path.Combine(Path.GetTempPath(), "NuGetPackages", packageId, packageVersion.ToNormalizedString());
                Directory.CreateDirectory(tempDir);
                string packageFilePath = Path.Combine(tempDir, $"{packageId}.{packageVersion.ToNormalizedString()}.nupkg");

                // Check if the package file is already cached.
                if (!File.Exists(packageFilePath))
                {
                    using (var packageStream = new MemoryStream())
                    {
                        bool success = await findPackageResource.CopyNupkgToStreamAsync(
                            packageId,
                            packageVersion,
                            packageStream,
                            cache,
                            logger,
                            cancellationToken);

                        if (!success)
                        {
                            // Skip if download fails.
                            return;
                        }

                        packageStream.Position = 0;
                        using (var fileStream = File.Create(packageFilePath))
                        {
                            await packageStream.CopyToAsync(fileStream, cancellationToken);
                        }
                    }
                }

                // Extract the package if it hasn’t been extracted yet.
                string extractPath = Path.Combine(tempDir, "extracted");
                if (!Directory.Exists(extractPath))
                {
                    ZipFile.ExtractToDirectory(packageFilePath, extractPath);
                }

                // Look for the lib folder.
                string libFolder = Path.Combine(extractPath, "lib");
                if (!Directory.Exists(libFolder))
                {
                    // No assemblies available.
                    return;
                }

                var tfmFolders = Directory.GetDirectories(libFolder);
                if (tfmFolders.Length == 0)
                {
                    return;
                }

                // Determine the best matching target framework folder.
                // Parse the desired target framework.
                NuGetFramework desiredFramework = NuGetFramework.ParseFolder(targetFramework);
                // Build a list of available frameworks from the folder names.
                var availableFrameworks = tfmFolders
                    .Select(folder => new
                    {
                        Folder = folder,
                        Framework = NuGetFramework.ParseFolder(Path.GetFileName(folder))
                    })
                    .ToList();

                // Use FrameworkReducer to find the best match.
                var reducer = new FrameworkReducer();
                var bestMatch = reducer.GetNearest(desiredFramework, availableFrameworks.Select(x => x.Framework));
                string? selectedFolder = bestMatch != null ? 
                    availableFrameworks.FirstOrDefault(x => x.Framework.Equals(bestMatch))?.Folder : null;

                // Fallback if no match is found.
                if (string.IsNullOrEmpty(selectedFolder))
                {
                    selectedFolder = tfmFolders.First();
                }

                // Load all DLLs from the selected folder.
                var dllFiles = Directory.GetFiles(selectedFolder, "*.dll");
                foreach (var dll in dllFiles)
                {
                    try
                    {
                        var metadataReference = MetadataReference.CreateFromFile(dll);
                        references.Add(metadataReference);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not load reference from {dll}: {ex.Message}");
                    }
                }

                // Resolve transitive dependencies.
                var dependencyResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
                var packageIdentity = new PackageIdentity(packageId, packageVersion);
                var dependencyInfo = await dependencyResource.ResolvePackage(
                    packageIdentity,
                    NuGetFramework.ParseFolder(targetFramework),
                    cache,
                    logger,
                    cancellationToken);
                if (dependencyInfo != null)
                {
                    foreach (var dependency in dependencyInfo.Dependencies)
                    {
                        // For simplicity, using the minimum version in the dependency range.
                        await ProcessPackageAsync(
                            dependency.Id,
                            dependency.VersionRange?.MinVersion ?? NuGetVersion.Parse("0.0.0"),
                            references,
                            processed,
                            cache,
                            logger,
                            cancellationToken,
                            repository,
                            targetFramework);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log errors and move on to the next package.
                Console.WriteLine($"Error processing package {packageId} {packageVersion}: {ex.Message}");
            }
        }
    }
}
