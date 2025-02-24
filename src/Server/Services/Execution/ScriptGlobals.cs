using SharpPad.Server.Services.Execution.Storage;

namespace SharpPad.Server.Services.Execution;

/// <summary>
/// Represents the global variables available in a script.
/// </summary>
public class ScriptGlobals
{
    /// <summary>
    /// The file service.
    /// </summary>
    public IFileService? File { get; set; }
}