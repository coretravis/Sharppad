namespace SharpPad.Server.Services.Nugets;

/// <summary>
/// An interface for downloading NuGet packages.
/// </summary>
public interface INuGetDownloadService
{
    /// <summary>
    /// Downloads a NuGet package and saves it to the specified destination path.
    /// </summary>
    /// <param name="packageId">The NuGet package ID.</param>
    /// <param name="version">The NuGet package version.</param>
    /// <param name="destinationPath">The path to save the downloaded package.</param>
    /// <returns>The path to the downloaded package.</returns>
    /// <exception cref="Exception">Thrown when an error occurs during the download.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="packageId"/>, <paramref name="version"/>, or <paramref name="destinationPath"/> is null.</exception>
    Task<string> DownloadPackageAsync(string packageId, string version, string destinationPath);
}
