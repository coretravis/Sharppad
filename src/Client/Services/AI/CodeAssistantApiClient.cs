using SharpPad.Client.Services.Storage;
using SharpPad.Shared.Models.AI;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SharpPad.Client.Services.AI;

/// <summary>
/// Client for interacting with the Code Assistant API.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CodeAssistantApiClient"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client to use for API requests.</param>
/// <param name="localStorage">The local storage service for retrieving the authentication token.</param>
public class CodeAssistantApiClient(HttpClient httpClient, ISimpleStorage localStorage) : ICodeAssistantApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ISimpleStorage _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    private bool _tokenSet;

    private async Task EnsureBearerTokenSet()
    {
        if (!_tokenSet)
        {
            var token = await _localStorage.GetAsync("authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            _tokenSet = true;
        }
    }

    
    public async Task<string> ExplainCodeAsync(string code, string language)
    {
        await EnsureBearerTokenSet();
        var request = new CodeAssistantRequest
        {
            Code = code,
            Language = language
        };

        var response = await _httpClient.PostAsJsonAsync("api/CodeAssistant/explain", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CodeAssistantResponse>();
        return result?.Result ?? string.Empty;
    }

    public async Task<string> FixCodeAsync(string code, string errorMessage, string language)
    {
        await EnsureBearerTokenSet();
        var request = new CodeAssistantRequest
        {
            Code = code,
            Language = language,
            ErrorMessage = errorMessage
        };

        var response = await _httpClient.PostAsJsonAsync("api/CodeAssistant/fix", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CodeAssistantResponse>();
        return result?.Result ?? string.Empty;
    }

    public async Task<string> OptimizeCodeAsync(string code, string language)
    {
        await EnsureBearerTokenSet();
        var request = new CodeAssistantRequest
        {
            Code = code,
            Language = language
        };

        var response = await _httpClient.PostAsJsonAsync("api/CodeAssistant/optimize", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CodeAssistantResponse>();
        return result?.Result ?? string.Empty;
    }


    public async Task<string> AddDocumentationAsync(string code, string language)
    {
        await EnsureBearerTokenSet();
        var request = new CodeAssistantRequest
        {
            Code = code,
            Language = language
        };

        var response = await _httpClient.PostAsJsonAsync("api/CodeAssistant/document", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CodeAssistantResponse>();
        return result?.Result ?? string.Empty;
    }

    public async Task<string> AnswerQuestionAsync(string code, string language, string question)
    {
        await EnsureBearerTokenSet();
        var request = new QuestionRequest
        {
            Code = code,
            Language = language,
            Question = question
        };

        var response = await _httpClient.PostAsJsonAsync("api/CodeAssistant/question", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CodeAssistantResponse>();
        return result?.Result ?? string.Empty;
    }
}
