using Unlocker.Core.Models;

namespace Unlocker.Core.Abstractions;

/// <summary>Manifest of the files this app created. Only these files may ever be deleted.</summary>
public interface IInstalledStore
{
    /// <summary>Tracked items whose files still exist.</summary>
    IReadOnlyList<InstalledCameraProfile> All { get; }

    /// <summary>Output folders: DCP first, then XMP.</summary>
    IReadOnlyList<string> Folders { get; }

    /// <summary>Raised on the caller's context after every change.</summary>
    event EventHandler? Changed;

    Task LoadAsync(CancellationToken ct = default);

    /// <summary>Replaces an existing item with the same name, source model and target model.</summary>
    Task AddAsync(InstalledCameraProfile item, CancellationToken ct = default);

    /// <summary>Moves files to a session trash so <see cref="RestoreAsync"/> can undo.
    /// Not cancellable: a cancel between file moves and the manifest save would leave them out of sync.</summary>
    Task RemoveAsync(IEnumerable<InstalledCameraProfile> items);

    /// <summary>Skips items installed again since removal or whose file path is taken: never overwrites. Not cancellable.</summary>
    Task RestoreAsync(IEnumerable<InstalledCameraProfile> items);
}
