namespace SharpPad.Client.Services.Library;

/// <summary>
/// Defines the interface for a library script client.
/// </summary>
public interface ILibraryScriptClient
{
    /// <summary>
    /// Retrieves a library script by its ID.
    /// </summary>
    /// <param name="id">The ID of the script to retrieve.</param>
    /// <param name="includeCode">Whether to include the script code in the response.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The retrieved library script.</returns>
    Task<LibraryScript> GetScriptByIdAsync(string id, bool includeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of library scripts owned by the current user.
    /// </summary>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of library scripts owned by the current user.</returns>
    Task<IEnumerable<LibraryScript>> GetAllUserScriptsAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of public library scripts.
    /// </summary>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of public library scripts.</returns>
    Task<IEnumerable<LibraryScript>> GetAllPublicScriptsAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for library scripts owned by the current user.
    /// </summary>
    /// <param name="searchTerm">The search term to use.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of library scripts owned by the current user that match the search term.</returns>
    Task<IEnumerable<LibraryScript>> SearchUserScriptsAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for public library scripts.
    /// </summary>
    /// <param name="searchTerm">The search term to use.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of public library scripts that match the search term.</returns>
    Task<IEnumerable<LibraryScript>> SearchPublicScriptsAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new library script.
    /// </summary>
    /// <param name="script">The script to create.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The created library script.</returns>
    Task<LibraryScript> CreateScriptAsync(LibraryScript script, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing library script.
    /// </summary>
    /// <param name="id">The ID of the script to update.</param>
    /// <param name="script">The updated script.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The updated library script.</returns>
    Task<LibraryScript> UpdateScriptAsync(string id, LibraryScript script, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a library script.
    /// </summary>
    /// <param name="id">The ID of the script to delete.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>True if the script was deleted successfully, false otherwise.</returns>
    Task<bool> DeleteScriptAsync(string id, CancellationToken cancellationToken = default);
}
