namespace SharpPad.Shared.Models.Auth;

/// <summary>
/// Represents an external login model.
/// </summary>
public class ExternalLoginModel
{
    /// <summary>
    /// Gets or sets the provider.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external access token.
    /// </summary>
    public string ExternalAccessToken { get; set; } = string.Empty;
}
