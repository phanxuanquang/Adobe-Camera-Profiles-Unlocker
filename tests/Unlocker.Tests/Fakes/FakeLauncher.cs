using Unlocker.Presentation.Ports;

namespace Unlocker.Tests.Fakes;

internal sealed class FakeLauncher : ILauncher
{
    public List<string> Opened { get; } = [];

    public Task OpenUrlAsync(Uri uri, CancellationToken ct = default)
    {
        Opened.Add(uri.AbsoluteUri);
        return Task.CompletedTask;
    }

    public Task OpenFolderAsync(string path, CancellationToken ct = default)
    {
        Opened.Add(path);
        return Task.CompletedTask;
    }
}
