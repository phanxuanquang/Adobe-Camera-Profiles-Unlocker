using AdobeCameraProfilesUnlocker.Domain.Interfaces;
using AdobeCameraProfilesUnlocker.Domain.Models.Enums;
using AdobeCameraProfilesUnlocker.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace AdobeCameraProfilesUnlocker.Infrastructure.Implementations;

public class DcpToolService(
    IIOService ioService,
    IOptionsSnapshot<DcpToolOptions> options, 
    ICameraProfileService cameraProfileService,
    ILoggerFactory? loggerFactory = null) : IDcpToolService
{
    private readonly ILogger<DcpToolService> _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DcpToolService>();
    private readonly DcpToolOptions _options = options.Value;
    private readonly IIOService _ioService = ioService;
    private readonly ICameraProfileService _cameraProfileService = cameraProfileService;

    public async Task CompileXmlIntoDcpAsync(IEnumerable<Guid> cameraProfileIds, string? targetDirectory = null)
    {
        _logger.LogTrace("Compiling XML files into DCP for profile IDs:\n- {ProfileIds}", string.Join("\n- ", cameraProfileIds));

        targetDirectory = string.IsNullOrEmpty(targetDirectory) ? Path.GetTempPath() : targetDirectory;
        
        var profileIdsToCompile = cameraProfileIds
            .Distinct()
            .Select(id => new
            {
                Id = id,
                XmlFilePath = Path.Combine(_options.DecompileOutputDirectory, $"{id}.xml"),
                TargetDcpFilePath = Path.Combine(targetDirectory, $"{id}.dcp")
            })
            .Where(x => File.Exists(x.XmlFilePath) && !File.Exists(x.TargetDcpFilePath))
            .Select(x => x.Id)
            .ToArray();

        if (profileIdsToCompile.Length == 0)
        {
            _logger.LogWarning("No new XML files found to compile into DCP in the directory: {DecompileOutputDirectory}", _options.DecompileOutputDirectory);
            return;
        }

        var profileIds = await _cameraProfileService.GetCameraProfilesByIdsAsync(profileIdsToCompile);
        var profileIdWithFilePathDict = profileIds
            .Select(p => new
            {
                XmlFilePath = Path.Combine(_options.DecompileOutputDirectory, $"{p.Id}.xml"),
                DcpFilePath = Path.Combine(targetDirectory, $"{p.Id}.dcp")
            })
            .ToArray();

        foreach (var item in profileIdWithFilePathDict)
        {
            try
            {
                await RunDcpToolAsync("-c", item.XmlFilePath, item.DcpFilePath);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error compiling XML file {XmlFilePath} into DCP file {DcpFilePath}.", item.XmlFilePath, item.DcpFilePath);
            }
        }
    }

    public async Task DecompileDcpIntoXmlAsync(IEnumerable<Guid> cameraProfileIds, string? targetDirectory = null)
    {
        _logger.LogTrace("Decompiling DCP files into XML for profile IDs:\n- {ProfileIds}", string.Join("\n- ", cameraProfileIds));

        targetDirectory = string.IsNullOrEmpty(targetDirectory) ? Path.GetTempPath() : targetDirectory;

        var profileIds = await _cameraProfileService.GetCameraProfilesByIdsAsync(cameraProfileIds.ToArray());
        var profileIdWithFilePathDict = profileIds
            .Where(p => p.FileType == CameraProfileType.DCP && File.Exists(p.FilePath) && !File.Exists(Path.Combine(targetDirectory, $"{p.Id}.xml")))
            .Select(p => new
            {
                DcpFilePath = p.FilePath,
                XmlFilePath = Path.Combine(targetDirectory, $"{p.Id}.xml")
            })
            .ToArray();

        if (profileIdWithFilePathDict.Length == 0)
        {
            _logger.LogWarning("No new DCP files found to decompile into XML for the provided profile IDs.");
            return;
        }

        foreach (var item in profileIdWithFilePathDict)
        {
            try
            {
                await RunDcpToolAsync("-d", item.DcpFilePath, item.XmlFilePath);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error decompiling DCP file {DcpFilePath} into XML file {XmlFilePath}.", item.DcpFilePath, item.XmlFilePath);
            }
        }
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

    private async Task RunDcpToolAsync(string option, string inputFilePath, string outputFilePath)
    {
        _logger.LogTrace("Running DCP tool with option {Option} on input file {InputFilePath} to produce output file {OutputFilePath}.", option, inputFilePath, outputFilePath);

        if (option != "-d" && option != "-c")
        {
            throw new ArgumentException("Option must be either '-d' for decompilation or '-c' for compilation.", nameof(option));
        }

        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException($"Input file not found: {inputFilePath}", inputFilePath);
        }

        if (File.Exists(outputFilePath))
        {
            _logger.LogWarning("Output file {OutputFilePath} already exists. Skipping execution of DCP tool for input file {InputFilePath}.", outputFilePath, inputFilePath);
            return;
        }

        var inputFileExt = Path.GetExtension(inputFilePath).ToLower();
        var expectedInputExt = Path.GetExtension(outputFilePath).ToLower();

        if (inputFileExt.Equals(expectedInputExt, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Input file extension '{inputFileExt}' does not match expected extension '{expectedInputExt}' for the given option.", nameof(inputFilePath));
        }

        if (option == "-d" && (inputFileExt != ".dcp" || expectedInputExt != ".xml"))
        {
            throw new ArgumentException($"For decompilation option '-d', input file must have a '.dcp' extension and output file must have a '.xml' extension. Provided files: {inputFilePath} -> {outputFilePath}", nameof(inputFilePath));
        }

        if (option == "-c" && (inputFileExt != ".xml" || expectedInputExt != ".dcp"))
        {
            throw new ArgumentException($"For compilation option '-c', input file must have a '.xml' extension and output file must have a '.dcp' extension. Provided files: {inputFilePath} -> {outputFilePath}", nameof(inputFilePath));
        }

        var executablePath = Path.Combine(_options.DcpToolExecutableDirectory, "dcpTool.exe");
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"DCP tool executable not found: {executablePath}", executablePath);
        }

        using var process = new Process 
        { 
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                ArgumentList = { option, inputFilePath, outputFilePath },
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        process.Start();

        using var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync().ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var stderr = await stderrTask.ConfigureAwait(false);
            throw new InvalidOperationException($"DCP tool exited with code {process.ExitCode} for '{option} {inputFilePath} -> {outputFilePath}'. stderr: {stderr}");
        }
    }
}