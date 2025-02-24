using SharpPad.Shared.Models.Compiler;
using SharpPad.Shared.Models.Nuget;
using System.Runtime.CompilerServices;

namespace SharpPad.Server.Services.Execution.Compiler;

/// <summary>
/// The code execution service interface.
/// </summary>
public interface ICodeExecutionService
{
    /// <summary>
    /// Executes the code asynchronously.
    /// </summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="compilerVersion">The compiler version to use.</param>
    /// <param name="nugetPackages">The NuGet packages to include.</param>
    Task<CodeExecutionResponse> ExecuteCodeAsync(string code, string compilerVersion, List<NugetPackage> nugetPackages);
}