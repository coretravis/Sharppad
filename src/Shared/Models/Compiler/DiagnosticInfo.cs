namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Represents diagnostic information for a compiler error or warning.
/// </summary>
public class DiagnosticInfo
{
    /// <summary>
    /// Gets or sets the error or warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start line of the error or warning.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Gets or sets the start column of the error or warning.
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// Gets or sets the end line of the error or warning.
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Gets or sets the end column of the error or warning.
    /// </summary>
    public int EndColumn { get; set; }

    /// <summary>
    /// Gets or sets the severity of the error or warning.
    /// </summary>
    public string Severity { get; set; } = string.Empty;
}
