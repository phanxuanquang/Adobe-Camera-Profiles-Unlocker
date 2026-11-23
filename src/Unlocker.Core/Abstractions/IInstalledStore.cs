using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

/// <summary>Manifest of the files this app created. Only these files may ever be deleted.</summary>
public interface IInstalledStore
{
    /// <summary>Tracked items plus legacy files found on disk.</summary>
    IReadOnlyList<InstalledCameraProfile> All { get; }

    /// <summary>Output folders: DCP first, then XMP.</summary>
    IReadOnlyList<string> Folders { get; }

    /// <summary>Raised on the caller's context after every change.</summary>
    event EventHandler? Changed;

    Task LoadAsync(CancellationToken ct = default);

    /// <summary>Replaces an existing item with the same name and target.</summary>
    Task AddAsync(InstalledCameraProfile item, CancellationToken ct = default);

    /// <summary>Moves files to a session trash so <see cref="RestoreAsync"/> can undo. Legacy files are deleted elevated, without undo.</summary>
    Task RemoveAsync(IEnumerable<InstalledCameraProfile> items, CancellationToken ct = default);

    Task RestoreAsync(IEnumerable<InstalledCameraProfile> items, CancellationToken ct = default);
}
