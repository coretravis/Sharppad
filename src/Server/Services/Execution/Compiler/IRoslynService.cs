using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution.Compiler;

/// <summary>
/// Represents the Roslyn service for code compilation and analysis.
/// </summary>
public interface IRoslynService
{
    /// <summary>
    /// Finds the definition of a symbol at the specified position in the code.
    /// </summary>
    /// <param name="code">The code to search in.</param>
    /// <param name="position">The position of the symbol.</param>
    /// <returns>The definition of the symbol as a string.</returns>
    Task<string> FindDefinitionAsync(string code, int position);

    /// <summary>
    /// Formats the given code.
    /// </summary>
    /// <param name="code">The code to format.</param>
    /// <returns>The formatted code.</returns>
    string FormatCode(string code);

    /// <summary>
    /// Gets the completions at the specified position in the code.
    /// </summary>
    /// <param name="code">The code to get completions for.</param>
    /// <param name="position">The position to get completions at.</param>
    /// <returns>A list of completion strings.</returns>
    Task<List<string>> GetCompletionsAsync(string code, int position);

    /// <summary>
    /// Gets the diagnostics for the given code.
    /// </summary>
    /// <param name="code">The code to get diagnostics for.</param>
    /// <returns>A list of diagnostic information.</returns>
    List<DiagnosticInfo> GetDiagnostics(string code);
}
