namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Represents the output of an execution.
/// </summary>
public class ExecutionOutput
{
    /// <summary>
    /// Gets or sets the content of the execution output.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content of the execution output.
    /// </summary>
    public char CharContent { get; set; } 

    /// <summary>
    /// Gets or sets the type of the execution output.
    /// </summary>
    public OutputType Type { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the execution output.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the execution output.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
