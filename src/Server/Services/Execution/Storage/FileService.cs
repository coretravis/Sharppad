using System.Text;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution.Storage;

public class FileService : IFileService
{
    private readonly long _maxFileSize;
    private readonly string _storagePath;
    private CodeExecutionResponse? _currentResponse;
    private ILogger<FileService> _logger;
    public FileService(IConfiguration configuration, ILogger<FileService> logger)
    {
        // Get settings from configuration; defaults are 1MB and "UploadedFiles" folder.
        _maxFileSize = configuration.GetValue<long>("FileStorage:MaxFileSize", 1024 * 1024);
        _storagePath = configuration.GetValue<string>("FileStorage:Path") ?? "UploadedFiles";
        _logger = logger;
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <summary>
    /// Although not part of IFile, this method allows you to set the current response
    /// where file paths will be recorded.
    /// </summary>
    public void SetExecutionResponse(CodeExecutionResponse response)
    {
        _currentResponse = response;
    }

    /// <summary>
    /// Generates a unique file name by inserting a GUID before the extension.
    /// The file is placed in the specified storage folder if not already in a directory.
    /// </summary>
    private string GetUniquePath(string originalPath)
    {
        // If the caller provided a relative path, ensure it is under our storage folder.
        string directory = Path.GetDirectoryName(originalPath) ?? _storagePath;
        if (!Path.IsPathRooted(directory))
        {
            directory = Path.Combine(_storagePath, directory);
        }
        var fileName = Path.GetFileNameWithoutExtension(originalPath);
        var extension = Path.GetExtension(originalPath);
        var uniqueName = $"{fileName}_{Guid.NewGuid()}{extension}";
        return Path.Combine(directory, uniqueName);
    }

    /// <summary>
    /// Validates that the content size does not exceed the maximum allowed.
    /// </summary>
    private void ValidateSize(long size)
    {
        if (size > _maxFileSize)
        {
            throw new Exception($"File size exceeds maximum allowed size of {_maxFileSize} bytes.");
        }
    }

    /// <summary>
    /// For append operations, validates that the file’s current size plus the new content
    /// does not exceed the maximum allowed.
    /// </summary>
    private void ValidateAppendSize(string path, long appendedSize)
    {
        long existingSize = File.Exists(path) ? new FileInfo(path).Length : 0;
        if (existingSize + appendedSize > _maxFileSize)
        {
            throw new Exception($"Appending content would exceed maximum allowed size of {_maxFileSize} bytes.");
        }
    }

    /// <summary>
    /// If a current response is set, records the file path in it.
    /// </summary>
    private void RecordFile(string filePath)
    {
        _currentResponse?.Files.Add(filePath);
    }

    #region Non–Write Operations (Delegated Directly)

    public bool Exists(string path) => File.Exists(path);
    public void Delete(string path) => File.Delete(path);

    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);
    public void SetAttributes(string path, FileAttributes fileAttributes) => File.SetAttributes(path, fileAttributes);

