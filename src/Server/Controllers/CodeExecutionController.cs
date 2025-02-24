using Microsoft.AspNetCore.Mvc;
using SharpPad.Server.Services.Execution.Compiler;
using SharpPad.Server.Services.Nugets;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Controllers;


/// <summary>
/// Controller for handling code execution requests.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CodeExecutionController"/> class.
/// </remarks>
/// <param name="codeExecutionService">The code execution service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/[controller]")]
public class CodeExecutionController(ICodeExecutionService codeExecutionService, ILogger<CodeExecutionController> logger) : ControllerBase
{
    /// <summary>
    /// Service for executing code.
    /// </summary>
    private readonly ICodeExecutionService _codeExecutionService = codeExecutionService;

    /// <summary>
    /// Logger for logging errors.
    /// </summary>
    private readonly ILogger<CodeExecutionController> _logger = logger;

    /// <summary>
    /// Executes the provided code and returns the result.
    /// </summary>
    /// <param name="request">The code execution request.</param>
    /// <returns>The result of the code execution.</returns>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteCode([FromBody] CodeExecutionRequest request)
    {
        try
        {
            // Validate the request 
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }

            // validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Execute the code using the code execution service
            var response = await _codeExecutionService.ExecuteCodeAsync(request.Code,
                                                                        request.CompilerVersion,
                                                                        request.NugetPackages);

            // Return the result as OK (200)
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Log the error using the logger
            _logger.LogError(ex, "Error executing code.");

            // Return an internal server error (500)
            return StatusCode(500, "Internal Server Error");
        }
    }
}