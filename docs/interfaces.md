# Contracts

## Core models (`Unlocker.Core/Models`, records, immutable)
```csharp
public enum CameraProfileFormat { Dcp, Xmp }
public sealed record CameraProfile(string Name, string SourcePath, CameraProfileFormat Format);
public sealed record CameraModel(string Name, IReadOnlyList<CameraProfile> Profiles);
public sealed record Brand(string Name, string Family, IReadOnlyList<CameraModel> Models);
public sealed record InstalledCameraProfile(string Name, string SourceModel, string TargetModel,
    IReadOnlyList<string> Files);
public sealed record ReleaseInfo(Version Version, Uri PageUrl); // release page, not an asset
public enum EnvironmentIssue { CameraRawMissing, ProfilesMissing }
```

## Core abstractions (`Unlocker.Core/Abstractions`, backend ports)
```csharp
public interface ICameraCatalog          { Task<IReadOnlyList<Brand>> LoadAsync(CancellationToken ct = default); }
public interface ICameraProfileInstaller { Task<InstalledCameraProfile> InstallAsync(CameraProfile profile, string sourceModel, string targetModel, CancellationToken ct = default); }
public interface IInstalledStore
{
    IReadOnlyList<InstalledCameraProfile> All { get; }
    IReadOnlyList<string> Folders { get; }            // [DCP output dir, XMP output dir]
    event EventHandler? Changed;
    Task LoadAsync(CancellationToken ct = default);   // manifest, once at startup
    Task AddAsync(InstalledCameraProfile item, CancellationToken ct = default);
    Task RemoveAsync(IEnumerable<InstalledCameraProfile> items);   // move to trash, not cancellable
    Task RestoreAsync(IEnumerable<InstalledCameraProfile> items);  // undo, not cancellable
}
public interface IEnvironmentProbe { IReadOnlyList<EnvironmentIssue> Check(); }
public interface IUpdateService    { Task<ReleaseInfo?> GetNewerAsync(Version current, CancellationToken ct = default); } // null = up to date, throws when the check fails
public interface ISettings         { string? TargetCamera { get; set; } Task SaveAsync(CancellationToken ct = default); }
```
Installer does NOT touch store; UnlockViewModel adds results (single place for Undo/rollback).
Per format: `DcpProfileInstaller` / `XmpProfileInstaller` (Infrastructure-internal, same shape: `Task<string> InstallAsync(profile, sourceModel, targetModel, ct)` returns output path). `DcpFile` = parsed DCP (`Parse`, `GetStrings`, `SetStrings`, `Size`, `WriteTo`).

## Presentation ports (`Unlocker.Presentation/Ports`, each UI implements)
```csharp
public interface IDialogService { Task<bool> ConfirmAsync(string title, string body, string confirmText); Task<bool> InformAsync(string title, string body, string primaryText); }
public interface ILauncher      { Task OpenUrlAsync(Uri uri, CancellationToken ct = default); Task OpenFolderAsync(string path, CancellationToken ct = default); }
```
InfoBar, progress, errors = VM observable state (`Common/StatusMessage`) bound to a plain WinUI `InfoBar` in the page. No notification service, no custom InfoBar control.

## Rules
- Add member only when a VM needs it. No speculative members.
- Single impl per interface OK: 2nd "impl" = test fake / other UI.
- Impl classes `internal sealed`; exposed only via `Add<Layer>()`.
