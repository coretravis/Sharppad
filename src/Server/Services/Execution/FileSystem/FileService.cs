using System.Text;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution.FileSystem;

public class FileService : IFileService
{
    private readonly long _maxFileSize;
    private readonly string _storagePath;
    private CodeExecutionResponse? _currentResponse;
    private readonly ILogger<FileService> _logger;

    public FileService(IConfiguration configuration, ILogger<FileService> logger)
    {
        // Get settings from configuration; defaults are 1MB and "UploadedFiles" folder, 
        _maxFileSize = configuration.GetValue<long>("FileStorage:MaxFileSize", 1024 * 1024);
        _storagePath = configuration.GetValue<string>("FileStorage:Path") ?? "UploadedFiles"; // ideally we should use some sort of blob storage.
        _logger = logger;
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <summary>
    /// Although not part of IFileService, this method allows you to set the current response
    /// where file paths will be recorded.
    /// </summary>
    public void SetExecutionResponse(CodeExecutionResponse response)
    {
        _currentResponse = response;
    }

    /// <summary>
    /// Returns an absolute path that is guaranteed to be inside the storage folder.
    /// If the provided path is not rooted, it is combined with the storage folder.
    /// Throws an exception if the resulting path is outside of the sandbox.
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
    /// Generates a unique file name by inserting a GUID before the extension.
    /// The provided path is first sanitized so that it is inside the sandbox.
    /// </summary>
    private string GetUniquePath(string originalPath)
    {
        // Ensure the original path is sandboxed.
        string sandboxedPath = GetSandboxedPath(originalPath);
        string directory = Path.GetDirectoryName(sandboxedPath) ?? _storagePath;
        var fileName = Path.GetFileNameWithoutExtension(sandboxedPath);
        var extension = Path.GetExtension(sandboxedPath);
        var uniqueName = $"{fileName}_{Guid.NewGuid()}{extension}";
        return Path.Combine(directory, uniqueName);
    }

    private void ValidateSize(long size)
    {
        if (size > _maxFileSize)
        {
            throw new Exception($"File size exceeds maximum allowed size of {_maxFileSize} bytes.");
        }
    }

    private void ValidateAppendSize(string path, long appendedSize)
    {
        long existingSize = File.Exists(path) ? new FileInfo(path).Length : 0;
        if (existingSize + appendedSize > _maxFileSize)
        {
            throw new Exception($"Appending content would exceed maximum allowed size of {_maxFileSize} bytes.");
        }
    }

    private void RecordFile(string filePath)
    {
        _currentResponse?.Files.Add(filePath);
    }

    #region Non–Write Operations (Sandboxed)

    public bool Exists(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.Exists(safePath);
    }

    public void Delete(string path)
    {
        string safePath = GetSandboxedPath(path);
        File.Delete(safePath);
    }

    public FileAttributes GetAttributes(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetAttributes(safePath);
    }

    public void SetAttributes(string path, FileAttributes fileAttributes)
    {
        string safePath = GetSandboxedPath(path);
        File.SetAttributes(safePath, fileAttributes);
    }

    public DateTime GetCreationTime(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetCreationTime(safePath);
    }

    public DateTime GetCreationTimeUtc(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetCreationTimeUtc(safePath);
    }

    public void SetCreationTime(string path, DateTime creationTime)
    {
        string safePath = GetSandboxedPath(path);
        File.SetCreationTime(safePath, creationTime);
    }

    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
        string safePath = GetSandboxedPath(path);
        File.SetCreationTimeUtc(safePath, creationTimeUtc);
    }

    public DateTime GetLastAccessTime(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetLastAccessTime(safePath);
    }

    public DateTime GetLastAccessTimeUtc(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetLastAccessTimeUtc(safePath);
    }

