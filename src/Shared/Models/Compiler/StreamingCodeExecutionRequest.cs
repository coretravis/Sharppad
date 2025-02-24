using SharpPad.Shared.Models.Nuget;
using System.ComponentModel.DataAnnotations;

namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Request model for streaming code execution.
/// </summary>
public class StreamingCodeExecutionRequest
{
    /// <summary>
    /// The C# code to execute.
    /// </summary>
    [Required(ErrorMessage = "Code is required.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The compiler version to target (e.g., .net6.0).
    /// </summary>
    [Required(ErrorMessage = "CompilerVersion is required.")]
    public string CompilerVersion { get; set; } = string.Empty;

    /// <summary>
    /// The list of NuGet packages needed for code execution.
    /// </summary>
    [Required(ErrorMessage = "NugetPackages is required.")]
    public List<NugetPackage> NugetPackages { get; set; } = new List<NugetPackage>();

    /// <summary>
    /// The session identifier that maps to the client SignalR group.
    /// </summary>
    [Required(ErrorMessage = "SessionId is required.")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Determines if to run an interactive session
    /// </summary>
    public bool Interactive { get; set; } = false;
}
