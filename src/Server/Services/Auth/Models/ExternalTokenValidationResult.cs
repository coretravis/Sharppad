namespace SharpPad.Server.Services.Auth.Models;

/// <summary>
/// Represents the result of validating an external access token.
/// </summary>
public class ExternalTokenValidationResult
{
    /// <summary>
    /// Indicates whether the token is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The external user id obtained from the provider.
    /// </summary>
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// The email address associated with the external account.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