    public void SetLastAccessTime(string path, DateTime lastAccessTime)
    {
        string safePath = GetSandboxedPath(path);
        File.SetLastAccessTime(safePath, lastAccessTime);
    }

    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
        string safePath = GetSandboxedPath(path);
        File.SetLastAccessTimeUtc(safePath, lastAccessTimeUtc);
    }

    public DateTime GetLastWriteTime(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetLastWriteTime(safePath);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.GetLastWriteTimeUtc(safePath);
    }

    public void SetLastWriteTime(string path, DateTime lastWriteTime)
    {
        string safePath = GetSandboxedPath(path);
        File.SetLastWriteTime(safePath, lastWriteTime);
    }

    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
        string safePath = GetSandboxedPath(path);
        File.SetLastWriteTimeUtc(safePath, lastWriteTimeUtc);
    }

    public void Copy(string sourceFileName, string destFileName)
    {
        string safeSource = GetSandboxedPath(sourceFileName);
        string safeDest = GetSandboxedPath(destFileName);
        File.Copy(safeSource, safeDest);
    }

    public void Copy(string sourceFileName, string destFileName, bool overwrite)
    {
        string safeSource = GetSandboxedPath(sourceFileName);
        string safeDest = GetSandboxedPath(destFileName);
        File.Copy(safeSource, safeDest, overwrite);
    }

    public void Move(string sourceFileName, string destFileName)
    {
        string safeSource = GetSandboxedPath(sourceFileName);
        string safeDest = GetSandboxedPath(destFileName);
        File.Move(safeSource, safeDest);
    }

    public FileStream Open(string path, FileMode mode)
    {
        string safePath = GetSandboxedPath(path);
        return File.Open(safePath, mode);
    }

    public FileStream Open(string path, FileMode mode, FileAccess access)
    {
        string safePath = GetSandboxedPath(path);
        return File.Open(safePath, mode, access);
    }

    public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        string safePath = GetSandboxedPath(path);
        return File.Open(safePath, mode, access, share);
    }

    public FileStream OpenRead(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.OpenRead(safePath);
    }

    public StreamReader OpenText(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.OpenText(safePath);
    }

    public FileStream OpenWrite(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.OpenWrite(safePath);
    }

    public byte[] ReadAllBytes(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadAllBytes(safePath);
    }

    public string[] ReadAllLines(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadAllLines(safePath);
    }

    public string[] ReadAllLines(string path, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadAllLines(safePath, encoding);
    }

    public string ReadAllText(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadAllText(safePath);
    }

    public string ReadAllText(string path, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadAllText(safePath, encoding);
    }

    public IEnumerable<string> ReadLines(string path)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadLines(safePath);
    }

    public IEnumerable<string> ReadLines(string path, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        return File.ReadLines(safePath, encoding);
    }

    #endregion

    #region Write Operations with Custom Logic (Sandboxed)

    public void WriteAllBytes(string path, byte[] bytes)
    {
        string safePath = GetSandboxedPath(path);
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(safePath);
        File.WriteAllBytes(newPath, bytes);
        RecordFile(newPath);
    }

    public void WriteAllLines(string path, IEnumerable<string> contents)
        => WriteAllLines(path, contents, Encoding.UTF8);

    public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(safePath);
        File.WriteAllLines(newPath, contents, encoding);
        RecordFile(newPath);
    }

    public void WriteAllText(string path, string contents)
        => WriteAllText(path, contents, Encoding.UTF8);

    public void WriteAllText(string path, string contents, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        byte[] bytes = encoding.GetBytes(contents);
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(safePath);
        File.WriteAllText(newPath, contents, encoding);
        RecordFile(newPath);
    }

    public void AppendAllLines(string path, IEnumerable<string> contents)
        => AppendAllLines(path, contents, Encoding.UTF8);

    public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        string targetPath = File.Exists(safePath) ? safePath : GetUniquePath(safePath);
        if (File.Exists(targetPath))
        {
            ValidateAppendSize(targetPath, bytes.Length);
        }
        File.AppendAllLines(targetPath, contents, encoding);
        RecordFile(targetPath);
    }

    public void AppendAllText(string path, string contents)
        => AppendAllText(path, contents, Encoding.UTF8);

    public void AppendAllText(string path, string contents, Encoding encoding)
    {
        string safePath = GetSandboxedPath(path);
        byte[] bytes = encoding.GetBytes(contents);
        string targetPath = File.Exists(safePath) ? safePath : GetUniquePath(safePath);
        if (File.Exists(targetPath))
        {
            ValidateAppendSize(targetPath, bytes.Length);
        }
        File.AppendAllText(targetPath, contents, encoding);
        RecordFile(targetPath);
    }

    public StreamWriter AppendText(string path)
    {
        string safePath = GetSandboxedPath(path);
        string targetPath = File.Exists(safePath) ? safePath : GetUniquePath(safePath);
        var writer = File.AppendText(targetPath);
        RecordFile(targetPath);
        return writer;
    }

    public FileStream Create(string path, int bufferSize, FileOptions options)
    {
        string safePath = GetSandboxedPath(path);
        var newPath = GetUniquePath(safePath);
        var fs = File.Create(newPath, bufferSize, options);
        RecordFile(newPath);
        return fs;
    }

    public FileStream Create(string path, int bufferSize)
    {
        string safePath = GetSandboxedPath(path);
        var newPath = GetUniquePath(safePath);
        var fs = File.Create(newPath, bufferSize);
        RecordFile(newPath);
        return fs;
    }

    public FileStream Create(string path)
    {
        string safePath = GetSandboxedPath(path);
        var newPath = GetUniquePath(safePath);
        var fs = File.Create(newPath);
        RecordFile(newPath);
        return fs;
    }

    public StreamWriter CreateText(string path)
    {
        string safePath = GetSandboxedPath(path);
        var newPath = GetUniquePath(safePath);
        var sw = File.CreateText(newPath);
        RecordFile(newPath);
        return sw;
    }

    #endregion

    #region Asynchronous Write Methods

    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        string safePath = GetSandboxedPath(path);
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(safePath);
        await File.WriteAllBytesAsync(newPath, bytes, cancellationToken);
        RecordFile(newPath);
    }

    public async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
        => await WriteAllLinesAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        string safePath = GetSandboxedPath(path);
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(safePath);
        await File.WriteAllLinesAsync(newPath, contents, encoding, cancellationToken);
        RecordFile(newPath);
    }

    public async Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        => await WriteAllTextAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        string safePath = GetSandboxedPath(path);
        byte[] bytes = encoding.GetBytes(contents);
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(safePath);
        await File.WriteAllTextAsync(newPath, contents, encoding, cancellationToken);
        RecordFile(newPath);
    }

    public async Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        => await AppendAllTextAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        string safePath = GetSandboxedPath(path);
        byte[] bytes = encoding.GetBytes(contents);
        string targetPath = File.Exists(safePath) ? safePath : GetUniquePath(safePath);
        if (File.Exists(targetPath))
        {
            ValidateAppendSize(targetPath, bytes.Length);
        }
        await File.AppendAllTextAsync(targetPath, contents, encoding, cancellationToken);
        RecordFile(targetPath);
    }

    public async Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
        => await AppendAllLinesAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        string safePath = GetSandboxedPath(path);
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        string targetPath = File.Exists(safePath) ? safePath : GetUniquePath(safePath);
        if (File.Exists(targetPath))
        {
            ValidateAppendSize(targetPath, bytes.Length);
        }
        await File.AppendAllLinesAsync(targetPath, contents, encoding, cancellationToken);
        RecordFile(targetPath);
    }

    #endregion
}
