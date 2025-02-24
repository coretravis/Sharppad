using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpPad.Server.Services.Library;
using SharpPad.Server.Services.Library.Models;
using SharpPad.Server.Services.Users;

namespace SharpPad.Server.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LibraryScriptsController(
    IScriptLibraryService scriptService,
    IUserService userService,
    ILogger<LibraryScriptsController> logger) : ControllerBase
{
    private readonly IScriptLibraryService _scriptService = scriptService;
    private readonly IUserService _userService = userService;
    private readonly ILogger<LibraryScriptsController> _logger = logger;

    /// <summary>
    /// Retrieves a script by its id.
    /// </summary>
    /// <param name="id">The script id.</param>
    /// <param name="includeCode">Whether to include the code in the response.</param>
    /// <returns>The requested script, if found and accessible.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetScriptById(string id, [FromQuery] bool includeCode = true)
    {
        try
        {
            // Get the current user's id
            var userId = _userService.GetUserId() ?? string.Empty;
            // Retrieve the script
            var script = await _scriptService.GetScriptById(id, includeCode, userId);
            if (script == null)
            {
                _logger.LogInformation("GetScriptById: Script with id {ScriptId} not found or access denied.", id);
                return NotFound();
            }

            return Ok(script);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving script with id {ScriptId}.", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Retrieves a paged list of scripts that are public or owned by the current user.
    /// </summary>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <returns>A list of scripts.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllUserScripts([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            // Get the current user's id
            var userId = _userService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetAllUserScripts: Unauthorized access attempt. User id is missing.");
                return Unauthorized("User is not authenticated.");
            }

            // Retrieve scripts from the script service
            var scripts = await _scriptService.GetAllUserScripts(userId, skip, take);
            return Ok(scripts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scripts.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }


    /// <summary>
    /// Retrieves a list of all public scripts.
    /// </summary>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <returns>A list of public scripts.</returns>
    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetAllPublicScripts([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            // Retrieve public scripts from the script service
            var scripts = await _scriptService.GetAllPublicScripts(skip, take);

            // Return the scripts with a 200 OK status code
            return Ok(scripts);
        }
        catch (Exception ex)
        {
            // Log the exception and return a 500 Internal Server Error status code
            _logger.LogError(ex, "Error retrieving scripts.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Searches for scripts by title, description, or tags.
    /// Only public scripts or those owned by the current user are returned.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <returns>A list of matching scripts.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchScripts([FromQuery] string searchTerm, [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term cannot be empty.");
            }

            // Get the current user's id
            var userId = _userService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SearchUserScripts: Unauthorized access attempt. User id is missing.");
                return Unauthorized("User is not authenticated.");
            }

            // Retrieve scripts from the script service
            var scripts = await _scriptService.SearchUserScripts(searchTerm, userId, skip, take);
            return Ok(scripts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching scripts with term: {SearchTerm}.", searchTerm);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }


    /// <summary>
    /// Searches for public scripts by title, description, or tags.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <returns>A list of matching public scripts.</returns>
    [AllowAnonymous]
    [HttpGet("search/public")]
    public async Task<IActionResult> SearchPublicScripts([FromQuery] string searchTerm, [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            // Validate search term
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Return bad request if search term is empty
                return BadRequest("Search term cannot be empty.");
            }

            // Search for public scripts using script service
            var scripts = await _scriptService.SearchPublicScripts(searchTerm, skip, take);

            // Return OK response with matching scripts
            return Ok(scripts);
        }
        catch (Exception ex)
        {
            // Log error and return internal server error response
            _logger.LogError(ex, "Error searching scripts with term: {SearchTerm}.", searchTerm);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Creates a new library script.
    /// </summary>
    /// <param name="libraryScript">The script to create.</param>
    /// <returns>The newly created script.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateScript([FromBody] LibraryScript libraryScript)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current user's id
            var userId = _userService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("CreateScript: Unauthorized access attempt. User id is missing.");
                return Unauthorized("User is not authenticated.");
            }

            // Ensure the new script is owned by the current user.
            libraryScript.OwnerId = userId;
            libraryScript.Author = userId;
            var createdScript = await _scriptService.CreateScript(libraryScript);

            _logger.LogInformation("CreateScript: Script created with id {ScriptId} by user {UserId}.", createdScript.Id, userId);
            return CreatedAtAction(nameof(GetScriptById), new { id = createdScript.Id, includeCode = true }, createdScript);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating script.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Updates an existing script.
    /// Only the owner of the script is allowed to update it.
    /// </summary>
    /// <param name="id">The id of the script to update.</param>
    /// <param name="updatedScript">The updated script data.</param>
    /// <returns>The updated script.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateScript(string id, [FromBody] LibraryScript updatedScript)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!Guid.TryParse(id, out Guid scriptGuid) || updatedScript.Id != scriptGuid)
            {
                return BadRequest("Script id mismatch.");
            }

            // Get the current user's id
            var userId = _userService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("UpdateUserScript: Unauthorized access attempt. User id is missing.");
                return Unauthorized("User is not authenticated.");
            }

            // Ensure the updated script is owned by the current user
            updatedScript.Author = userId;

            // Update the script
            var script = await _scriptService.UpdateUserScript(updatedScript, userId);
            if (script == null)
            {
                _logger.LogInformation("UpdateUserScript: Script with id {ScriptId} not found or access denied for user {UserId}.", id, userId);
                return NotFound("Script not found or access denied.");
            }

            _logger.LogInformation("UpdateUserScript: Script with id {ScriptId} updated by user {UserId}.", id, userId);
            return Ok(script);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating script with id {ScriptId}.", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    
    /// <summary>
    /// Deletes a script.
    /// Only the owner of the script is allowed to delete it.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScript(string id)
    {
        try
        {
            var userId = _userService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("DeleteScript: Unauthorized access attempt. User id is missing.");
                return Unauthorized("User is not authenticated.");
            }

            var isDeleted = await _scriptService.DeleteScript(id, userId);
            if (!isDeleted)
            {
                _logger.LogInformation("DeleteScript: Script with id {ScriptId} not found or access denied for user {UserId}.", id, userId);
                return NotFound("Script not found or access denied.");
            }

            _logger.LogInformation("DeleteScript: Script with id {ScriptId} deleted by user {UserId}.", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting script with id {ScriptId}.", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}