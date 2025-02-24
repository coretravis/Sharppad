namespace SharpPad.Shared.Models.AI;

/// <summary>
/// Represents a question request for the AI.
/// </summary>
public class QuestionRequest : CodeAssistantRequest
{
    /// <summary>
    /// Gets or sets the question.
    /// </summary>
    public string Question { get; set; } = string.Empty;
}
