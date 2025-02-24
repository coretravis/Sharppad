using Microsoft.CodeAnalysis;
using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Server.Services.Nugets;

public interface INugetPackageService
{
    
    /// <summary>
    /// Given a list of NuGet packages, downloads each package (or reuses a cached copy),
    /// extracts the DLLs from the best matching target framework folder (by default, netstandard2.0),
    /// resolves transitive dependencies, and returns the corresponding metadata references.
    /// </summary>
    /// <param name="packages">The list of NuGet packages.</param>
    /// <param name="targetFramework">The target framework.</param>
    /// <returns>A list of metadata references.</returns>
    /// <exception cref="Exception">Thrown when an error occurs.</exception>
    Task<List<MetadataReference>> GetMetadataReferencesAsync(List<NugetPackage> packages,
            string targetFramework);
}
