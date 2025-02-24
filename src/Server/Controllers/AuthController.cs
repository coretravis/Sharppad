using Microsoft.AspNetCore.Mvc;
using SharpPad.Server.Services.Auth;
using SharpPad.Server.Services.Auth.Models;
using SharpPad.Shared.Models.Auth;
using System.Net.Http.Headers;

namespace SharpPad.Server.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related requests.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </remarks>
    /// <param name="authService">The authentication service.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
    {

        private readonly IAuthService _authService = authService;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Handles the login request.
        /// </summary>
        /// <param name="model">The login model.</param>
        /// <returns>The result of the login operation.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _authService.LoginAsync(model);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new { Token = token });
        }

        /// <summary>
        /// Handles the registration request.
        /// </summary>
        /// <param name="model">The registration model.</param>
        /// <returns>The result of the registration operation.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            // Register the user
            var result = await _authService.RegisterAsync(model);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Registration successful.");
        }

        /// <summary>
        /// Handles the external login request.
        /// </summary>
        /// <param name="model">The external login model.</param>
        /// <returns>The result of the external login operation.</returns>
        [HttpPost("externallogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ExternalLoginAsync(model);
            if (string.IsNullOrEmpty(result?.Token))
            {
                return Unauthorized("External authentication failed.");
            }

            return Ok(new { Token = result.Token, Username = result.Username });
        }


        /// <summary>
        /// GitHub callback endpoint – exchanges GitHub code for an access token,
        /// then logs in (or registers) the user via external login.
        /// </summary>
        /// <param name="code">The GitHub OAuth code.</param>
        /// <param name="state">The state value (optional).</param>
        /// <returns>A JWT token if successful.</returns>
        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback([FromQuery] string code)
        {
            // todo: refactor this into an External Login Service
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Missing code.");
            }

            string gitHubClientId = _configuration["Authentication:GitHub:ClientId"] ?? throw new Exception("Missing GitHub client ID"); // "Iv23linnNscLVOV2vpLH";
            string gitHubClientSecret = _configuration["Authentication:GitHub:ClientSecret"] ?? throw new Exception("Missing GitHub client secret"); //"0716ee5c210015f4b0a32360a99e9e8472dc56e5" ?? throw new Exception("Missing GitHub client secret");

            // Build the request to GitHub’s token endpoint
            var tokenRequestUrl = "https://github.com/login/oauth/access_token";
            var requestBody = new Dictionary<string, string>
            {
                {"client_id", gitHubClientId },
                {"client_secret", gitHubClientSecret },
                {"code", code }
            };

            using var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, tokenRequestUrl)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            // Ask for JSON response
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Error exchanging code for token.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>();
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return BadRequest("Invalid token response from GitHub.");
            }

            //call your external login logic
            var externalLoginModel = new ExternalLoginModel
            {
                Provider = "GitHub",
                ExternalAccessToken = tokenResponse.AccessToken
            };

            var result = await _authService.ExternalLoginAsync(externalLoginModel);

            if (string.IsNullOrEmpty(result?.Token))
            {
                return Unauthorized("External authentication failed.");
            }

            return Ok(new { Token = result.Token, Username = result.Username });
        }
    }

}

