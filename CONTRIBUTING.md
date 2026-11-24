# Contributing

Thanks for helping. This file explains how to report problems and how to send code.

## Report a bug or ask for a feature
Use [GitHub Issues](https://github.com/phanxuanquang/Adobe-Camera-Profiles-Unlocker/issues/new/choose) and pick a template. Search existing issues first.

For bugs, include:
- App version (Settings page), Windows version, Camera Raw version.
- Your camera and the camera profiles you tried to install.
- Details from Event Viewer: **Windows Logs > Application**, source **.NET Runtime**. The app writes warnings and errors there.

## Build from source

### Requirements
- Windows 10 version 1809 or later (Windows 11 recommended).
- .NET SDK matching [global.json](global.json).
- Visual Studio 2026 with the **WinUI application development** workload, or the [WinApp CLI](https://github.com/microsoft/WinAppCli).
- Adobe Camera Raw installed, to run the app against real camera profiles.

### Commands
```powershell
cd src
dotnet build Unlocker.slnx
dotnet test ../tests/Unlocker.Tests
winapp run Unlocker.WinUI        # or F5 in Visual Studio
```

### Project layout
| Project | Role |
|---|---|
| `Unlocker.Core` | Models, abstractions, camera profile rewrite rules. No I/O. |
| `Unlocker.Infrastructure` | File system, catalog scan, installer, JSON storage, update check. |
| `Unlocker.Presentation` | ViewModels (CommunityToolkit.Mvvm). No UI framework references. |
| `Unlocker.WinUI` | WinUI 3 views, DI setup, platform services. |
| `tests/Unlocker.Tests` | MSTest unit and integration tests. |
| `tests/ui-tests.ps1` | UI smoke tests on the running app (WinApp CLI). Installs and removes one camera profile. |

## Rules for code changes
These rules protect users' Adobe installs. Pull requests that break them are not merged.
- **Never modify Adobe originals** under `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera\` or `C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\`. Copy, then change the copy.
- **Never delete files** that the app did not create (files not in the app manifest).
- The app runs **without admin rights** and never asks for elevation.
- Presentation never references WinUI or Infrastructure.
- Build for `x64` and `ARM64`. Never `AnyCPU`.

## Code style
- One type per file. Folder name = namespace.
- Comments only where the code cannot explain itself.
- Use the term **camera profile** in UI and code.
- `ILogger<T>` for logging. `CancellationToken` on async methods that do I/O.
- Use `{ThemeResource}` brushes and built-in text styles in XAML. Set `AutomationProperties` on interactive controls.

## Pull requests
1. Fork the repo and create a branch from `master`.
2. Keep the change small and focused on one topic.
3. Add or update tests. `dotnet build` must have 0 warnings and `dotnet test` must pass.
4. For UI changes, attach screenshots in light and dark theme.
5. Fill in the pull request template.
