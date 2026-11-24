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

### Release
Push a tag such as `v3.1.0` on `master`. [release.yml](.github/workflows/release.yml) runs the tests, publishes a self-contained app for x64 and ARM64 (version taken from the tag), and attaches one zip per architecture to a new GitHub release. To build the same folder locally:
```powershell
dotnet publish Unlocker.WinUI -c Release -p:Platform=x64 -o out\x64
```

### How the code is organized
Start with [docs/README.md](docs/README.md): how the app works, then one page each for the architecture, camera profile formats, UI spec, development workflow and past decisions.

## Rules for code changes
These rules protect users' Adobe installs. Pull requests that break them are not merged.
- **Never modify Adobe originals** under `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera\` or `C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\`. Copy, then change the copy.
- **Never delete files** that the app did not create (files not in the app manifest).
- The app runs **without admin rights** and never asks for elevation.
- Presentation never references WinUI or Infrastructure.
- Build for `x64` and `ARM64`. Never `AnyCPU`.

## Code style
See [docs/development.md](docs/development.md#conventions). In short: one type per file, comments only for the why, the term **camera profile** everywhere, `{ThemeResource}` brushes and `AutomationProperties` in XAML.

## Pull requests
1. Fork the repo and create a branch from `master`.
2. Keep the change small and focused on one topic.
3. Add or update tests. `dotnet build` must have 0 warnings and `dotnet test` must pass.
4. For UI changes, attach screenshots in light and dark theme.
5. Fill in the pull request template.
