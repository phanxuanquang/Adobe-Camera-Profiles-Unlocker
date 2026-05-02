using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class DcpToolService(
    IIOService ioService,
    IOptionsSnapshot<DcpToolOptions> options,
    ILoggerFactory? loggerFactory = null) : IDcpToolService
{
    private readonly ILogger<DcpToolService> _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DcpToolService>();
    private readonly DcpToolOptions _options = options.Value;
    private readonly IIOService _ioService = ioService;

    public async Task CompileXmlIntoDcpAsync(IEnumerable<Guid> cameraProfileIds, string? targetDirectory = null)
    {
        _logger.LogTrace("Compiling XML files into DCP for profile IDs:\n- {ProfileIds}", string.Join("\n- ", cameraProfileIds));

        targetDirectory = string.IsNullOrEmpty(targetDirectory) ? Path.GetTempPath() : targetDirectory;

        var newFilesToCompile = cameraProfileIds
            .Distinct()
            .Select(id => new
            {
                XmlFilePath = Path.Combine(_options.DecompileOutputDirectory, $"{id}.xml"),
                TargetDcpFilePath = Path.Combine(targetDirectory, $"{id}.dcp")
            })
            .Where(x => File.Exists(x.XmlFilePath) && !File.Exists(x.TargetDcpFilePath))
            .Select(x => x.XmlFilePath)
            .ToArray();

        if (newFilesToCompile.Length == 0)
        {
            _logger.LogWarning("No new XML files found to compile into DCP in the directory: {DecompileOutputDirectory}", _options.DecompileOutputDirectory);
            return;
        }

        // TODO: Implement the actual compilation logic using the DCP tool executable.
    }

    public async Task DecompileDcpIntoXmlAsync(IEnumerable<Guid> cameraProfileIds, string? targetDirectory = null)
    {
        _logger.LogTrace("Decompiling DCP files into XML for profile IDs:\n- {ProfileIds}", string.Join("\n- ", cameraProfileIds));

        targetDirectory = string.IsNullOrEmpty(targetDirectory) ? Path.GetTempPath() : targetDirectory;

        var newFilesToDecompile = cameraProfileIds
            .Distinct()
            .Select(id => new
            {
                DcpFilePath = Path.Combine(_options.TargetDirectory, $"{id}.dcp"),
                TargetXmlFilePath = Path.Combine(targetDirectory, $"{id}.xml")
            })
            .Where(x => File.Exists(x.DcpFilePath) && !File.Exists(x.TargetXmlFilePath))
            .Select(x => x.DcpFilePath)
            .ToArray();

        if (newFilesToDecompile.Length == 0)
        {
            _logger.LogWarning("No new DCP files found to decompile into XML in the directory: {TargetDirectory}", _options.TargetDirectory);
            return;
        }

        // TODO: Implement the actual decompilation logic using the DCP tool executable.
    }

    public async Task EnsureResourceInitializedAsync()
    {
        if (Directory.Exists(_options.DcpToolExecutableDirectory)
            && _ioService.EnumerateFiles(_options.DcpToolExecutableDirectory).Any()
            && _ioService.EnumerateFiles(_options.DcpToolExecutableDirectory).Sum(f => new FileInfo(f).Length) > 1 * 1024 * 1024
            && File.Exists(Path.Combine(_options.DcpToolExecutableDirectory, "dcpTool.exe")))
        {
            _logger.LogTrace("DCP tool executable directory already exists and is not empty. Skipping initialization.");
            return;
        }

        await _ioService.DownloadAndExtractZipAsync(_options.DcpToolFileUrl, _options.DcpToolExecutableDirectory);
    }
}