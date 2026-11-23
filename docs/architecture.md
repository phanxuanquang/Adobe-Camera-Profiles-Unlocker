# Architecture

Goal: UI framework swappable (WinUI 3 now, WPF later possible). App logic talks to backend only via interfaces. .NET 10 LTS.

App code in `src/`, tests in `tests/`. Shared build files at repo root (apply to both):
```
Directory.Build.props         Nullable, ImplicitUsings, TreatWarningsAsErrors, LangVersion latest, CPM on
Directory.Packages.props      all package versions (CPM). Extensions 10.0.x to match .NET 10 LTS
global.json                   SDK 11.0.100 (only SDK able to build; targets net10.0)
src/
  Unlocker.slnx               platforms x64 + ARM64 (WinUI mapped; libs AnyCPU is fine). Includes ../tests/Unlocker.Tests
  Unlocker.Core/              net10.0, 0 packages.
    Abstractions/             ICameraCatalog, ICameraProfileInstaller, IInstalledStore, ISettings, IEnvironmentProbe, IUpdateService
    Models/                   Brand, CameraModel, CameraProfile, CameraProfileFormat, InstalledCameraProfile, ReleaseInfo, EnvironmentIssue
    Rules/                    CameraProfileRewriter (shared rules, author credit, output name)
  Unlocker.Infrastructure/    net10.0-windows. AddInfrastructure() at root.
    Catalog/                  FileCameraCatalog
    Installing/               CameraProfileInstaller (picks by format) -> DcpProfileInstaller (+ DcpFile), XmpProfileInstaller; each has Rewrite (shared + format tags)
    Storage/                  JsonInstalledStore, JsonSettings, SettingsData
    Updates/                  GitHubUpdateService, GitHubRelease, GitHubAsset
    Platform/                 AppPaths, EnvironmentProbe
    Serialization/            UnlockerJsonContext
  Unlocker.Presentation/      net10.0 + CommunityToolkit.Mvvm. AddPresentation() at root.
    Common/                   StatusMessage, StatusAction, StatusSeverity, Text
    Ports/                    IDialogService, ILauncher
    Shell/ Startup/ Unlock/ Installed/ Settings/   one folder per screen: its VM + item VMs/records
  Unlocker.WinUI/             net10.0-windows10.0.26100.0, WinAppSDK 2.7 (template `winui-navview`).
                              Root: App.xaml(.cs) (composition root, shared converters/styles), MainWindow, AddWinUI()
    Pages/                    Startup, Unlock, Installed, Settings
    Controls/                 CameraPicker (+ CameraPickerGroup)
    Services/                 DialogService, Launcher (Windows.System.Launcher)
                              + CommunityToolkit.WinUI Converters/Triggers/SettingsControls 8.2
tests/
  Unlocker.Tests/             MSTest 4. DependencyInjectionTests at root.
    Core/ Infrastructure/ Presentation/   one test class per subject
    Fakes/                    one fake per file
    Support/                  TempFolderTest (temp AppPaths base)
    Fixtures/                 sample.xmp (DCP built in code: DcpFileTests.Sample)
  ui-tests.ps1                UI smoke batch (winapp ui); output test-results.json + ui-test-artifacts/ next to it (git-ignored)
```
One type per file. Folder = namespace.
Dev machine is ARM64: default build output `bin\ARM64\...`.

## Dependency direction
```
WinUI (or Wpf) -> Presentation -> Core <- Infrastructure
composition root (WinUI) also refs Infrastructure, only to call AddInfrastructure()
```
- Core refs nothing.
- Presentation refs Core only. Never Infrastructure, never `Microsoft.UI.*`, never `System.Windows.*`.
- New UI = new project: views + `IDialogService` + `ILauncher` + `Add<Ui>()`. All VMs reused.

## No extra layers
Use cases live in ViewModels. No mediator, repository, use-case classes, AutoMapper. Add layer only when 2nd caller exists.

## Threading
- I/O async + `CancellationToken`.
- VM commands = `[RelayCommand]` async (built-in cancel). VMs never touch dispatcher.
- Store `Changed` event: VMs refresh; WinUI marshals via `DispatcherQueue` in view if raised off UI thread. Prefer raising on caller context (VM awaits on UI thread).

## Packaging
Dev: packaged, `winapp run` (winui-dev-workflow rule). Release format (MSIX vs unpackaged self-contained exe like old app on GitHub Releases): ASK owner at plan step 6. Never `AnyCPU`; x64 + ARM64.
