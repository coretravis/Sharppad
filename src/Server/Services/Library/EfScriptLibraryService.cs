using Microsoft.EntityFrameworkCore;
using SharpPad.Server.Data;
using SharpPad.Server.Services.Library.Models;

namespace SharpPad.Server.Services.Library;

/// <summary>
/// An EF Core–based implementation of IScriptLibraryService
/// </summary>
public class EfScriptLibraryService(ApplicationDbContext dbContext, ILogger<EfScriptLibraryService> logger) : IScriptLibraryService
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<EfScriptLibraryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<LibraryScript?> GetScriptById(string scriptId, bool includeCode, string ownerId)
    {
        _logger.LogInformation("Retrieving script with id {ScriptId}", scriptId);
        try
        {
            if (!Guid.TryParse(scriptId, out Guid id))
            {
                _logger.LogWarning("Invalid script id format: {ScriptId}", scriptId);
                return null;
            }

            // As required, do not return the Code property in non-GetScriptById calls.
            var script = await _dbContext.LibraryScripts
                .Include(s => s.NugetPackages)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (script == null)
            {
                _logger.LogWarning("Script with id {ScriptId} not found.", scriptId);
                return null;
            }

            // If the script is private, only the owner can access it.
            if (script.IsPrivate && script.OwnerId != ownerId)
            {
                _logger.LogWarning("Access denied for script {ScriptId} to user {OwnerId}.", scriptId, ownerId);
                return null;
            }

            // Only include the Code property if explicitly requested.
            if (!includeCode)
            {
                script.Code = string.Empty;
            }

            return script;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving script with id {ScriptId}", scriptId);
            throw;
        }
    }

    public async Task<LibraryScript> CreateScript(LibraryScript libraryScript)
    {
        if (libraryScript == null)
        {
            _logger.LogError("Attempted to create a null LibraryScript.");
            throw new ArgumentNullException(nameof(libraryScript));
        }

        try
        {
            _dbContext.LibraryScripts.Add(libraryScript);
            await _dbContext.SaveChangesAsync();

            // As required, do not return the Code property in non-GetScriptById calls.
            libraryScript.Code = string.Empty;
            _logger.LogInformation("Created LibraryScript with id {ScriptId}", libraryScript.Id);
            return libraryScript;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LibraryScript with title {Title}", libraryScript.Title);
            throw;
        }
    }

    public async Task<LibraryScript?> UpdateUserScript(LibraryScript updatedScript, string ownerId)
    {
        if (updatedScript == null)
        {
            _logger.LogError("Attempted to update a null LibraryScript.");
            throw new ArgumentNullException(nameof(updatedScript));
        }

        try
        {
            var existingScript = await _dbContext.LibraryScripts
                .Include(s => s.NugetPackages)
                .FirstOrDefaultAsync(s => s.Id == updatedScript.Id);

            if (existingScript == null)
            {
                _logger.LogWarning("Script with id {ScriptId} not found for update.", updatedScript.Id);
                return null;
            }

            if (existingScript.OwnerId != ownerId)
            {
                _logger.LogWarning("User {OwnerId} attempted to update script {ScriptId} without proper ownership.", ownerId, updatedScript.Id);
                return null;
            }

            // Update mutable properties.
            existingScript.Title = updatedScript.Title;
            existingScript.Language = updatedScript.Language;
            existingScript.CompilerVersion = updatedScript.CompilerVersion;
            existingScript.Description = updatedScript.Description;
            existingScript.Author = updatedScript.Author;
            existingScript.Tags = updatedScript.Tags;
            existingScript.IsPrivate = updatedScript.IsPrivate;
            existingScript.Code = updatedScript.Code;

            // Replace all NuGet packages.
            existingScript.NugetPackages.Clear();
            if (updatedScript.NugetPackages != null)
            {
                foreach (var pkg in updatedScript.NugetPackages)
                {
                    existingScript.NugetPackages.Add(new LibraryScriptPackage
                    {
                        Id = pkg.Id,
                        Version = pkg.Version
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            // Clear the Code property when returning.
            existingScript.Code = string.Empty;
            _logger.LogInformation("Updated LibraryScript with id {ScriptId}", updatedScript.Id);
            return existingScript;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LibraryScript with id {ScriptId}", updatedScript.Id);
            throw;
        }
    }

    public async Task<List<LibraryScript>> GetAllUserScripts(string ownerId)
    {
        _logger.LogInformation("Retrieving all scripts for owner {OwnerId}", ownerId);
        try
        {
            var scripts = await _dbContext.LibraryScripts
                .Where(s => s.OwnerId == ownerId)
                .Include(s => s.NugetPackages)
                .AsNoTracking()
                .ToListAsync();

            scripts.ForEach(s => s.Code = string.Empty);
            _logger.LogInformation("Retrieved {Count} scripts for owner {OwnerId}", scripts.Count, ownerId);
            return scripts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scripts for owner {OwnerId}", ownerId);
            throw;
        }
    }

    public async Task<List<LibraryScript>> GetAllUserScripts(string ownerId, int skip, int take)
    {
        _logger.LogInformation("Retrieving paged scripts for owner {OwnerId} (skip: {Skip}, take: {Take})", ownerId, skip, take);
        try
        {
            var scripts = await _dbContext.LibraryScripts
                .Where(s => s.OwnerId == ownerId)
                .OrderBy(s => s.Title)
                .Skip(skip)
                .Take(take)
                .Include(s => s.NugetPackages)
                .AsNoTracking()
                .ToListAsync();

            scripts.ForEach(s => s.Code = string.Empty);
            _logger.LogInformation("Retrieved {Count} paged scripts for owner {OwnerId} (skip: {Skip}, take: {Take})",
                scripts.Count, ownerId, skip, take);
            return scripts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged scripts for owner {OwnerId}", ownerId);
            throw;
        }
    }

    public async Task<List<LibraryScript>> GetAllPublicScripts(int skip, int take)
    {
        _logger.LogInformation("Retrieving paged public scripts (skip: {Skip}, take: {Take})", skip, take);
        try
        {
            var scripts = await _dbContext.LibraryScripts
                .Where(s => !s.IsPrivate)
                .OrderBy(s => s.Title)
                .Skip(skip)
                .Take(take)
                .Include(s => s.NugetPackages)
                .AsNoTracking()
                .ToListAsync();

            scripts.ForEach(s => s.Code = string.Empty);
            _logger.LogInformation("Retrieved {Count} paged public scripts (skip: {Skip}, take: {Take})",
                scripts.Count, skip, take);
            return scripts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged public scripts");
            throw;
        }
    }

    public async Task<List<LibraryScript>> SearchUserScripts(string searchTerm, string ownerId, int skip, int take)
    {
        _logger.LogInformation("Searching user scripts for owner {OwnerId} with term '{SearchTerm}'", ownerId, searchTerm);
        try
        {
            var scripts = await _dbContext.LibraryScripts
                .Where(s => s.OwnerId == ownerId &&
                            (s.Title.Contains(searchTerm) ||
                             s.Description.Contains(searchTerm) ||
                             s.Tags.Contains(searchTerm)))
                .OrderBy(s => s.Title)
                .Skip(skip)
                .Take(take)
                .Include(s => s.NugetPackages)
                .AsNoTracking()
                .ToListAsync();

            scripts.ForEach(s => s.Code = string.Empty);
            _logger.LogInformation("Searched user scripts for owner {OwnerId} with term '{SearchTerm}' and found {Count} results",
                ownerId, searchTerm, scripts.Count);
            return scripts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching user scripts for owner {OwnerId} with term '{SearchTerm}'",
                ownerId, searchTerm);
            throw;
        }
    }

    public async Task<List<LibraryScript>> SearchPublicScripts(string searchTerm, int skip, int take)
    {
        _logger.LogInformation("Searching public scripts with term '{SearchTerm}'", searchTerm);
        try
        {
            var scripts = await _dbContext.LibraryScripts
                .Where(s => !s.IsPrivate &&
                            (s.Title.Contains(searchTerm) ||
                             s.Description.Contains(searchTerm) ||
                             s.Tags.Contains(searchTerm)))
                .OrderBy(s => s.Title)
                .Skip(skip)
                .Take(take)
                .Include(s => s.NugetPackages)
                .AsNoTracking()
                .ToListAsync();

            scripts.ForEach(s => s.Code = string.Empty);
            _logger.LogInformation("Searched public scripts with term '{SearchTerm}' and found {Count} results",
                searchTerm, scripts.Count);
            return scripts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching public scripts with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<bool> DeleteScript(string scriptId, string ownerId)
    {
        _logger.LogInformation("Deleting LibraryScript with id {ScriptId}", scriptId);
        try
        {
            var script = await _dbContext.LibraryScripts.FirstOrDefaultAsync(s => s.Id == Guid.Parse(scriptId));
            if (script == null || script.OwnerId != ownerId)
            {
                _logger.LogWarning("Access denied for script {ScriptId} to user {OwnerId}.", scriptId, ownerId);
                return false;
            }

            _dbContext.LibraryScripts.Remove(script);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Deleted LibraryScript with id {ScriptId}", scriptId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting LibraryScript with id {ScriptId}", scriptId);
            throw;
        }
    }
}