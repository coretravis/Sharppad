using Microsoft.AspNetCore.Mvc;
using SharpPad.Server.Services.Streaming;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class StreamingCodeExecutionController(
IStreamingCodeExecutionService streamingService,
ILogger<StreamingCodeExecutionController> logger) : ControllerBase {
    private readonly IStreamingCodeExecutionService _streamingService = streamingService ?? throw new ArgumentNullException(nameof(streamingService)); 
    private readonly ILogger<StreamingCodeExecutionController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteStreamingCode([FromBody] StreamingCodeExecutionRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Execute the code in a streaming session. The service will stream output and await input via SignalR.
            await _streamingService.ExecuteCodeStreamingAsync(request.Code, request.CompilerVersion, request.NugetPackages, request.SessionId, request.Interactive);
            // Return Accepted (202) to indicate the execution has started.
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing streaming code.");
            return StatusCode(500, "Internal Server Error.");
        }
    }
}

