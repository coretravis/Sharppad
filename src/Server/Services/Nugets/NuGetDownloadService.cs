namespace SharpPad.Server.Services.Nugets;

/// <summary>
/// A service for downloading NuGet packages.
/// </summary>
/// <remarks>
/// This class downloads NuGet packages from the NuGet API.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="NuGetDownloadService"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
public class NuGetDownloadService(ILogger<NuGetDownloadService> logger) : INuGetDownloadService
{
    private readonly HttpClient _httpClient = new();
    private readonly ILogger _logger = logger;
    private const string NuGetPackageUrl = "https://www.nuget.org/api/v2/package";

    public async Task<string> DownloadPackageAsync(string packageId, string version, string destinationPath)
    {
        _logger.LogInformation("Downloading NuGet package {packageId} version {version} to {destinationPath}",
                               packageId,
                               version,
                               destinationPath);

        try
        {
            var packageUrl = $"{NuGetPackageUrl}/{packageId}/{version}";
            _logger.LogInformation("Downloading package from {packageUrl}", packageUrl);

            var response = await _httpClient.GetByteArrayAsync(packageUrl);
            _logger.LogInformation("Received response from NuGet package download");

            var filePath = Path.Combine(destinationPath, $"{packageId}.{version}.nupkg");
            _logger.LogInformation("Saving package to {filePath}", filePath);

            await File.WriteAllBytesAsync(filePath, response);
            _logger.LogInformation("Package saved successfully");

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error downloading NuGet package: {ex.Message}", ex.Message);
            throw new Exception($"Error downloading NuGet package: {ex.Message}", ex);
        }
    }
}