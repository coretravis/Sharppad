using System.Net.Http.Json;
using SharpPad.Shared.Models.Auth;

namespace SharpPad.Client.Services.Auth;

/// <summary>
/// Client for handling authentication related operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthClient"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client.</param>
public class AuthClient(HttpClient httpClient) : IAuthClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <summary>
    /// Logs in the user with the provided login model.
    /// </summary>
    /// <param name="model">The login model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication token.</returns>
    public async Task<string?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
    {
        // Post the login data to the API endpoint.
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", model, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Deserialize the response into an AuthResponse object.
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.Token))
        {
            throw new InvalidOperationException("Login failed or token not returned.");
        }
        return authResponse.Token;
    }

    /// <summary>
    /// Registers a new user with the provided registration model.
    /// </summary>
    /// <param name="model">The registration model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        // Post the registration data to the API endpoint.
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", model, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Logs in the user using external login provider with the provided external login model.
    /// </summary>
    /// <param name="model">The external login model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication token.</returns>
    public async Task<string?> ExternalLoginAsync(ExternalLoginModel model, CancellationToken cancellationToken = default)
    {
        // Post the external login data to the API endpoint.
        var response = await _httpClient.PostAsJsonAsync("api/auth/externallogin", model, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Deserialize the JSON response into an AuthResponse object.
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.Token))
        {
            throw new InvalidOperationException("External login failed or token not returned.");
        }
        return authResponse.Token;
    }
}
