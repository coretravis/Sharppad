using System.Text.Json.Serialization;

namespace SharpPad.Server.Services.Auth.Models;

/// <summary>
/// Represents the user information returned from GitHub.
/// </summary>
public class GitHubUserInfo
{
    /// <summary>
    /// The GitHub user id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The email address of the GitHub user.
    /// Note: This may be null or empty if the email is not public.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The GitHub username.
    /// </summary>
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}
