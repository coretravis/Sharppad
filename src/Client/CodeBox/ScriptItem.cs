using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Client.CodeBox;

/// <summary>
/// Represents a script item.
/// </summary>
public class ScriptItem
{
    /// <summary>
    /// Gets or sets the title of the script item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the code of the script item.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language of the script item.
    /// </summary>
    public string Language { get; set; } = "C#";

    /// <summary>
    /// Gets or sets the compiler version of the script item.
    /// </summary>
    public string CompilerVersion { get; set; } = ".NET 8.0";

    /// <summary>
    /// Gets or sets the ID of the script item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the script item.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the author of the script item.
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// Gets or sets the owner ID of the script item.
    /// </summary>
    public string OwnerId { get; set; } = "";

    /// <summary>
    /// Gets or sets the tags of the script item.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the script item is private.
    /// </summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the script item is read-only.
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets the NuGet packages of the script item.
    /// </summary>
    public List<NugetPackage> NugetPackges { get; set; } = new();
}
