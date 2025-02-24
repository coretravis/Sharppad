using System.Security.Claims;

namespace SharpPad.Server.Services.Users;

public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? GetUserId()
    {
        // Get the user id from the claims
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
        return userId;
    }
}