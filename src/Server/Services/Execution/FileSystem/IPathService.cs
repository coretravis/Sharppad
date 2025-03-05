namespace SharpPad.Server.Services.Execution.FileSystem
{
    /// <summary>
    /// Provides methods for manipulating and retrieving information about file paths.
    /// </summary>
    public interface IPathService
    {
        /// <summary>
        /// Changes the extension of a path string.
        /// </summary>
        /// <param name="path">The path information to modify.</param>
        /// <param name="extension">The new extension (with or without a leading period).</param>
        /// <returns>The modified path information.</returns>
        string ChangeExtension(string path, string extension);

        /// <summary>
        /// Combines an array of strings into a path.
        /// </summary>
        /// <param name="paths">An array of parts of the path.</param>
        /// <returns>The combined paths.</returns>
        string Combine(params string[] paths);

        /// <summary>
        /// Returns the directory information for the specified path string.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>Directory information for path, or null if path denotes a root directory or is null.</returns>
        string GetDirectoryName(string path);

        /// <summary>
        /// Returns the extension of the specified path string.
        /// </summary>
        /// <param name="path">The path string from which to get the extension.</param>
        /// <returns>The extension of the specified path (including the period "."), or null, or String.Empty.</returns>
        string GetExtension(string path);

        /// <summary>
        /// Returns the file name and extension of the specified path string.
        /// </summary>
        /// <param name="path">The path string from which to obtain the file name and extension.</param>
        /// <returns>The characters after the last directory character in path.</returns>
        string GetFileName(string path);

        /// <summary>
        /// Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The string returned by GetFileName, minus the last period (.) and all characters following it.</returns>
        string GetFileNameWithoutExtension(string path);

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute path information.</param>
        /// <returns>A string containing the fully qualified location of path, such as "C:\MyFile.txt".</returns>
        string GetFullPath(string path);

        /// <summary>
        /// Gets an array containing the characters that are not allowed in file names.
        /// </summary>
        /// <returns>An array containing the characters that are not allowed in file names.</returns>
        char[] GetInvalidFileNameChars();

        /// <summary>
        /// Gets an array containing the characters that are not allowed in path names.
        /// </summary>
        /// <returns>An array containing the characters that are not allowed in path names.</returns>
        char[] GetInvalidPathChars();

        /// <summary>
        /// Determines whether a path includes a file name extension.
        /// </summary>
        /// <param name="path">The path to search for an extension.</param>
        /// <returns>true if the characters that follow the last directory separator (\\ or /) or volume separator (:) in the path include a period (.) followed by one or more characters; otherwise, false.</returns>
        bool HasExtension(string path);

        /// <summary>
        /// Gets a value indicating whether the specified path string contains a root.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>true if path contains a root; otherwise, false.</returns>
        bool IsPathRooted(string path);
    }
}