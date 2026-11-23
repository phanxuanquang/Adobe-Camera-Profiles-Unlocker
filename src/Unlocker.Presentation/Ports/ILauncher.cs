namespace Unlocker.Presentation.Ports;

public interface ILauncher
{
    Task OpenUrlAsync(Uri uri, CancellationToken ct = default);

    /// <summary>Creates the folder first when it does not exist yet.</summary>
    Task OpenFolderAsync(string path, CancellationToken ct = default);
}
