using System.Text.Json.Serialization;

namespace SharpPad.Server.Services.Auth.Models;


/// <summary>
/// Represents the information about a GitHub email.
/// </summary>
public class GitHubEmailInfo
{
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the email is primary.
    /// </summary>
    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is verified.
    /// </summary>
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    /// <summary>
    /// Gets or sets the visibility of the email.
    /// </summary>
    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }
}
