namespace SharpPad.Server.Services.Execution.FileSystem;

public class DirectoryService : IDirectoryService
{
    private readonly string _storagePath;

    public DirectoryService(IConfiguration configuration)
    {
        _storagePath = configuration.GetValue<string>("FileStorage:Path") ?? "UploadedFiles";
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <summary>
    /// Ensures that the provided directory path is within the designated storage folder.
    /// If the path is relative, it is combined with the storage folder.
    /// Throws an UnauthorizedAccessException if the resolved path lies outside of the sandbox.
    /// </summary>
    private string GetSandboxedPath(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(_storagePath, path);
        }
        string fullPath = Path.GetFullPath(path);
        string storageFullPath = Path.GetFullPath(_storagePath);
        if (!fullPath.StartsWith(storageFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Access denied for path: {path}");
        }
        return fullPath;
    }

    #region Basic Directory Operations

    public bool Exists(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.Exists(safePath);
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.CreateDirectory(safePath);
    }

    public void Delete(string path)
    {
        string safePath = GetSandboxedPath(path);
        Directory.Delete(safePath);
    }

    public void Delete(string path, bool recursive)
    {
        string safePath = GetSandboxedPath(path);
        Directory.Delete(safePath, recursive);
    }

    #endregion

    #region Retrieval of File System Entries

    public string[] GetFiles(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetFiles(safePath);
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetFiles(safePath, searchPattern);
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetFiles(safePath, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateFiles(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateFiles(safePath);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateFiles(safePath, searchPattern);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateFiles(safePath, searchPattern, searchOption);
    }

    public string[] GetDirectories(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetDirectories(safePath);
    }

    public string[] GetDirectories(string path, string searchPattern)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetDirectories(safePath, searchPattern);
    }

    public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetDirectories(safePath, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateDirectories(safePath);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateDirectories(safePath, searchPattern);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateDirectories(safePath, searchPattern, searchOption);
    }

    public string[] GetFileSystemEntries(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetFileSystemEntries(safePath);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.EnumerateFileSystemEntries(safePath);
    }

    #endregion

    #region Time and Attribute Information

    public DateTime GetCreationTime(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetCreationTime(safePath);
    }

    public DateTime GetCreationTimeUtc(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetCreationTimeUtc(safePath);
    }

    public DateTime GetLastAccessTime(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetLastAccessTime(safePath);
    }

    public DateTime GetLastAccessTimeUtc(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetLastAccessTimeUtc(safePath);
    }

    public DateTime GetLastWriteTime(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetLastWriteTime(safePath);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        string safePath = GetSandboxedPath(path);
        return Directory.GetLastWriteTimeUtc(safePath);
    }

    #endregion
}
