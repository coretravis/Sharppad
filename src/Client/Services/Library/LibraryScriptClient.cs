using SharpPad.Client.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SharpPad.Client.Services.Library;

/// <summary>
/// Client for interacting with the library scripts API.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LibraryScriptClient"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client.</param>
/// <param name="localStorage">The local storage.</param>
public class LibraryScriptClient(HttpClient httpClient, ISimpleStorage localStorage) : ILibraryScriptClient
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

    /// <summary>
    /// Gets a library script by its ID.
    /// </summary>
    /// <param name="id">The ID of the script.</param>
    /// <param name="includeCode">A flag indicating whether to include the script code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The library script.</returns>
    public async Task<LibraryScript> GetScriptByIdAsync(string id, bool includeCode, CancellationToken cancellationToken = default)
    {
        await EnsureBearerTokenSet();
        var url = $"api/LibraryScripts/{id}?includeCode={includeCode}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var script = await response.Content.ReadFromJsonAsync<LibraryScript>(cancellationToken: cancellationToken);
        return script ?? throw new InvalidOperationException("Response deserialization returned null.");
    }

    /// <summary>
    /// Gets all user scripts.
    /// </summary>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of user scripts.</returns>
    public async Task<IEnumerable<LibraryScript>> GetAllUserScriptsAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        await EnsureBearerTokenSet();
        var url = $"api/LibraryScripts?skip={skip}&take={take}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var scripts = await response.Content.ReadFromJsonAsync<IEnumerable<LibraryScript>>(cancellationToken: cancellationToken);
        return scripts ?? new List<LibraryScript>();
    }

    /// <summary>
    /// Gets all public scripts.
    /// </summary>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of public scripts.</returns>
    public async Task<IEnumerable<LibraryScript>> GetAllPublicScriptsAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var url = $"api/LibraryScripts/public?skip={skip}&take={take}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var scripts = await response.Content.ReadFromJsonAsync<IEnumerable<LibraryScript>>(cancellationToken: cancellationToken);
        return scripts ?? new List<LibraryScript>();
    }

    /// <summary>
    /// Searches user scripts by a search term.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of user scripts matching the search term.</returns>
    public async Task<IEnumerable<LibraryScript>> SearchUserScriptsAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken = default)
    {
        await EnsureBearerTokenSet();
        var url = $"api/LibraryScripts/search?searchTerm={Uri.EscapeDataString(searchTerm)}&skip={skip}&take={take}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var scripts = await response.Content.ReadFromJsonAsync<IEnumerable<LibraryScript>>(cancellationToken: cancellationToken);
        return scripts ?? new List<LibraryScript>();
    }

    /// <summary>
    /// Searches public scripts by a search term.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of public scripts matching the search term.</returns>
    public async Task<IEnumerable<LibraryScript>> SearchPublicScriptsAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken = default)
    {
        var url = $"api/LibraryScripts/search/public?searchTerm={Uri.EscapeDataString(searchTerm)}&skip={skip}&take={take}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var scripts = await response.Content.ReadFromJsonAsync<IEnumerable<LibraryScript>>(cancellationToken: cancellationToken);
        return scripts ?? new List<LibraryScript>();
    }

    /// <summary>
    /// Creates a new library script.
    /// </summary>
    /// <param name="script">The library script to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created library script.</returns>
    public async Task<LibraryScript> CreateScriptAsync(LibraryScript script, CancellationToken cancellationToken = default)
    {
        await EnsureBearerTokenSet();
        ArgumentNullException.ThrowIfNull(script);
        var response = await _httpClient.PostAsJsonAsync("api/LibraryScripts", script, cancellationToken);
        response.EnsureSuccessStatusCode();
        var createdScript = await response.Content.ReadFromJsonAsync<LibraryScript>(cancellationToken: cancellationToken);
        return createdScript ?? throw new InvalidOperationException("Response deserialization returned null.");
    }

    /// <summary>
    /// Updates an existing library script.
    /// </summary>
    /// <param name="id">The ID of the script to update.</param>
    /// <param name="script">The updated library script.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated library script.</returns>
    public async Task<LibraryScript> UpdateScriptAsync(string id, LibraryScript script, CancellationToken cancellationToken = default)
    {
        await EnsureBearerTokenSet();
        ArgumentNullException.ThrowIfNull(script);
        var response = await _httpClient.PutAsJsonAsync($"api/LibraryScripts/{id}", script, cancellationToken);
        response.EnsureSuccessStatusCode();
        var updatedScript = await response.Content.ReadFromJsonAsync<LibraryScript>(cancellationToken: cancellationToken);
        return updatedScript ?? throw new InvalidOperationException("Response deserialization returned null.");
    }

    /// <summary>
    /// Deletes a library script by its ID.
    /// </summary>
    /// <param name="id">The ID of the script to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the script was deleted successfully, false otherwise.</returns>
    public async Task<bool> DeleteScriptAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureBearerTokenSet();
        var response = await _httpClient.DeleteAsync($"api/LibraryScripts/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return true;
    }
}
