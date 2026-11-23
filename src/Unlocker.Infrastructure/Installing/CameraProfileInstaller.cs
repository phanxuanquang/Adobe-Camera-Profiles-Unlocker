using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Unlocker.Core.Enums;
using Unlocker.Core.Abstractions;
using Unlocker.Core.Models;
using Unlocker.Core.Rules;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Installing;

internal sealed class CameraProfileInstaller(AppPaths paths, IDcpTool dcpTool, ILogger<CameraProfileInstaller> logger) : ICameraProfileInstaller
{
    public async Task<InstalledCameraProfile> InstallAsync(CameraProfile profile, CameraModel source, string targetModel, CancellationToken ct = default)
    {
        var installed = profile.Format == CameraProfileFormat.DCP
            ? await InstallDcpAsync(profile, source, targetModel, ct)
            : await InstallXmpAsync(profile, source, targetModel, ct);
        logger.LogInformation("Installed profile {Profile} from {SourceModel} for {TargetModel} at {Output}",
            profile.Name, source.Name, targetModel, installed.Files[0]);
        return installed;
    }

    private async Task<InstalledCameraProfile> InstallDcpAsync(CameraProfile profile, CameraModel source, string targetModel, CancellationToken ct)
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        try
        {
            await dcpTool.DecompileAsync(profile.SourcePath, tmp, ct);
            XDocument doc;
            await using (var read = File.OpenRead(tmp))
                doc = await XDocument.LoadAsync(read, LoadOptions.None, ct);
            CameraProfileRewriter.RewriteDcpXml(doc, source.Name, targetModel);
            await using (var write = File.Create(tmp))
                await doc.SaveAsync(write, SaveOptions.None, ct);

            Directory.CreateDirectory(paths.DcpOutputDir);
            var output = Path.Combine(paths.DcpOutputDir, CameraProfileRewriter.OutputFileName(profile.Name, targetModel, CameraProfileFormat.DCP));
            await dcpTool.CompileAsync(tmp, output, ct);
            return new InstalledCameraProfile(profile.Name, source.Name, targetModel, [output]);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    private async Task<InstalledCameraProfile> InstallXmpAsync(CameraProfile profile, CameraModel source, string targetModel, CancellationToken ct)
    {
        XDocument doc;
        await using (var read = File.OpenRead(profile.SourcePath))
            doc = await XDocument.LoadAsync(read, LoadOptions.PreserveWhitespace, ct);
        CameraProfileRewriter.RewriteXmp(doc, source.Name, profile.Name, targetModel, Guid.NewGuid());

        Directory.CreateDirectory(paths.XmpOutputDir);
        var output = Path.Combine(paths.XmpOutputDir, CameraProfileRewriter.OutputFileName(profile.Name, targetModel, CameraProfileFormat.XMP));
        await using (var write = File.Create(output))
            await doc.SaveAsync(write, SaveOptions.DisableFormatting, ct);
        return new InstalledCameraProfile(profile.Name, source.Name, targetModel, [output]);
    }
}
