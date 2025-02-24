using SharpPad.Server.Services.AI;
using SharpPad.Server.Services.Auth.Extensions;
using SharpPad.Server.Services.Auth.Models;
using SharpPad.Server.Services.Auth;
using SharpPad.Server.Services.Nugets;
using SharpPad.Server.Services.Library;
using SharpPad.Server.Services.Users;
using SharpPad.Server.Services.Execution.Storage;
using SharpPad.Server.Services.Execution.Compiler;
using Microsoft.EntityFrameworkCore;
using SharpPad.Server.Data;
using SharpPad.Server.Middleware;
using SharpPad.Server.Services.Streaming;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json, environment variables, and user secrets (in development)
builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var configuration = builder.Configuration;

// Register JWT settings from secrets in dev and env vars in prod
builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
    options.Issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
    options.ExpiryInMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "60");
    options.Audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");
});

// Add Identity and external authentication.
builder.Services.AddIdentityAuthentication<JwtSettings>(configuration);

// Register the authentication service.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register the NuGet search service
builder.Services.AddScoped<INugetPackageService, NugetPackageService>();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICompilerVersionService, CompilerVersionService>();
builder.Services.AddScoped<ICodeExecutionService, CodeExecutionService>();
builder.Services.AddScoped<IStreamingCodeExecutionService, StreamingCodeExecutionService>();
builder.Services.AddScoped<IScriptLibraryService, EfScriptLibraryService>();
builder.Services.AddScoped<IRoslynService, RoslynService>();

// Register the CodeAssistantService with HttpClient support.
builder.Services.AddHttpClient<ICodeAssistantService, CodeAssistantService>();

builder.Services.AddControllersWithViews();

// SignlR
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Use authentication and authorization.
app.UseAuthentication();
app.UseAuthorization();

// Use RequestLoggingMiddleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapRazorPages();
app.MapControllers();
app.MapHub<CodeExecutionHub>("/codeExecutionHub");
app.MapFallbackToFile("index.html");

// Migrate database automatically: This isn't safe for production, just testing and dev
using var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync();

app.Run();