namespace SharpPad.Shared.Models.Compiler;

/// <summary>
/// Represents the output type for the compiler.
/// </summary>
public enum OutputType
{
    /// <summary>
    /// Text output.
    /// </summary>
    Text,

    /// <summary>
    /// JSON output.
    /// </summary>
    Json,

    /// <summary>
    /// Collection output.
    /// </summary>
    Collection,

    /// <summary>
    /// Compilation error output.
    /// </summary>
    CompilationError,

    /// <summary>
    /// Runtime error output.
    /// </summary>
    RuntimeError,

    /// <summary>
    /// Error output.
    /// </summary>
    Error,

    /// <summary>
    /// Warning output.
    /// </summary>
    Warning,

    /// <summary>
    /// XML output.
    /// </summary>
    Xml,

    /// <summary>
    /// HTML output.
    /// </summary>
    Html,

    /// <summary>
    /// File output.
    /// </summary>
    File,

    /// <summary>
    /// Console input output.
    /// </summary>
    ConsoleInput,

    /// <summary>
    /// System output.
    /// </summary>
    System
}
