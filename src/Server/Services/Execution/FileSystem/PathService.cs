namespace SharpPad.Server.Services.Execution.FileSystem;

public class PathService : IPathService
{
    private readonly string _storagePath;

    public PathService(IConfiguration configuration)
    {
        // Get the storage path from configuration, defaulting to "UploadedFiles"
        _storagePath = configuration.GetValue<string>("FileStorage:Path") ?? "UploadedFiles";
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <summary>
    /// Ensures that the provided path is inside the designated storage folder.
    /// If the path is relative, it is combined with the storage folder.
    /// Throws an UnauthorizedAccessException if the resulting path lies outside of the sandbox.
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

    /// <summary>
    /// Combines an array of strings into a path and ensures the resulting path is within the sandbox.
    /// </summary>
    public string Combine(params string[] paths)
    {
        string combined = Path.Combine(paths);
        return GetSandboxedPath(combined);
    }

    /// <summary>
    /// Returns the absolute sandboxed path for the provided path.
    /// </summary>
    public string GetFullPath(string path)
    {
        return GetSandboxedPath(path);
    }

    /// <summary>
    /// Returns the directory information for the provided sandboxed path.
    /// </summary>
    public string GetDirectoryName(string path)
    {
        string sandboxed = GetSandboxedPath(path);
        return Path.GetDirectoryName(sandboxed);
    }

    /// <summary>
    /// Returns the file name and extension of the specified sandboxed path.
    /// </summary>
    public string GetFileName(string path)
    {
        string sandboxed = GetSandboxedPath(path);
        return Path.GetFileName(sandboxed);
    }

    /// <summary>
    /// Returns the extension of the specified sandboxed path.
    /// </summary>
    public string GetExtension(string path)
    {
        string sandboxed = GetSandboxedPath(path);
        return Path.GetExtension(sandboxed);
    }

    /// <summary>
    /// Returns the file name without the extension of the specified sandboxed path.
    /// </summary>
    public string GetFileNameWithoutExtension(string path)
    {
        string sandboxed = GetSandboxedPath(path);
        return Path.GetFileNameWithoutExtension(sandboxed);
    }

    /// <summary>
    /// Changes the extension of the specified sandboxed path.
    /// Ensures the resulting path remains within the sandbox.
    /// </summary>
    public string ChangeExtension(string path, string extension)
    {
        string sandboxed = GetSandboxedPath(path);
        string changed = Path.ChangeExtension(sandboxed, extension);
        return GetSandboxedPath(changed);
    }

    /// <summary>
    /// Returns whether the specified sandboxed path has an extension.
    /// </summary>
    public bool HasExtension(string path)
    {
        string sandboxed = GetSandboxedPath(path);
        return Path.HasExtension(sandboxed);
    }

    /// <summary>
    /// Returns a value indicating whether the specified path string contains a root.
    /// This method is not sandbox‐enforced since it only inspects the input string.
    /// </summary>
    public bool IsPathRooted(string path)
    {
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// Returns an array containing characters that are not allowed in path names.
    /// </summary>
    public char[] GetInvalidPathChars()
    {
        return Path.GetInvalidPathChars();
    }

    /// <summary>
    /// Returns an array containing characters that are not allowed in file names.
    /// </summary>
    public char[] GetInvalidFileNameChars()
    {
        return Path.GetInvalidFileNameChars();
    }
}
