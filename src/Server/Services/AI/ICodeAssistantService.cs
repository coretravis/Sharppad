namespace SharpPad.Server.Services.AI;

/// <summary>
/// Defines the interface for the code assistant service.
/// </summary>
public interface ICodeAssistantService
{
    /// <summary>
    /// Explains the provided code in the specified language.
    /// </summary>
    /// <param name="code">The source code to explain.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>A string containing the explanation of the code.</returns>
    Task<string> ExplainCodeAsync(string code, string language);

    /// <summary>
    /// Fixes the error in the provided code
    /// </summary>
    /// <param name="code">The code that resulted in an error</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="language">The programming language of the code</param>
    /// <returns></returns>
    Task<string> FixCodeErrorAsync(string code, string errorMessage, string language);

    /// <summary>
    /// Optimizes the provided code in the specified language for readability and performance.
    /// </summary>
    /// <param name="code">The source code to optimize.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>A string containing the optimized code.</returns>
    Task<string> OptimizeCodeAsync(string code, string language);

    /// <summary>
    /// Adds clear and concise documentation comments to the provided code in the specified language.
    /// </summary>
    /// <param name="code">The source code to document.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>A string containing the documented code.</returns>
    Task<string> AddDocumentationAsync(string code, string language);

    /// <summary>
    /// Answers a question related to the provided code in the specified language.
    /// </summary>
    /// <param name="code">The source code related to the question.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <param name="question">The question to answer.</param>
    /// <returns>A string containing the answer to the question.</returns>
    Task<string> AnswerQuestionAsync(string code, string language, string question);
}