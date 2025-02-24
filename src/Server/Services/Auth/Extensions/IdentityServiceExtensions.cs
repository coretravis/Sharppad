using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using SharpPad.Server.Data;

namespace SharpPad.Server.Services.Auth.Extensions;

/// <summary>
/// Extension methods for adding identity authentication to the service collection.
/// </summary>
public static class IdentityServiceExtensions
{
    /// <summary>
    /// Adds identity authentication to the service collection.
    /// </summary>
    /// <typeparam name="JwtOptions">The type of the JWT options.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddIdentityAuthentication<JwtOptions>(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the ApplicationDbContext (an IdentityDbContext) using MS SQL.
        string? connectionString = configuration["ConnectionStrings:DefaultConnection"];
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Add ASP.NET Core Identity.
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
            
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure Authentication and add external providers.
        services.AddAuthentication(options =>
        {
            // Set the default scheme to JWT Bearer.
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        // GitHub external login via OAuth.
        .AddOAuth("GitHub", options =>
        {
            options.ClientId = configuration["Authentication:GitHub:ClientId"]
            ?? throw new ArgumentNullException("Authentication:GitHub:ClientId");
            options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"]
            ?? throw new ArgumentNullException("Authentication:GitHub:ClientSecret");
            options.CallbackPath = "/signin-github";
            options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            options.TokenEndpoint = "https://github.com/login/oauth/access_token";
            options.UserInformationEndpoint = "https://api.github.com/user";
            options.Scope.Add("user:email");
            options.ClaimActions.MapJsonKey("urn:github:login", "login", "string");
            options.ClaimActions.MapJsonKey("urn:github:id", "id", "string");
            options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url", "string");
            options.SaveTokens = true;
        })
        // JWT Bearer authentication.
        .AddJwtBearer(options =>
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured."));
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured."),
                ValidAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured."),
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };
        });
        return services;
    }
}
