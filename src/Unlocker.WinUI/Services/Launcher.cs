using Microsoft.Extensions.Logging;
using Unlocker.Presentation.Ports;

namespace Unlocker.WinUI.Services;

internal sealed class Launcher(ILogger<Launcher> logger) : ILauncher
{
    public async Task OpenUrlAsync(Uri uri, CancellationToken ct = default)
    {
        if (!await Windows.System.Launcher.LaunchUriAsync(uri))
            logger.LogWarning("Could not open URI '{Uri}'", uri);
    }

    public async Task OpenFolderAsync(string path, CancellationToken ct = default)
    {
        if (!await Windows.System.Launcher.LaunchFolderPathAsync(path))
            logger.LogWarning("Could not open the folder '{Path}'", path);
    }
}
