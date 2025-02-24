using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Client.Services.Integrations;

/// <summary>
/// NuGet search service
/// </summary>
public interface INuGetSearchService
{
    /// <summary>
    /// Searches for NuGet packages.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="skip">The skip.</param>
    /// <param name="take">The take.</param>
    /// <returns>A collection of NuGet packages.</returns>
    Task<IEnumerable<NugetPackage>> SearchPackagesAsync(string searchTerm, int skip = 0, int take = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the versions of a specific NuGet package.
    /// </summary>
    /// <param name="packageId">The package ID.</param>
    /// <returns>A collection of package versions.</returns>
    Task<IEnumerable<string>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken = default);
}
