using Microsoft.Extensions.Logging;
using System.Buffers;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Installing;

internal sealed class DcpProfileInstaller(AppPaths paths, CameraProfileRewriter rewriter, ILogger<DcpProfileInstaller> logger)
{
    /// <summary>New DCP = source look + the target camera's calibration from its Adobe Standard. Source and Adobe Standard are only read.</summary>
    public async Task<string> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct)
    {
        byte[]? input = null, calibration = null, output = null;
        try
        {
            int length;
            (input, length) = await ReadAsync(profile.SourcePath, ct);
            var dcp = DcpFile.Parse(input.AsMemory(0, length));

            var adobeStandard = await FindAdobeStandardAsync(targetModel, ct);
            if (adobeStandard is null)
            {
                logger.LogWarning("No Adobe Standard for {TargetModel} in {Dir}: {Profile} keeps the {SourceModel} calibration",
                    targetModel, paths.AdobeStandardDir, profile.Name, sourceModel);
            }
            else
            {
                (calibration, length) = await ReadAsync(adobeStandard, ct);
                try
                {
                    dcp.CopyCalibrationFrom(DcpFile.Parse(calibration.AsMemory(0, length)));
                }
                catch (InvalidDataException e)
                {
                    // A broken Adobe file must not block the install or be reported as a broken source profile.
                    logger.LogWarning(e, "Unreadable Adobe Standard {Path}: {Profile} keeps the {SourceModel} calibration", adobeStandard, profile.Name, sourceModel);
                }
            }

            var tags = dcp.GetStrings();
            Rewrite(tags, sourceModel, targetModel);
            dcp.SetStrings(tags);
            var size = dcp.Size;
            output = ArrayPool<byte>.Shared.Rent(size);
            dcp.WriteTo(output.AsSpan(0, size));

            Directory.CreateDirectory(paths.DcpOutputDir);
            var path = Path.Combine(paths.DcpOutputDir, rewriter.OutputFileName(sourceModel, profile.Name, targetModel, CameraProfileFormat.DCP));
            try
            {
                await using var target = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 0, FileOptions.Asynchronous);
                await target.WriteAsync(output.AsMemory(0, size), ct);
            }
            catch
            {
                File.Delete(path); // no half-written profile outside the manifest
                throw;
            }
            return path;
        }
        finally
        {
            if (input is not null) ArrayPool<byte>.Shared.Return(input);
            if (calibration is not null) ArrayPool<byte>.Shared.Return(calibration);
            if (output is not null) ArrayPool<byte>.Shared.Return(output);
        }
    }

    internal void Rewrite(IDictionary<string, string> tags, string sourceModel, string targetModel)
    {
        rewriter.Rewrite(tags, sourceModel, targetModel);
        if (tags.TryGetValue("ProfileCalibrationSignature", out var signature))
            tags["ProfileCalibrationSignature"] = signature.Replace("com.adobe", CameraProfileRewriter.AuthorSignature, StringComparison.Ordinal);
    }

    /// <summary>Model (UniqueCameraModel) to Adobe Standard file, built on the first name miss. Installs run one at a time.</summary>
    private Dictionary<string, string>? _adobeStandardByModel;

    /// <summary>
    /// The target's Adobe Standard: "v2" first (Adobe's newer calibration of the same camera), then the plain name.
    /// A few files are named differently from their camera model ("Leica D-Lux 7 Adobe_Standard.dcp"), so on a miss
    /// every file's UniqueCameraModel is read once per session.
    /// </summary>
    private async Task<string?> FindAdobeStandardAsync(string model, CancellationToken ct)
    {
        string[] byName = [Path.Combine(paths.AdobeStandardDir, $"{model} Adobe Standard v2.dcp"), paths.AdobeStandardFile(model)];
        if (byName.FirstOrDefault(File.Exists) is { } path) return path;
        _adobeStandardByModel ??= await IndexAdobeStandardAsync(ct);
        return _adobeStandardByModel.GetValueOrDefault(model);
    }

    private async Task<Dictionary<string, string>> IndexAdobeStandardAsync(CancellationToken ct)
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(paths.AdobeStandardDir)) return index;
        foreach (var file in Directory.EnumerateFiles(paths.AdobeStandardDir, "*.dcp"))
        {
            var (buffer, length) = await ReadAsync(file, ct);
            try
            {
                var strings = DcpFile.Parse(buffer.AsMemory(0, length)).GetStrings();
                if (!strings.TryGetValue("CameraModelRestriction", out var model)) continue;
                if (!index.ContainsKey(model) || strings.GetValueOrDefault("Name", "").EndsWith("v2", StringComparison.Ordinal)) index[model] = file;
            }
            catch (InvalidDataException e)
            {
                logger.LogWarning(e, "Skipping unreadable Adobe Standard {Path}", file);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        return index;
    }

    /// <summary>Whole file into a pooled buffer; the caller returns it.</summary>
    private static async Task<(byte[] Buffer, int Length)> ReadAsync(string path, CancellationToken ct)
    {
        await using var source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0, FileOptions.Asynchronous);
        var length = checked((int)source.Length);
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            await source.ReadExactlyAsync(buffer.AsMemory(0, length), ct);
            return (buffer, length);
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw;
        }
    }
}
