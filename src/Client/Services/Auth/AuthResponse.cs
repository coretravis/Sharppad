namespace SharpPad.Client.Services.Auth;

public class AuthResponse
{
    /// <summary>
    /// Gets or sets the JWT token returned by the API.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
