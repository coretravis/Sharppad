namespace SharpPad.Client.Services.Library;

/// <summary>
/// Represents a library script package.
/// </summary>
public class LibraryScriptPackage
{
    /// <summary>
    /// Gets or sets the ID of the package.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the package.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