    public DateTime GetCreationTime(string path) => File.GetCreationTime(path);
    public DateTime GetCreationTimeUtc(string path) => File.GetCreationTimeUtc(path);
    public void SetCreationTime(string path, DateTime creationTime) => File.SetCreationTime(path, creationTime);
    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc) => File.SetCreationTimeUtc(path, creationTimeUtc);

    public DateTime GetLastAccessTime(string path) => File.GetLastAccessTime(path);
    public DateTime GetLastAccessTimeUtc(string path) => File.GetLastAccessTimeUtc(path);
    public void SetLastAccessTime(string path, DateTime lastAccessTime) => File.SetLastAccessTime(path, lastAccessTime);
    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) => File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

    public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);
    public DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTimeUtc(path);
    public void SetLastWriteTime(string path, DateTime lastWriteTime) => File.SetLastWriteTime(path, lastWriteTime);
    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

    public void Copy(string sourceFileName, string destFileName) => File.Copy(sourceFileName, destFileName);
    public void Copy(string sourceFileName, string destFileName, bool overwrite) => File.Copy(sourceFileName, destFileName, overwrite);
    public void Move(string sourceFileName, string destFileName) => File.Move(sourceFileName, destFileName);

    public FileStream Open(string path, FileMode mode) => File.Open(path, mode);
    public FileStream Open(string path, FileMode mode, FileAccess access) => File.Open(path, mode, access);
    public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share) => File.Open(path, mode, access, share);
    public FileStream OpenRead(string path) => File.OpenRead(path);
    public StreamReader OpenText(string path) => File.OpenText(path);
    public FileStream OpenWrite(string path) => File.OpenWrite(path);

    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
    public string[] ReadAllLines(string path) => File.ReadAllLines(path);
    public string[] ReadAllLines(string path, Encoding encoding) => File.ReadAllLines(path, encoding);
    public string ReadAllText(string path) => File.ReadAllText(path);
    public string ReadAllText(string path, Encoding encoding) => File.ReadAllText(path, encoding);
    public IEnumerable<string> ReadLines(string path) => File.ReadLines(path);
    public IEnumerable<string> ReadLines(string path, Encoding encoding) => File.ReadLines(path, encoding);

    #endregion

    #region Write Operations with Custom Logic

    // --- Synchronous Write Methods ---

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(path);
        File.WriteAllBytes(newPath, bytes);
        RecordFile(newPath);
    }

    public void WriteAllLines(string path, IEnumerable<string> contents)
        => WriteAllLines(path, contents, Encoding.UTF8);

    public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        // Join lines to compute the total size.
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        ValidateSize(bytes.Length);

        var newPath = GetUniquePath(path);
        File.WriteAllLines(newPath, contents, encoding);
        RecordFile(newPath);
    }

    public void WriteAllText(string path, string contents)
        => WriteAllText(path, contents, Encoding.UTF8);

    public void WriteAllText(string path, string contents, Encoding encoding)
    {
        byte[] bytes = encoding.GetBytes(contents);
        ValidateSize(bytes.Length);

        var newPath = GetUniquePath(path);
        File.WriteAllText(newPath, contents, encoding);
        RecordFile(newPath);
    }

    public void AppendAllLines(string path, IEnumerable<string> contents)
        => AppendAllLines(path, contents, Encoding.UTF8);

    public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);

        // If the target file exists, use it and validate the additional size.
        // Otherwise, generate a unique path.
        string targetPath = File.Exists(path) ? path : GetUniquePath(path);
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
        byte[] bytes = encoding.GetBytes(contents);

        string targetPath = File.Exists(path) ? path : GetUniquePath(path);
        if (File.Exists(targetPath))
        {
            ValidateAppendSize(targetPath, bytes.Length);
        }

        File.AppendAllText(targetPath, contents, encoding);
        RecordFile(targetPath);
    }

    public StreamWriter AppendText(string path)
    {
        string targetPath = File.Exists(path) ? path : GetUniquePath(path);
        var writer = File.AppendText(targetPath);
        RecordFile(targetPath);
        return writer;
    }

    // For stream–creating methods, we generate a unique name and record the file path.
    public FileStream Create(string path, int bufferSize, FileOptions options)
    {
        var newPath = GetUniquePath(path);
        var fs = File.Create(newPath, bufferSize, options);
        RecordFile(newPath);
        return fs;
    }

    public FileStream Create(string path, int bufferSize)
    {
        var newPath = GetUniquePath(path);
        var fs = File.Create(newPath, bufferSize);
        RecordFile(newPath);
        return fs;
    }

    public FileStream Create(string path)
    {
        var newPath = GetUniquePath(path);
        var fs = File.Create(newPath);
        RecordFile(newPath);
        return fs;
    }

    public StreamWriter CreateText(string path)
    {
        var newPath = GetUniquePath(path);
        var sw = File.CreateText(newPath);
        RecordFile(newPath);
        return sw;
    }

    #endregion

    #region Asynchronous Write Methods

    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ValidateSize(bytes.Length);
        var newPath = GetUniquePath(path);
        await File.WriteAllBytesAsync(newPath, bytes, cancellationToken);
        RecordFile(newPath);
    }

    public async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
        => await WriteAllLinesAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        ValidateSize(bytes.Length);

        var newPath = GetUniquePath(path);
        await File.WriteAllLinesAsync(newPath, contents, encoding, cancellationToken);
        RecordFile(newPath);
    }

    public async Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        => await WriteAllTextAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        byte[] bytes = encoding.GetBytes(contents);
        ValidateSize(bytes.Length);

        var newPath = GetUniquePath(path);
        await File.WriteAllTextAsync(newPath, contents, encoding, cancellationToken);
        RecordFile(newPath);
    }

    public async Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        => await AppendAllTextAsync(path, contents, Encoding.UTF8, cancellationToken);

    public async Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        byte[] bytes = encoding.GetBytes(contents);
        string targetPath = File.Exists(path) ? path : GetUniquePath(path);
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
        string contentStr = string.Join(Environment.NewLine, contents);
        byte[] bytes = encoding.GetBytes(contentStr);
        string targetPath = File.Exists(path) ? path : GetUniquePath(path);
        if (File.Exists(targetPath))
        {
            ValidateAppendSize(targetPath, bytes.Length);
        }

        await File.AppendAllLinesAsync(targetPath, contents, encoding, cancellationToken);
        RecordFile(targetPath);
    }

    #endregion
}
