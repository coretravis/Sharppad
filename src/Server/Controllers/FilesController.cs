using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SharpPad.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly long _maxFileSize;
    private readonly string _storagePath;

    public FilesController(IConfiguration configuration)
    {
        _configuration = configuration;
        // Read the maximum allowed file size (in bytes) from configuration; default to 1MB (1_048_576 bytes) if not set.
        _maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSize", 1_048_576);
        // Read the storage path from configuration or use the "UploadedFiles" folder.
        _storagePath = _configuration.GetValue<string>("FileStorage:Path") ?? "UploadedFiles";

        // Ensure the storage folder exists.
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <summary>
    /// Downloads a file from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the file to be downloaded.</param>
    /// <returns>A file result containing the downloaded file.</returns>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile([FromQuery] string filePath)
    {
        // Check if the file exists at the specified path
        if (!System.IO.File.Exists(filePath))
        {
            // Return a 404 Not Found response if the file does not exist
            return NotFound();
        }

        // Create a memory stream to hold the file contents
        var memory = new MemoryStream();

        // Open the file in read-only mode and copy its contents to the memory stream
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            await stream.CopyToAsync(memory);
        }

        // Reset the memory stream position to the beginning
        memory.Position = 0;

        // Determine the content type of the file (defaulting to octet-stream)
        var contentType = "application/octet-stream";

        // Return the file contents as a response with the determined content type
        return File(memory, contentType, Path.GetFileName(filePath));
    }

    /// <summary>
    /// Handles file uploads.
    /// </summary>
    /// <param name="file">The file to be uploaded.</param>
    /// <returns>A JSON response containing the file path.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        // Check if a file is provided
        if (file == null || file.Length == 0)
        {
            // Return an error response if no file is provided
            return BadRequest("No file uploaded.");
        }

        // Check if the file size exceeds the maximum allowed size
        if (file.Length > _maxFileSize)
        {
            // Return an error response if the file size exceeds the maximum allowed size
            return BadRequest($"File size exceeds the maximum allowed size of {_maxFileSize} bytes.");
        }

        // Create a unique file name to avoid conflicts
        var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        // Save the file to the storage path
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            // Copy the file contents to the stream
            await file.CopyToAsync(stream);
        }

        // Return the file path as JSON
        return Ok(new { filePath });
    }
}