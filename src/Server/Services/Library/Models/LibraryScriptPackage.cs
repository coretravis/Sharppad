namespace SharpPad.Server.Services.Library.Models;

/// <summary>
/// Represents a library script package.
/// </summary>
public class LibraryScriptPackage
{
    /// <summary>
    /// Gets or sets the ID of the script package.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the script package.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
