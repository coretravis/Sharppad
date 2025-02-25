using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SharpPad.Server.Services.Auth;
using SharpPad.Server.Services.Auth.Models;
using SharpPad.Shared.Models.Auth;

namespace SharpPad.Tests.Server.Services.Auth;

#pragma warning disable
/// <summary>
/// A simple fake HTTP message handler that allows you to inject custom responses.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
    {
        _handlerFunc = handlerFunc;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_handlerFunc(request));
    }
}

public class AuthServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private HttpClient _httpClient;

    public AuthServiceTests()
    {
        // Setup UserManager mock (using a dummy IUserStore)
        var store = new Mock<IUserStore<IdentityUser>>();

        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        // Setup SignInManager mock
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            userClaimsPrincipalFactory.Object,
            null, null, null, null);

        // Setup JwtSettings
        _jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "super-secret-key-that-is-at-least-32-chars",
            ExpiryInMinutes = 60,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        });

        // Setup HttpClientFactory with a default client.
        _httpClient = new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient);
    }

    /// <summary>
    /// Helper method to create an instance of AuthService.
    /// </summary>
    private AuthService CreateAuthService() =>
        new AuthService(_userManagerMock.Object,
                        _signInManagerMock.Object,
                        _jwtSettings,
                        _httpClientFactoryMock.Object,
                        Mock.Of<ILogger<AuthService>>());

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        var loginModel = new LoginModel { Username = "nonexistent", Password = "password" };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginModel.Username))
                        .ReturnsAsync((IdentityUser)null);

        var authService = CreateAuthService();

        // Act
        var token = await authService.LoginAsync(loginModel);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordCheckFails()
    {
        // Arrange
        var loginModel = new LoginModel { Username = "user", Password = "wrongpassword" };
        var user = new IdentityUser { UserName = loginModel.Username, Id = "user-id" };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginModel.Username))
                        .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginModel.Password, false))
                          .ReturnsAsync(SignInResult.Failed);

        var authService = CreateAuthService();

        // Act
        var token = await authService.LoginAsync(loginModel);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginModel = new LoginModel { Username = "user", Password = "correctpassword" };
        var user = new IdentityUser { UserName = loginModel.Username, Id = "user-id" };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginModel.Username))
                        .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginModel.Password, false))
                          .ReturnsAsync(SignInResult.Success);

        var authService = CreateAuthService();

        // Act
        var token = await authService.LoginAsync(loginModel);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
    }

    #endregion

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_WhenUserCreatedSuccessfully()
    {
        // Arrange
        var registerModel = new RegisterModel
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "password123"
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerModel.Password))
                        .ReturnsAsync(IdentityResult.Success);

        var authService = CreateAuthService();

        // Act
        var result = await authService.RegisterAsync(registerModel);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenUserCreationFails()
    {
        // Arrange
        var registerModel = new RegisterModel
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "password123"
        };

        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Error creating user." });
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerModel.Password))
                        .ReturnsAsync(failedResult);

        var authService = CreateAuthService();

        // Act
        var result = await authService.RegisterAsync(registerModel);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region ExternalLoginAsync Tests

    [Fact]
    public async Task ExternalLoginAsync_ReturnsNull_WhenModelIsNull()
    {
        // Arrange
        var authService = CreateAuthService();

        // Act
        var token = await authService.ExternalLoginAsync(null);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task ExternalLoginAsync_ReturnsNull_WhenProviderOrTokenIsMissing()
    {
        // Arrange
        var model = new ExternalLoginModel { Provider = "", ExternalAccessToken = "" };
        var authService = CreateAuthService();

        // Act
        var token = await authService.ExternalLoginAsync(model);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task ExternalLoginAsync_ReturnsNull_WhenProviderIsUnknown()
    {
        // Arrange
        var model = new ExternalLoginModel { Provider = "Unknown", ExternalAccessToken = "token" };
        var authService = CreateAuthService();

        // Act
        var token = await authService.ExternalLoginAsync(model);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task ExternalLoginAsync_ReturnsNull_WhenGoogleTokenValidationFails()
    {
        // Arrange
        var model = new ExternalLoginModel { Provider = "Google", ExternalAccessToken = "invalid-token" };

        // Create a fake HTTP client that returns a BadRequest for the Google token info endpoint.
        var fakeHandler = new FakeHttpMessageHandler(request =>
            new HttpResponseMessage(HttpStatusCode.BadRequest));
        var httpClient = new HttpClient(fakeHandler);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                              .Returns(httpClient);

        var authService = CreateAuthService();

        // Act
        var token = await authService.ExternalLoginAsync(model);

        // Assert
        Assert.Null(token);
    }



    [Fact]
    public async Task ExternalLoginAsync_ReturnsToken_ForValidGitHubToken_NewUserCreated()
    {
        // Arrange
        var model = new ExternalLoginModel { Provider = "GitHub", ExternalAccessToken = "valid-token" };
        var githubResponse = new GitHubUserInfo { Id = 123, Email = "githubuser@example.com" };

        // Fake HTTP client for GitHub.
        var fakeHandler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri!.ToString().Contains("api.github.com"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(githubResponse)
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        var httpClient = new HttpClient(fakeHandler);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                              .Returns(httpClient);

        // Simulate no user exists for this external login.
        _userManagerMock.Setup(x => x.FindByLoginAsync("GitHub", githubResponse.Id.ToString()))
                        .ReturnsAsync((IdentityUser)null);
        _userManagerMock.Setup(x => x.FindByEmailAsync(githubResponse.Email))
                        .ReturnsAsync((IdentityUser)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>()))
                        .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddLoginAsync(It.IsAny<IdentityUser>(), It.IsAny<UserLoginInfo>()))
                        .ReturnsAsync(IdentityResult.Success);

        var authService = CreateAuthService();

        // Act
        var tokenResult = await authService.ExternalLoginAsync(model);

        // Assert
        Assert.False(string.IsNullOrEmpty(tokenResult?.Token));
    }
    #endregion
}
