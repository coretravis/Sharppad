namespace SharpPad.Server.Services.Execution.Analysis;

/// <summary>
/// The result of static code analysis.
/// </summary>
public class StaticAnalysisResult
{
    /// <summary>
    /// Indicates whether the submitted code passed the static analysis (i.e. no dangerous patterns detected).
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// A list of warnings about detected patterns that may be unsafe.
    /// </summary>
    public List<string> Warnings { get; set; } = new List<string>();
}
