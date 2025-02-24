namespace SharpPad.Server.Services.Library.Models;

/// <summary>
/// Represents a library script.
/// </summary>
public class LibraryScript
{
    /// <summary>
    /// Gets or sets the unique identifier of the script.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the script.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the code of the script.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language of the script.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the compiler version of the script.
    /// </summary>
    public string CompilerVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the script.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the script.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags of the script.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner ID of the script.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the script is private.
    /// </summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>
    /// Gets or sets the NuGet packages used by the script.
    /// </summary>
    public List<LibraryScriptPackage> NugetPackages { get; set; } = new();
}
