namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Represents the response of a code execution.
/// </summary>
public class CodeExecutionResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the code execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the list of execution outputs.
    /// </summary>
    public List<ExecutionOutput> Outputs { get; set; } = new List<ExecutionOutput>();

    /// <summary>
    /// Gets or sets the list of files generated during the code execution.
    /// </summary>
    public List<string> Files { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the execution metrics.
    /// </summary>
    public ExecutionMetrics Metrics { get; set; } = new ExecutionMetrics();

    /// <summary>
    /// Gets or sets the version of the compiler used for the code execution.
    /// </summary>
    public string CompilerVersion { get; set; } = string.Empty;
}
