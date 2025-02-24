using SharpPad.Shared.Models.Nuget;
using System.ComponentModel.DataAnnotations;

namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Represents a request for code execution.
/// </summary>
public class CodeExecutionRequest
{
    /// <summary>
    /// Gets or sets the code to be executed.
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project path.
    /// </summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of NuGet packages required for code execution.
    /// </summary>
    [Required(ErrorMessage = "NugetPackages is required")]
    public List<NugetPackage> NugetPackages { get; set; } = new List<NugetPackage>();

    /// <summary>
    /// Gets or sets the compiler version.
    /// </summary>
    [Required(ErrorMessage = "CompilerVersion is required")]
    public string CompilerVersion { get; set; } = string.Empty;
}
