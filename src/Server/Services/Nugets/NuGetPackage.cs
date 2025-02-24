namespace SharpPad.Server.Services.Nugets;

/// <summary>
/// Represents a NuGet package.
/// </summary>
public class NuGetPackage
{
    /// <summary>
    /// Gets or sets the unique identifier of the package.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the package.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief description of the package.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authors of the package.
    /// </summary>
    public string Authors { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of downloads of the package.
    /// </summary>
    public long TotalDownloads { get; set; }
}