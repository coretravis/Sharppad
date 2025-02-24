namespace SharpPad.Client.Services.AI;


/// <summary>
/// Client for interacting with the Code Assistant API.
/// </summary>
public interface ICodeAssistantApiClient
{
    /// <summary>
    /// Calls the Document endpoint of the API.
    /// </summary>
    /// <param name="code">The source code to document.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>The code with added documentation returned by the API.</returns>
    Task<string> AddDocumentationAsync(string code, string language);

    /// <summary>
    /// Calls the Question endpoint of the API.
    /// </summary>
    /// <param name="code">The source code in question.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <param name="question">The question to ask about the code.</param>
    /// <returns>The answer provided by the API.</returns>
    Task<string> AnswerQuestionAsync(string code, string language, string question);
    /// <summary>
    /// Calls the Explain endpoint of the API.
    /// </summary>
    /// <param name="code">The source code to explain.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>The explanation provided by the API.</returns>
    Task<string> ExplainCodeAsync(string code, string language);

    /// <summary>
    /// Calls the Fix endpoint of the API.
    /// </summary>
    /// <param name="code">The code that produces the error messagee.</param>
    /// <param name="errorMessage">The error message to explain.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>The explanation provided by the API.</returns>
    Task<string> FixCodeAsync(string code, string errorMessage, string language);
    /// <summary>
    /// Calls the Optimize endpoint of the API.
    /// </summary>
    /// <param name="code">The source code to optimize.</param>
    /// <param name="language">The programming language of the code.</param>
    /// <returns>The optimized and formatted code returned by the API.</returns>
    Task<string> OptimizeCodeAsync(string code, string language);
}
