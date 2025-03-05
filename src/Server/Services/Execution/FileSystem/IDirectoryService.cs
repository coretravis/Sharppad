namespace SharpPad.Server.Services.Execution.FileSystem;

/// <summary>
/// Provides methods for creating, deleting, and enumerating directories and files.
/// </summary>
public interface IDirectoryService
{
    /// <summary>
    /// Creates all directories and subdirectories in the specified path.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <returns>A <see cref="DirectoryInfo"/> object representing the created directory.</returns>
    DirectoryInfo CreateDirectory(string path);

    /// <summary>
    /// Deletes the specified directory.
    /// </summary>
    /// <param name="path">The directory path to delete.</param>
    void Delete(string path);

    /// <summary>
    /// Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
    /// </summary>
    /// <param name="path">The directory path to delete.</param>
    /// <param name="recursive">true to delete subdirectories and files; otherwise, false.</param>
    void Delete(string path, bool recursive);

    /// <summary>
    /// Returns an enumerable collection of directory names in the specified path.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>An enumerable collection of directory names in the specified path.</returns>
    IEnumerable<string> EnumerateDirectories(string path);

    /// <summary>
    /// Returns an enumerable collection of directory names that match a search pattern in a specified path.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of directories in path.</param>
    /// <returns>An enumerable collection of directory names that match a search pattern in a specified path.</returns>
    IEnumerable<string> EnumerateDirectories(string path, string searchPattern);

    /// <summary>
    /// Returns an enumerable collection of directory names that match a search pattern in a specified path, and optionally searches subdirectories.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of directories in path.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>An enumerable collection of directory names that match a search pattern in a specified path.</returns>
    IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Returns an enumerable collection of file names in the specified path.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>An enumerable collection of file names in the specified path.</returns>
    IEnumerable<string> EnumerateFiles(string path);

    /// <summary>
    /// Returns an enumerable collection of file names that match a search pattern in a specified path.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in path.</param>
    /// <returns>An enumerable collection of file names that match a search pattern in a specified path.</returns>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern);

    /// <summary>
    /// Returns an enumerable collection of file names that match a search pattern in a specified path, and optionally searches subdirectories.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in path.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>An enumerable collection of file names that match a search pattern in a specified path.</returns>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Returns an enumerable collection of file system entries in the specified path.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>An enumerable collection of file system entries in the specified path.</returns>
    IEnumerable<string> EnumerateFileSystemEntries(string path);

    /// <summary>
    /// Determines whether the given path refers to an existing directory.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>true if path refers to an existing directory; otherwise, false.</returns>
    bool Exists(string path);

    /// <summary>
    /// Returns the creation date and time of the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain creation date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
    DateTime GetCreationTime(string path);

    /// <summary>
    /// Returns the creation date and time, in Coordinated Universal Time (UTC), of the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain creation date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in UTC time.</returns>
    DateTime GetCreationTimeUtc(string path);

    /// <summary>
    /// Returns the names of the subdirectories (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>An array of the full names (including paths) of the subdirectories that match the search pattern.</returns>
    string[] GetDirectories(string path);

    /// <summary>
    /// Returns the names of the subdirectories (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in path.</param>
    /// <returns>An array of the full names (including paths) of the subdirectories that match the search pattern.</returns>
    string[] GetDirectories(string path, string searchPattern);

    /// <summary>
    /// Returns the names of the subdirectories (including their paths) that match the specified search pattern in the specified directory, and optionally searches subdirectories.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in path.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>An array of the full names (including paths) of the subdirectories that match the search pattern.</returns>
    string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Returns the names of the files (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>An array of the full names (including paths) of the files that match the search pattern.</returns>
    string[] GetFiles(string path);

    /// <summary>
    /// Returns the names of the files (including their paths) that match the specified search pattern in the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in path.</param>
    /// <returns>An array of the full names (including paths) of the files that match the search pattern.</returns>
    string[] GetFiles(string path, string searchPattern);

    /// <summary>
    /// Returns the names of the files (including their paths) that match the specified search pattern in the specified directory, and optionally searches subdirectories.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in path.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>An array of the full names (including paths) of the files that match the search pattern.</returns>
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Returns the names of all the file system entries in the specified path.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <returns>An array of the full names (including paths) of the file system entries in the specified path.</returns>
    string[] GetFileSystemEntries(string path);

    /// <summary>
    /// Returns the date and time the specified file or directory was last accessed.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain access date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time the specified file or directory was last accessed. This value is expressed in local time.</returns>
    DateTime GetLastAccessTime(string path);

    /// <summary>
    /// Returns the date and time, in Coordinated Universal Time (UTC), that the specified file or directory was last accessed.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain access date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
    DateTime GetLastAccessTimeUtc(string path);

    /// <summary>
    /// Returns the date and time the specified file or directory was last written to.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain write date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time the specified file or directory was last written to. This value is expressed in local time.</returns>
    DateTime GetLastWriteTime(string path);

    /// <summary>
    /// Returns the date and time, in Coordinated Universal Time (UTC), that the specified file or directory was last written to.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain write date and time information.</param>
    /// <returns>A <see cref="DateTime"/> structure set to the date and time the specified file or directory was last written to. This value is expressed in UTC time.</returns>
    DateTime GetLastWriteTimeUtc(string path);
}
