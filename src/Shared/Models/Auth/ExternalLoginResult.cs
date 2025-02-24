namespace SharpPad.Shared.Models.Auth;

/// <summary>
/// Represents the result of an external login.
/// </summary>
public class ExternalLoginResult
{
    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;
}
