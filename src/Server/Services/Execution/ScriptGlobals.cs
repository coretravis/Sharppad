using SharpPad.Server.Services.Execution.FileSystem;

namespace SharpPad.Server.Services.Execution;

/// <summary>
/// Represents the global variables available in a script.
/// </summary>
public class ScriptGlobals
{
    /// <summary>
    /// The file service. Mimics the System.IO.File class.
    /// </summary>
    public IFileService? File { get; set; }

    /// <summary>
    /// The path service. Mimics the System.IO.Path class.
    /// </summary>
    public IPathService? Path { get; set; }

    /// <summary>
    /// The directory service. Mimics the System.IO.Directory class.
    /// </summary>
    public IDirectoryService? Directory { get; set; }
}
