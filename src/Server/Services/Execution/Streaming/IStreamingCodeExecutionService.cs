using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Server.Services.Execution.Streaming;

/// <summary>
/// Streaming, Interactive based code execution service
/// </summary>
public interface IStreamingCodeExecutionService
{
    /// <summary>
    /// Executes the code in a streaming manner.
    /// </summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="compilerVersion">The version of the compiler to use.</param>
    /// <param name="nugetPackages">The list of NuGet packages required for execution.</param>
    /// <param name="sessionId">The session ID for tracking purposes.</param>
    /// <param name="interactive">Indicates whether the execution is interactive or not.</param>
    /// <returns>A task representing the asynchronous code execution.</returns>
    Task ExecuteCodeStreamingAsync(string code, string compilerVersion, List<NugetPackage> nugetPackages, string sessionId, bool? interactive);
}
