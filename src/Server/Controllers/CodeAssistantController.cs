using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpPad.Server.Services.AI;
using SharpPad.Shared.Models.AI;

namespace SharpPad.Server.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="CodeAssistantController"/> class.
/// </summary>
/// <param name="codeAssistantService">The code assistant service.</param>
[ApiController]
[Route("api/[controller]")]
public class CodeAssistantController(ICodeAssistantService codeAssistantService) : ControllerBase
{
    private readonly ICodeAssistantService _codeAssistantService = codeAssistantService;

    /// <summary>
    /// Explains the provided code.
    /// </summary>
    /// <param name="request">The code assistant request.</param>
    /// <returns>The code assistant response containing the explanation.</returns>
    /// <response code="200">Returns the explanation.</response>
    /// <response code="400">If code or language is missing.</response>
    [HttpPost("explain")]
    public async Task<ActionResult<CodeAssistantResponse>> ExplainCode([FromBody] CodeAssistantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Language))
        {
            return BadRequest("Code and language are required.");
        }

        var explanation = await _codeAssistantService.ExplainCodeAsync(request.Code, request.Language);

        // Log explanation
        LogExplanation(request.Code, request.Language, explanation);

        return Ok(new CodeAssistantResponse { Result = explanation });
    }


    /// <summary>
    /// Fixes an error in the provided code error.
    /// </summary>
    /// <param name="request">The code assistant request.</param>
    /// <returns>The code assistant response containing the explanation.</returns>
    /// <response code="200">Returns the explanation.</response>
    /// <response code="400">If code or language is missing.</response>
    [HttpPost("fix")]
    public async Task<ActionResult<CodeAssistantResponse>> FixCodeError([FromBody] CodeAssistantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Language) ||string.IsNullOrWhiteSpace(request.ErrorMessage))
        {
            return BadRequest("Code and language are required.");
        }

        var explanation = await _codeAssistantService.FixCodeErrorAsync(request.Code,  request.ErrorMessage, request.Language);

        // Log explanation
        LogExplanation(request.Code, request.Language, explanation);

        return Ok(new CodeAssistantResponse { Result = explanation });
    }

    /// <summary>
    /// Optimizes the provided code.
    /// </summary>
    /// <param name="request">The code assistant request.</param>
    /// <returns>The code assistant response containing the optimized code.</returns>
    /// <response code="200">Returns the optimized code.</response>
    /// <response code="400">If code or language is missing.</response>
    [HttpPost("optimize")]
    public async Task<ActionResult<CodeAssistantResponse>> OptimizeCode([FromBody] CodeAssistantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Language))
        {
            return BadRequest("Code and language are required.");
        }

        var optimizedCode = await _codeAssistantService.OptimizeCodeAsync(request.Code, request.Language);

        // Log optimized code
        LogOptimizedCode(request.Code, request.Language, optimizedCode);

        return Ok(new CodeAssistantResponse { Result = optimizedCode });
    }

    /// <summary>
    /// Adds documentation to the provided code.
    /// </summary>
    /// <param name="request">The code assistant request.</param>
    /// <returns>The code assistant response containing the documented code.</returns>
    [HttpPost("document")]
    public async Task<ActionResult<CodeAssistantResponse>> AddDocumentation([FromBody] CodeAssistantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Language))
        {
            return BadRequest("Code and language are required.");
        }

        var documentedCode = await _codeAssistantService.AddDocumentationAsync(request.Code, request.Language);

        // Log documented code
        LogDocumentedCode(request.Code, request.Language, documentedCode);

        return Ok(new CodeAssistantResponse { Result = documentedCode });
    }

    /// <summary>
    /// Answers the provided question related to the code.
    /// </summary>
    /// <param name="request">The question request.</param>
    /// <returns>The code assistant response containing the answer.</returns>
    [HttpPost("question")]
    public async Task<ActionResult<CodeAssistantResponse>> AnswerQuestion([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) ||
            string.IsNullOrWhiteSpace(request.Language) ||
            string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Code, language, and question are required.");
        }

        var answer = await _codeAssistantService.AnswerQuestionAsync(request.Code, request.Language, request.Question);

        // Log answer
        LogAnswer(request.Code, request.Language, request.Question, answer);

        return Ok(new CodeAssistantResponse { Result = answer });
    }

    private void LogExplanation(string code, string language, string explanation)
    {
        // Log the explanation to XML file or database
    }

    private void LogOptimizedCode(string code, string language, string optimizedCode)
    {
        // Log the optimized code to XML file or database
    }

    private void LogDocumentedCode(string code, string language, string documentedCode)
    {
        // Log the documented code to XML file or database
    }

    private void LogAnswer(string code, string language, string question, string answer)
    {
        // Log the answer to XML file or database
    }
}
