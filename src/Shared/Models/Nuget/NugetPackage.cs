namespace SharpPad.Shared.Models.Nuget;

/// <summary>
/// Represents a NuGet package.
/// </summary>
public class NugetPackage
{
    /// <summary>
    /// Gets or sets the package ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authors of the package.
    /// </summary>
    public List<string> Authors { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the package version.
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// Gets or sets the summary of the package.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// Gets or sets the description of the package.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the title of the package.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the total number of downloads for the package.
    /// </summary>
    public long TotalDownloads { get; set; }
}
