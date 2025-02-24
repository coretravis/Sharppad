using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharpPad.Server.Services.Auth.Models;
using SharpPad.Shared.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SharpPad.Server.Services.Auth;

public class AuthService(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IOptions<JwtSettings> jwtSettings,
    IHttpClientFactory httpClientFactory, ILogger<AuthService> logger) : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(); // Used for external token validation

    public async Task<string?> LoginAsync(LoginModel model)
    {
        
        _logger.LogInformation("Logging in user {Username}", model.Username);
        // Find the user by username.
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            _logger.LogInformation("User {Username} not found", model.Username);
            return null;
        }

        // Check the password.
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogInformation("Invalid password for user {Username}", model.Username);
            return null;
        }

        // Generate and return a JWT token.
        return GenerateJwtToken(user);
    }

    public async Task<IdentityResult> RegisterAsync(RegisterModel model)
    {
        // Create a new user.
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email
        };

        // Create the user.
        var result = await _userManager.CreateAsync(user, model.Password);
        return result;
    }

    public async Task<ExternalLoginResult?> ExternalLoginAsync(ExternalLoginModel model)
    {        
        if (model == null || string.IsNullOrEmpty(model.Provider) || string.IsNullOrEmpty(model.ExternalAccessToken))
            return null;

        _logger.LogInformation("Attempting external login for provider {Provider}", model.Provider);

        ExternalTokenValidationResult? validationResult = null;

        // Validate the external access token.
        if (model.Provider.Equals("GitHub", StringComparison.OrdinalIgnoreCase))
        {
            validationResult = await VerifyGitHubTokenAsync(model.ExternalAccessToken);
        }
        else
        {
            return null;
        }

        if (validationResult == null || !validationResult.IsValid)
            return null;

        // Create a UserLoginInfo for the external login.
        var info = new UserLoginInfo(model.Provider, validationResult.ExternalUserId, model.Provider);

        // Check if a user already exists for this external login.
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user == null)
        {
            _logger.LogInformation("User not found for external login provider {Provider}", model.Provider);
            // Optionally, find by email if the user exists.
            user = await _userManager.FindByEmailAsync(validationResult.Email);
            if (user == null)
            {
                // Create a new user.
                user = new IdentityUser
                {
                    UserName = validationResult.ExternalUserId,
                    Email = validationResult.Email
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return null;
                }
            }
            // Link the external login to the user.
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return null;
            }
        }
        _logger.LogInformation("User found for external login provider {Provider}", model.Provider);

        // Generate and return a JWT token.
        return new ExternalLoginResult { Token = GenerateJwtToken(user), Username = user?.UserName ?? string.Empty };
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        // Create the JWT token.
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Create the token.
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<ExternalTokenValidationResult?> VerifyGitHubTokenAsync(string accessToken)
    {
        // Request the main GitHub user info.
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("SharpPad");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadFromJsonAsync<GitHubUserInfo>();
        if (payload == null)
            return null;

        string? email = payload.Email;

        // If email is null or empty, try to fetch the user's emails.
        if (string.IsNullOrEmpty(email))
        {
            var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            emailRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            emailRequest.Headers.UserAgent.ParseAdd("SharpPad");

            var emailResponse = await _httpClient.SendAsync(emailRequest);
            if (emailResponse.IsSuccessStatusCode)
            {
                var emails = await emailResponse.Content.ReadFromJsonAsync<List<GitHubEmailInfo>>();
                // Get the primary and verified email.
                email = emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email;
            }
        }

        if (string.IsNullOrEmpty(email))
        {
            email = $"{payload.Id}@github.com";
        }

        return new ExternalTokenValidationResult
        {
            IsValid = true,
            ExternalUserId = payload.Id.ToString(),
            Email = email
        };
    }
    
}
