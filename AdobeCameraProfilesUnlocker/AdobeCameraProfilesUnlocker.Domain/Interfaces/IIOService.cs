namespace AdobeCameraProfilesUnlocker.Domain.Interfaces;

/// <summary>
/// Defines a set of file system operations for downloading, extracting, deleting, and enumerating files and directories.
/// </summary>
/// <remarks>Implementations of this interface provide asynchronous and synchronous methods for common file
/// management tasks, including downloading and extracting zip archives, deleting files or directories if they exist,
/// and enumerating files with optional search patterns and recursion. Methods are designed to support cancellation and
/// efficient file handling. Thread safety and error handling depend on the specific implementation.</remarks>
public interface IIOService
{
    /// <summary>
    /// Download a zip file from the specified URL and extract its contents to the target directory. If the target directory already exists, it will be deleted before extraction.
    /// </summary>
    /// <param name="url">The URL of the zip file to download.</param>
    /// <param name="targetDirectory">The directory where the contents of the zip file will be extracted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks> Ensure the high performance and memory efficiency of this method.</remarks>
    Task DownloadAndExtractZipAsync(string url, string targetDirectory);

    /// <summary>
    /// Deletes the specified file if it exists.
    /// </summary>
    /// <param name="path">The path of the file to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string path);

    /// <summary>
    /// Deletes the specified directory if it exists.
    /// </summary>
    /// <param name="path">The full path of the directory to delete. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteDirectoryAsync(string path);

    /// <summary>
    /// Creates a directory at the specified path if it does not already exist. 
    /// </summary>
    /// <param name="path">The full path of the directory to create. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous create operation.</returns>
    Task CreateDirectoryAsync(string path);

    /// <summary>
    /// Enumerates the files in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to enumerate files from.</param>
    /// <param name="searchPattern">The search pattern to match files against.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories.</param>
    /// <returns>An enumerable of file paths.</returns>
    IEnumerable<string> EnumerateFiles(string directory, string searchPattern = "*", bool recursive = true);

    /// <summary>
    /// Moves multiple files from the source paths to the destination directory. If a file already exists at the destination, it will be overwritten.
    /// </summary>
    /// <param name="sourceFilePaths">The paths of the files to move.</param>
    /// <param name="destinationDirectory">The directory to move the files to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MoveFilesAsync(IEnumerable<string> sourceFilePaths, string destinationDirectory);

    /// <summary>
    /// Copies multiple files from the specified source paths to the specified destination directory.
    /// </summary>
    /// <param name="sourceFilePaths">The full paths of the files to copy. Cannot be null or empty. The files must exist.</param>
    /// <param name="destinationDirectory">The directory where the files will be copied. Cannot be null or empty. If a file exists, it may be overwritten.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    Task CopyFilesAsync(IEnumerable<string> sourceFilePaths, string destinationDirectory);

    /// <summary>
    /// Asynchronously reads the contents of a text file at the specified path.
    /// </summary>
    /// <param name="filePath">The full path to the text file to read. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the contents of the file as a string.</returns>
    /// <remarks> Ensure the high performance and memory efficiency of this method, especially for large files.</remarks>
    Task<string> ReadTextFileAsync(string filePath);
}