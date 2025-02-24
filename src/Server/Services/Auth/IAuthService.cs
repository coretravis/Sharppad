using Microsoft.AspNetCore.Identity;
using SharpPad.Shared.Models.Auth;

namespace SharpPad.Server.Services.Auth;

/// <summary>
/// Defines the interface for authentication services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Handles external login requests.
    /// </summary>
    /// <param name="model">The external login model.</param>
    /// <returns>The authentication token if successful, otherwise null.</returns>
    Task<ExternalLoginResult?> ExternalLoginAsync(ExternalLoginModel model);

    /// <summary>
    /// Handles login requests.
    /// </summary>
    /// <param name="model">The login model.</param>
    /// <returns>The authentication token if successful, otherwise null.</returns>
    Task<string?> LoginAsync(LoginModel model);

    /// <summary>
    /// Handles registration requests.
    /// </summary>
    /// <param name="model">The registration model.</param>
    /// <returns>The result of the registration operation.</returns>
    Task<IdentityResult> RegisterAsync(RegisterModel model);
}