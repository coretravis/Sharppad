using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SharpPad.Server.Services.AI;

public class CodeAssistantService(HttpClient httpClient, IConfiguration configuration, ILogger<CodeAssistantService> logger) : ICodeAssistantService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CodeAssistantService> _logger = logger;
    private readonly string _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");
    private readonly string _openAiApiUrl = configuration["OpenAI:ApiUrl"] ?? "http://localhost:1337/v1/chat/completions";

    // Helper method to send prompt to the OpenAI API.
    private async Task<string> CallOpenAiApiAsync(string prompt)
    {
        try
        {
            _logger.LogInformation("Calling OpenAI API with prompt: {Prompt}", prompt);

            // Create your request 
            var requestBody = new
            {
                model = "llama3.2-1b-instruct", // or your model choice
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful code assistant." },
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug("Request body serialized to JSON: {Json}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add the API key authorization header.
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
            _httpClient.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "yes"); // todo: remove this in production, its just for testing

            _logger.LogInformation("Sending request to OpenAI API at {Url}", _openAiApiUrl);
            var response = await _httpClient.PostAsync(_openAiApiUrl, content);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Received response from OpenAI API with status code {StatusCode}", response.StatusCode);

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response JSON: {Json}", responseJson);

            // Deserialize the response (adjust the deserialization according to OpenAI's response schema)
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            // For example, extracting the assistant reply:
            var reply = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            _logger.LogInformation("Extracted reply from OpenAI API response: {Reply}", reply);

            return reply ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<string> ExplainCodeAsync(string code, string language)
    {
        var prompt = $"Explain the following {language} code:\n\n{code}";
        return await CallOpenAiApiAsync(prompt);
    }

    public async Task<string> FixCodeErrorAsync(string code, string errorMessage, string language)
    {
        var prompt = $"Explain the following {language} code error:{errorMessage}\n\n that occured in the code:\n\n{code}";
        return await CallOpenAiApiAsync(prompt);
    }

    public async Task<string> OptimizeCodeAsync(string code, string language)
    {
        var prompt = $"Optimize and format the following {language} code for readability and performance:\n\n{code}";
        return await CallOpenAiApiAsync(prompt);
    }

    public async Task<string> AddDocumentationAsync(string code, string language)
    {
        var prompt = $"Add clear and concise documentation comments to the following {language} code:\n\n{code}";
        return await CallOpenAiApiAsync(prompt);
    }

    public async Task<string> AnswerQuestionAsync(string code, string language, string question)
    {
        var prompt = $"Given the following {language} code:\n\n{code}\n\nAnswer the question: {question}";
        return await CallOpenAiApiAsync(prompt);
    }
}