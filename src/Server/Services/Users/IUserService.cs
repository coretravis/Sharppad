namespace SharpPad.Server.Services.Users;

/// <summary>
/// Represents a user service.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    /// <returns>The user ID.</returns>
    string? GetUserId();
}
