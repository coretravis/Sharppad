using System.Net.Http.Json;
using SharpPad.Shared.Models.Auth;

namespace SharpPad.Client.Services.Auth;

/// <summary>
/// Represents a client for authenticating users.
/// </summary>
public interface IAuthClient
{
    /// <summary>
    /// Logs in a user using local credentials and returns a JWT token.
    /// </summary>
    Task<string?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs in a user using an external provider (e.g. Google, GitHub, Twitter) and returns a JWT token.
    /// </summary>
    Task<string?> ExternalLoginAsync(ExternalLoginModel model, CancellationToken cancellationToken = default);
}
