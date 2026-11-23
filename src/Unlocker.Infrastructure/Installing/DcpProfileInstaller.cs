using System.Buffers;
using Unlocker.Core.Enums;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Installing;

internal sealed class DcpProfileInstaller(AppPaths paths, CameraProfileRewriter rewriter)
{
    public async Task<string> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct)
    {
        byte[]? input = null, output = null;
        try
        {
            int length;
            await using (var source = new FileStream(profile.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0, FileOptions.Asynchronous))
            {
                length = checked((int)source.Length);
                input = ArrayPool<byte>.Shared.Rent(length);
                await source.ReadExactlyAsync(input.AsMemory(0, length), ct);
            }

            var dcp = DcpFile.Parse(input.AsMemory(0, length));
            var tags = dcp.GetStrings();
            Rewrite(tags, sourceModel, targetModel);
            dcp.SetStrings(tags);
            var size = dcp.Size;
            output = ArrayPool<byte>.Shared.Rent(size);
            dcp.WriteTo(output.AsSpan(0, size));

            Directory.CreateDirectory(paths.DcpOutputDir);
            var path = Path.Combine(paths.DcpOutputDir, rewriter.OutputFileName(profile.Name, targetModel, CameraProfileFormat.DCP));
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
            if (output is not null) ArrayPool<byte>.Shared.Return(output);
        }
    }

    internal void Rewrite(IDictionary<string, string> tags, string sourceModel, string targetModel)
    {
        rewriter.Rewrite(tags, sourceModel, targetModel);
        if (tags.TryGetValue("ProfileCalibrationSignature", out var signature))
            tags["ProfileCalibrationSignature"] = signature.Replace("com.adobe", CameraProfileRewriter.AuthorSignature, StringComparison.Ordinal);
    }
}
