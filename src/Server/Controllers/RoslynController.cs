using Microsoft.AspNetCore.Mvc;
using SharpPad.Server.Services.Execution.Compiler;
using SharpPad.Shared.Models.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpPad.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoslynController(IRoslynService roslynService) : ControllerBase
{
    private readonly IRoslynService _roslynService = roslynService;

    /// <summary>
    /// API: Get Auto-Completion Suggestions
    /// </summary>
    [HttpPost("autocomplete")]
    public async Task<IActionResult> GetAutocomplete([FromBody] RoslynRequest request)
    {
        var suggestions = await _roslynService.GetCompletionsAsync(request.Code, request.Position);
        return Ok(suggestions);
    }

    /// <summary>
    /// API: Get Syntax Errors
    /// </summary>
    [HttpPost("diagnostics")]
    public IActionResult GetDiagnostics([FromBody] RoslynRequest request)
    {
        var errors = _roslynService.GetDiagnostics(request.Code);
        return Ok(errors);
    }

    /// <summary>
    /// API: Format C# Code
    /// </summary>
    [HttpPost("format")]
    public IActionResult FormatCode([FromBody] RoslynRequest request)
    {
        return Ok(_roslynService.FormatCode(request.Code));
    }

    /// <summary>
    /// API: Find Go-To Definition
    /// </summary>
    [HttpPost("definition")]
    public async Task<IActionResult> FindDefinition([FromBody] RoslynRequest request)
    {
        var definition = await _roslynService.FindDefinitionAsync(request.Code, request.Position);
        return Ok(definition);
    }
}

