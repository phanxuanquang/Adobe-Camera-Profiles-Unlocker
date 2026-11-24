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

            var adobeStandard = paths.AdobeStandardFile(targetModel);
            if (!File.Exists(adobeStandard))
            {
                logger.LogWarning("No Adobe Standard for {TargetModel} at {Path}: {Profile} keeps the {SourceModel} calibration",
                    targetModel, adobeStandard, profile.Name, sourceModel);
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
