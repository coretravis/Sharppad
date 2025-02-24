using SharpPad.Server.Services.Library.Models;

namespace SharpPad.Server.Services.Library;

/// <summary>
/// The script library service interface.
/// </summary>
public interface IScriptLibraryService
{
    /// <summary>
    /// Retrieves a script by its id. If the script is private, only its owner (matching ownerId) can access it.
    /// </summary>
    /// <param name="scriptId">The script identifier (as a string representing a Guid).</param>
    /// <param name="includeCode">If false, the Code property is omitted from the returned result.</param>
    /// <param name="ownerId">The identifier of the user requesting the script.</param>
    /// <returns>The matching <see cref="LibraryScript"/>, or null if not found or access is denied.</returns>
    Task<LibraryScript?> GetScriptById(string scriptId, bool includeCode, string ownerId);

    /// <summary>
    /// Creates a new library script.
    /// </summary>
    /// <param name="libraryScript">The script to create.</param>
    /// <returns>The created <see cref="LibraryScript"/>.</returns>
    Task<LibraryScript> CreateScript(LibraryScript libraryScript);

    /// <summary>
    /// Updates an existing library script. Only the owner (as per ownerId) can update a script.
    /// </summary>
    /// <param name="updatedScript">The script with updated values (its Id must match an existing script).</param>
    /// <param name="ownerId">The identifier of the user attempting the update.</param>
    /// <returns>
    /// The updated <see cref="LibraryScript"/> if successful; otherwise, null (if the script does not exist or if access is denied).
    /// </returns>
    Task<LibraryScript?> UpdateUserScript(LibraryScript updatedScript, string ownerId);

    /// <summary>
    /// Retrieves all scripts that owned by the owner.
    /// </summary>
    /// <param name="ownerId">The identifier of the user requesting the scripts.</param>
    /// <returns>A list of <see cref="LibraryScript"/> objects.</returns>
    Task<List<LibraryScript>> GetAllUserScripts(string ownerId);

    /// <summary>
    /// Retrieves scripts with pagination. Only scripts owned by the owner are returned.
    /// </summary>
    /// <param name="ownerId">The identifier of the user requesting the scripts.</param>
    /// <param name="skip">The number of scripts to skip.</param>
    /// <param name="take">The number of scripts to return.</param>
    /// <returns>A paged list of <see cref="LibraryScript"/> objects.</returns>
    Task<List<LibraryScript>> GetAllUserScripts(string ownerId, int skip, int take);

    /// <summary>
    /// Retrieves all public scripts with pagination
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<List<LibraryScript>> GetAllPublicScripts(int skip, int take);

    /// <summary>
    /// Searches for scripts by title, description, or tags.
    /// Scripts are only returned if owned by the owner.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="ownerId">The identifier of the user performing the search.</param>
    /// <param name="skip">The number of matching scripts to skip.</param>
    /// <param name="take">The number of matching scripts to return.</param>
    /// <returns>A list of <see cref="LibraryScript"/> objects matching the search criteria.</returns>
    Task<List<LibraryScript>> SearchUserScripts(string searchTerm, string ownerId, int skip, int take);

    /// <summary>
    /// Searches for all public scripts by title, description, or tags with pagination.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="skip">The number of matching scripts to skip.</param>
    /// <param name="take">The number of matching scripts to return.</param>
    /// <returns>A list of <see cref="LibraryScript"/> objects matching the search criteria.</returns>
    Task<List<LibraryScript>> SearchPublicScripts(string searchTerm, int skip, int take);

    /// <summary>
    /// Deletes a script owned by the owner.
    /// </summary>
    /// <param name="scriptId">The identifier of the script to delete.</param>
    /// <param name="ownerId">The identifier of the user deleting the script.</param>
    /// <returns>True if the script was deleted; otherwise, false.</returns>
    Task<bool> DeleteScript(string scriptId, string ownerId);
}
