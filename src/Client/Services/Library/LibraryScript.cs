namespace SharpPad.Client.Services.Library;

public class LibraryScript
{
    /// <summary>
    /// Gets or sets the unique identifier of the library script.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the library script.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the code of the library script.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language of the library script.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the compiler version of the library script.
    /// </summary>
    public string CompilerVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the library script.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the library script.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags of the library script.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner ID of the library script.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the library script is private.
    /// </summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>
    /// Gets or sets the NuGet packages used by the library script.
    /// </summary>
    public List<LibraryScriptPackage> NugetPackages { get; set; } = new();
}
