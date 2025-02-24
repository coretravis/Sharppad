namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Represents the execution metrics of a compiled code.
/// </summary>
public class ExecutionMetrics
{
    /// <summary>
    /// Gets or sets the time taken for compilation.
    /// </summary>
    public TimeSpan CompilationTime { get; set; }

    /// <summary>
    /// Gets or sets the time taken for execution.
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the peak memory usage during execution.
    /// </summary>
    public long PeakMemoryUsage { get; set; }

    /// <summary>
    /// Gets or sets the count of output produced during execution.
    /// </summary>
    public int OutputCount { get; set; }
}
