using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharpPad.Client.Services.Library;
using SharpPad.Server.Data;
using SharpPad.Server.Services.Library;

namespace SharpPad.Tests.Server.Services.Library;

#region Test Model Stubs

#endregion

// Assume that EfScriptLibraryService (which implements IScriptLibraryService) is defined in your production code.
// The service is defined as:
//
// public class EfScriptLibraryService(ApplicationDbContext dbContext, ILogger<EfScriptLibraryService> logger) : IScriptLibraryService
// { ... }
//
// The tests below target its methods.

public class EfScriptLibraryServiceTests
{
    /// <summary>
    /// Creates a new ApplicationDbContext instance using the EF Core InMemory provider.
    /// A unique database name per test ensures isolation.
    /// </summary>
    private ApplicationDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Creates a logger for the service.
    /// </summary>
    private ILogger<EfScriptLibraryService> CreateLogger()
    {
        // For testing, a real logger is used; you can substitute a mock if desired.
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<EfScriptLibraryService>();
    }

    #region GetScriptById Tests

    [Fact]
    public async Task GetScriptById_InvalidGuid_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetScriptById_InvalidGuid_ReturnsNull));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Act
        var result = await service.GetScriptById("invalid-guid", includeCode: true, ownerId: "owner1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetScriptById_ScriptNotFound_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetScriptById_ScriptNotFound_ReturnsNull));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);
        string validGuid = Guid.NewGuid().ToString();

        // Act
        var result = await service.GetScriptById(validGuid, includeCode: true, ownerId: "owner1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetScriptById_PrivateScript_NotOwner_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetScriptById_PrivateScript_NotOwner_ReturnsNull));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(),
            Title = "Private Script",
            Code = "some code",
            IsPrivate = true,
            OwnerId = "owner1"
        };
        context.LibraryScripts.Add(script);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetScriptById(script.Id.ToString(), includeCode: true, ownerId: "otherOwner");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetScriptById_PrivateScript_Owner_ReturnsScriptWithCode()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetScriptById_PrivateScript_Owner_ReturnsScriptWithCode));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(),
            Title = "Private Script",
            Code = "private code",
            IsPrivate = true,
            OwnerId = "owner1"
        };
        context.LibraryScripts.Add(script);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetScriptById(script.Id.ToString(), includeCode: true, ownerId: "owner1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("private code", result!.Code);
    }

    [Fact]
    public async Task GetScriptById_PublicScript_ExcludeCode_ClearsCode()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetScriptById_PublicScript_ExcludeCode_ClearsCode));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(),
            Title = "Public Script",
            Code = "public code",
            IsPrivate = false,
            OwnerId = "owner1"
        };
        context.LibraryScripts.Add(script);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetScriptById(script.Id.ToString(), includeCode: false, ownerId: "owner1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result!.Code);
    }

    #endregion

    #region CreateScript Tests

    [Fact]
    public async Task CreateScript_NullScript_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(CreateScript_NullScript_ThrowsArgumentNullException));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateScript(null!));
    }

    [Fact]
    public async Task CreateScript_ValidScript_CreatesScriptAndClearsCode()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(CreateScript_ValidScript_CreatesScriptAndClearsCode));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(),
            Title = "New Script",
            Code = "initial code",
            IsPrivate = false,
            OwnerId = "owner1"
        };

        // Act
        var createdScript = await service.CreateScript(script);

        // Assert
        Assert.Equal(string.Empty, createdScript.Code);
        // Verify that the script was added to the database.
        var dbScript = await context.LibraryScripts.FindAsync(createdScript.Id);
        Assert.NotNull(dbScript);
    }

    #endregion

    #region UpdateUserScript Tests

    [Fact]
    public async Task UpdateUserScript_NullScript_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(UpdateUserScript_NullScript_ThrowsArgumentNullException));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateUserScript(null!, "owner1"));
    }

    [Fact]
    public async Task UpdateUserScript_ScriptNotFound_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(UpdateUserScript_ScriptNotFound_ReturnsNull));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var updatedScript = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(), // Non-existent script
            Title = "Updated Title",
            Code = "updated code",
            IsPrivate = false,
            OwnerId = "owner1"
        };

        // Act
        var result = await service.UpdateUserScript(updatedScript, "owner1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserScript_NotOwner_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(UpdateUserScript_NotOwner_ReturnsNull));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Code = "original code",
            IsPrivate = false,
            OwnerId = "owner1"
        };
        context.LibraryScripts.Add(script);
        await context.SaveChangesAsync();

        var updatedScript = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = script.Id,
            Title = "Updated Title",
            Code = "updated code",
            IsPrivate = true,
            OwnerId = "owner1" // Updated object's OwnerId (not used for permission check)
        };

        // Act: Pass a different owner than the one stored.
        var result = await service.UpdateUserScript(updatedScript, "otherOwner");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserScript_SuccessfullyUpdatesScript()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(UpdateUserScript_SuccessfullyUpdatesScript));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var originalScript = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Code = "original code",
            IsPrivate = false,
            OwnerId = "owner1",
            Language = "C#",
            CompilerVersion = "1.0",
            Description = "Old description",
            Author = "Old Author",
            Tags = "old"
        };
        originalScript.NugetPackages.Add(new SharpPad.Server.Services.Library.Models.LibraryScriptPackage { Id = Guid.NewGuid().ToString(), Version = "1.0.0" });
        context.LibraryScripts.Add(originalScript);
        await context.SaveChangesAsync();

        var updatedScript = new SharpPad.Server.Services.Library.Models.LibraryScript
        {
            Id = originalScript.Id,
            Title = "Updated Title",
            Code = "updated code",
            IsPrivate = true,
            OwnerId = "owner1",
            Language = "F#",
            CompilerVersion = "2.0",
            Description = "New description",
            Author = "New Author",
            Tags = "new",
            NugetPackages = new List<SharpPad.Server.Services.Library.Models.LibraryScriptPackage>
            {
                new SharpPad.Server.Services.Library.Models.LibraryScriptPackage { Id = Guid.NewGuid().ToString(), Version = "2.0.0" }
            }
        };

        // Act
        var result = await service.UpdateUserScript(updatedScript, "owner1");

        // Assert
        Assert.NotNull(result);
        // The returned script's Code property should be cleared.
        Assert.Equal(string.Empty, result!.Code);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("F#", result.Language);
        Assert.Equal("2.0", result.CompilerVersion);
        Assert.Equal("New description", result.Description);
        Assert.Equal("New Author", result.Author);
        Assert.Equal("new", result.Tags);
        // Verify that NugetPackages were replaced.
        Assert.Single(result.NugetPackages);
        Assert.Equal("2.0.0", result.NugetPackages.First().Version);
    }

    #endregion

    #region GetAllUserScripts Tests

    [Fact]
    public async Task GetAllUserScripts_ReturnsOnlyOwnerScripts_WithClearedCode()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetAllUserScripts_ReturnsOnlyOwnerScripts_WithClearedCode));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Add scripts for multiple owners.
        var script1 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Script 1", Code = "code1", OwnerId = "owner1", IsPrivate = false };
        var script2 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Script 2", Code = "code2", OwnerId = "owner1", IsPrivate = true };
        var script3 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Script 3", Code = "code3", OwnerId = "owner2", IsPrivate = false };

        context.LibraryScripts.AddRange(script1, script2, script3);
        await context.SaveChangesAsync();

        // Act
        var results = await service.GetAllUserScripts("owner1");

        // Assert
        Assert.Equal(2, results.Count);
        foreach (var script in results)
        {
            Assert.Equal("owner1", script.OwnerId);
            Assert.Equal(string.Empty, script.Code);
        }
    }

    [Fact]
    public async Task GetAllUserScripts_Paged_ReturnsCorrectScripts()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetAllUserScripts_Paged_ReturnsCorrectScripts));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Add several scripts for owner1 with distinct titles.
        var scripts = new List<SharpPad.Server.Services.Library.Models.LibraryScript>
        {
            new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "A Script", Code = "codeA", OwnerId = "owner1", IsPrivate = false },
            new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "B Script", Code = "codeB", OwnerId = "owner1", IsPrivate = false },
            new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "C Script", Code = "codeC", OwnerId = "owner1", IsPrivate = false },
            new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "D Script", Code = "codeD", OwnerId = "owner1", IsPrivate = false }
        };
        context.LibraryScripts.AddRange(scripts);
        await context.SaveChangesAsync();

        // Act: Skip 1 and take 2 – expecting "B Script" and "C Script" (ordered by Title).
        var results = await service.GetAllUserScripts("owner1", skip: 1, take: 2);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("B Script", results[0].Title);
        Assert.Equal("C Script", results[1].Title);
        Assert.All(results, s => Assert.Equal(string.Empty, s.Code));
    }

    #endregion

    #region GetAllPublicScripts Tests

    [Fact]
    public async Task GetAllPublicScripts_ReturnsOnlyPublicScripts()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(GetAllPublicScripts_ReturnsOnlyPublicScripts));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Add public and private scripts.
        var publicScript1 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Public 1", Code = "code1", IsPrivate = false, OwnerId = "owner1" };
        var publicScript2 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Public 2", Code = "code2", IsPrivate = false, OwnerId = "owner2" };
        var privateScript = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Private", Code = "code3", IsPrivate = true, OwnerId = "owner1" };

        context.LibraryScripts.AddRange(publicScript1, publicScript2, privateScript);
        await context.SaveChangesAsync();

        // Act
        var results = await service.GetAllPublicScripts(skip: 0, take: 10);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, s =>
        {
            Assert.False(s.IsPrivate);
            Assert.Equal(string.Empty, s.Code);
        });
    }

    #endregion

    #region SearchUserScripts Tests

    [Fact]
    public async Task SearchUserScripts_FindsMatchingScripts()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(SearchUserScripts_FindsMatchingScripts));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Add a few scripts for owner1.
        var script1 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Alpha", Description = "First script", Tags = "test", Code = "code1", OwnerId = "owner1" };
        var script2 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Beta", Description = "Second script", Tags = "sample", Code = "code2", OwnerId = "owner1" };
        var script3 = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Gamma", Description = "Third script", Tags = "example", Code = "code3", OwnerId = "owner1" };
        context.LibraryScripts.AddRange(script1, script2, script3);
        await context.SaveChangesAsync();

        // Act: Search for scripts with term "Alpha".
        var results = await service.SearchUserScripts("Alpha", "owner1", skip: 0, take: 10);

        // Assert
        Assert.Single(results);
        Assert.Equal("Alpha", results.First().Title);
        Assert.Equal(string.Empty, results.First().Code);
    }

    #endregion

    #region SearchPublicScripts Tests

    [Fact]
    public async Task SearchPublicScripts_FindsMatchingPublicScripts()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(SearchPublicScripts_FindsMatchingPublicScripts));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Add one public and one private script.
        var publicScript = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Public Alpha", Description = "A public script", Tags = "alpha", Code = "code1", IsPrivate = false, OwnerId = "owner1" };
        var privateScript = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Private Beta", Description = "A private script", Tags = "beta", Code = "code2", IsPrivate = true, OwnerId = "owner2" };
        context.LibraryScripts.AddRange(publicScript, privateScript);
        await context.SaveChangesAsync();

        // Act: Search for public scripts with term "Alpha".
        var results = await service.SearchPublicScripts("Alpha", skip: 0, take: 10);

        // Assert
        Assert.Single(results);
        Assert.Equal("Public Alpha", results.First().Title);
        Assert.Equal(string.Empty, results.First().Code);
    }

    #endregion

    #region DeleteScript Tests

    [Fact]
    public async Task DeleteScript_SuccessfullyDeletesScript()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(DeleteScript_SuccessfullyDeletesScript));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Script to Delete", Code = "code", IsPrivate = false, OwnerId = "owner1" };
        context.LibraryScripts.Add(script);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteScript(script.Id.ToString(), "owner1");

        // Assert
        Assert.True(result);
        var dbScript = await context.LibraryScripts.FindAsync(script.Id);
        Assert.Null(dbScript);
    }

    [Fact]
    public async Task DeleteScript_ScriptNotFound_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(DeleteScript_ScriptNotFound_ReturnsFalse));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        // Act
        var result = await service.DeleteScript(Guid.NewGuid().ToString(), "owner1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteScript_NotOwner_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext(nameof(DeleteScript_NotOwner_ReturnsFalse));
        var logger = CreateLogger();
        var service = new EfScriptLibraryService(context, logger);

        var script = new SharpPad.Server.Services.Library.Models.LibraryScript { Id = Guid.NewGuid(), Title = "Script", Code = "code", IsPrivate = false, OwnerId = "owner1" };
        context.LibraryScripts.Add(script);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteScript(script.Id.ToString(), "otherOwner");

        // Assert
        Assert.False(result);
    }

    #endregion
}
