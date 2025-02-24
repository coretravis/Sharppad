namespace SharpPad.Shared.Models.AI;


/// <summary>
/// Represents a request for the code assistant.
/// </summary>
public class CodeAssistantRequest
{
    /// <summary>
    /// Gets or sets the code to be processed.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language of the code.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message, if any.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
