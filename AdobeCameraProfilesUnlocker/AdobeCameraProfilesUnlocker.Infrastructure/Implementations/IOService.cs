using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Compression;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class IOService(
    IHttpClientFactory httpClientFactory,
    ILogger<IOService>? logger = null) : IIOService
{
    private readonly ILogger<IOService> _logger = logger ?? NullLogger<IOService>.Instance;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task DownloadAndExtractZipAsync(string url, string targetDirectory)
    {
        _logger.LogTrace("Downloading and extracting zip from {Url} to {TargetDirectory}.", url, targetDirectory);

        if (Directory.Exists(targetDirectory))
        {
            _logger.LogTrace("Target directory {TargetDirectory} already exists. Deleting before extraction.", targetDirectory);
            Directory.Delete(targetDirectory, recursive: true);
        }

        var tempFile = Path.GetTempFileName();
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(nameof(IOService));
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1 * 1024 * 1024, useAsync: true);
            await contentStream.CopyToAsync(fileStream, 1 * 1024 * 1024);

            _logger.LogTrace("Download complete. Extracting to {TargetDirectory}.", targetDirectory);
            ZipFile.ExtractToDirectory(tempFile, targetDirectory);
            _logger.LogInformation("Successfully downloaded and extracted zip from {Url} to {TargetDirectory}.", url, targetDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download and extract zip from {Url} to {TargetDirectory}.", url, targetDirectory);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    public Task DeleteFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            _logger.LogTrace("File {Path} does not exist. Skipping deletion.", path);
            return Task.CompletedTask;
        }

        File.Delete(path);
        _logger.LogTrace("Deleted file {Path}.", path);
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            _logger.LogTrace("Directory {Path} does not exist. Skipping deletion.", path);
            return Task.CompletedTask;
        }

        Directory.Delete(path, recursive: true);
        _logger.LogTrace("Deleted directory {Path}.", path);
        return Task.CompletedTask;
    }

    public Task CreateDirectoryAsync(string path)
    {
        Directory.CreateDirectory(path);
        _logger.LogTrace("Ensured directory exists at {Path}.", path);
        return Task.CompletedTask;
    }

    public IEnumerable<string> EnumerateFiles(string directory, string searchPattern = "*", bool recursive = true)
    {
        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Directory {Directory} does not exist. Returning empty enumeration.", directory);
            return [];
        }

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.EnumerateFiles(directory, searchPattern, searchOption);
    }

    public async Task MoveFilesAsync(IEnumerable<string> sourceFilePaths, string destinationDirectory)
    {
        var paths = sourceFilePaths.ToArray();
        _logger.LogTrace("Moving {Count} file(s) to {DestinationDirectory}.", paths.Length, destinationDirectory);

        await Parallel.ForEachAsync(paths, async (sourcePath, ct) =>
        {
            var destPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
            await Task.Run(() => File.Move(sourcePath, destPath, overwrite: true), ct);
        });

        _logger.LogInformation("Moved {Count} file(s) to {DestinationDirectory}.", paths.Length, destinationDirectory);
    }

    public async Task CopyFilesAsync(IEnumerable<string> sourceFilePaths, string destinationDirectory)
    {
        var paths = sourceFilePaths.ToArray();
        _logger.LogTrace("Copying {Count} file(s) to {DestinationDirectory}.", paths.Length, destinationDirectory);

        await Parallel.ForEachAsync(paths, async (sourcePath, ct) =>
        {
            var destPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
            await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
            await using var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            await source.CopyToAsync(dest, ct);
        });

        _logger.LogInformation("Copied {Count} file(s) to {DestinationDirectory}.", paths.Length, destinationDirectory);
    }

    public async Task<string> ReadTextFileAsync(string filePath)
    {
        _logger.LogTrace("Reading text file {FilePath}.", filePath);
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}