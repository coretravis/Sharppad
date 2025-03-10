﻿using System.Text;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution.FileSystem;

public interface IFileService
{
    void AppendAllLines(string path, IEnumerable<string> contents);
    void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);
    Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default);
    Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default);
    void AppendAllText(string path, string contents);
    void AppendAllText(string path, string contents, Encoding encoding);
    Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);
    Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default);
    StreamWriter AppendText(string path);
    void Copy(string sourceFileName, string destFileName);
    void Copy(string sourceFileName, string destFileName, bool overwrite);
    FileStream Create(string path);
    FileStream Create(string path, int bufferSize);
    FileStream Create(string path, int bufferSize, FileOptions options);
    StreamWriter CreateText(string path);
    void Delete(string path);
    bool Exists(string path);
    FileAttributes GetAttributes(string path);
    DateTime GetCreationTime(string path);
    DateTime GetCreationTimeUtc(string path);
    DateTime GetLastAccessTime(string path);
    DateTime GetLastAccessTimeUtc(string path);
    DateTime GetLastWriteTime(string path);
    DateTime GetLastWriteTimeUtc(string path);
    void Move(string sourceFileName, string destFileName);
    FileStream Open(string path, FileMode mode);
    FileStream Open(string path, FileMode mode, FileAccess access);
    FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);
    FileStream OpenRead(string path);
    StreamReader OpenText(string path);
    FileStream OpenWrite(string path);
    byte[] ReadAllBytes(string path);
    string[] ReadAllLines(string path);
    string[] ReadAllLines(string path, Encoding encoding);
    string ReadAllText(string path);
    string ReadAllText(string path, Encoding encoding);
    IEnumerable<string> ReadLines(string path);
    IEnumerable<string> ReadLines(string path, Encoding encoding);
    void SetAttributes(string path, FileAttributes fileAttributes);
    void SetCreationTime(string path, DateTime creationTime);
    void SetCreationTimeUtc(string path, DateTime creationTimeUtc);
    void SetExecutionResponse(CodeExecutionResponse response);
    void SetLastAccessTime(string path, DateTime lastAccessTime);
    void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
    void SetLastWriteTime(string path, DateTime lastWriteTime);
    void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
    void WriteAllBytes(string path, byte[] bytes);
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);
    void WriteAllLines(string path, IEnumerable<string> contents);
    void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);
    Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default);
    Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default);
    void WriteAllText(string path, string contents);
    void WriteAllText(string path, string contents, Encoding encoding);
    Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);
    Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default);
}
