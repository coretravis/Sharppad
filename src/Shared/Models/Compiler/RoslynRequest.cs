namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Request Model for Roslyn API
/// </summary>
public class RoslynRequest
{
    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position.
    /// Cursor position for autocomplete & go-to definition.
    /// </summary>
    public int Position { get; set; } // Cursor position for autocomplete & go-to definition
}
