namespace SharpPad.Server.Services.Auth.Models;

/// <summary>
/// Represents the JWT settings from configuration.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The issuer of the token.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience for the token.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The secret key used to sign the token.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The expiration time (in minutes) for the token.
    /// </summary>
    public int ExpiryInMinutes { get; set; }
}
