# Migration plan

Status: `[ ]` todo, `[~]` in progress, `[x]` done. Update this file when a step finishes. Log decisions + surprises under step.

## Decisions (owner, 2026-11-23)
- .NET 10 LTS.
- No admin (verified, adobe-paths.md).
- Keep branding (Copyright + signature overwrite).
- DI everywhere, lifetimes per di-lifetimes.md.
- UI followed the HTML mock (removed 2026-11-23; ux-flow.md is the spec), except: no theme picker. App always follows Windows theme (owner, 2026-11-23). Old `Theme` key in settings.json is ignored.
- Owner overrides (2026-11-23, round 2): one name "camera profile" in UI and code (no "look"); no black-and-white marker; search hides brands without matches; "Your camera" = right `SplitView` pane with brand filter (no dialog); plain WinUI `InfoBar` (no custom StatusInfoBar control); one type per file, folder = namespace, comments only where needed.

## Open questions (ask owner when reached)
- Release format: MSIX vs unpackaged self-contained exe (old app = single-file exe via GitHub Releases). Step 6.

## Steps
1. [x] **Scaffold** (2026-11-23)
   - Done: 5 projects + CPM + slnx, all types/interfaces/VMs/pages as skeleton with `TODO(step N)`, DI wired end-to-end
     (`DependencyInjectionTests` validates graph). `dotnet build` 0 warnings, `dotnet test` 8 pass / 2 skip, `winapp run` shows shell.
   - Already implemented (not TODO): AppPaths, EnvironmentProbe, BrandCatalog, ProfileRewriter helpers (ParseLookName,
     OutputFileName, IsMonochromeName), DCP scan in FileCameraCatalog, DcpTool process wrapper, JsonSettings,
     store Load/Save/Dispose, DialogService, Launcher, InstalledViewModel.Refresh, SettingsViewModel update/links.
   - Template: `winapp new --template winui-navview`. MSTest chosen.
   Original scope: `Unlocker.slnx`, 4 src + 1 test project, `Directory.Build.props`, `Directory.Packages.props`, `global.json`. WinUI app via `winapp new --template winui-mvvm` (winui-dev-workflow) then reshape. Add `Microsoft.Windows.SDK.BuildTools.WinUIAnalyzer`.
   - Done when: `dotnet build` 0 warnings; `winapp run` shows empty shell window.
2. [x] **Core**: models, abstractions, `ProfileRewriter` + tests with real fixtures. (2026-11-23)
   - Done when: tests green; source fixture hash unchanged.
   - Done: `RewriteDcpXml`, `RewriteXmp` + 3 fixture tests (fields rewritten, rest unchanged, fixture hash unchanged, new UUID). 11/11 pass.
   - Decision: DCP fixture trimmed (2.5 MB -> 67 lines); rewrite only touches top-level elements, bulk tables irrelevant.
   - Decision: ProfileName strips leading `Camera ` then prefixes `<Source>: ` (no prefix -> still prefixed). Signature: replace `com.adobe` substring only.
   - Surprise: Fujifilm XMPs are stubs (`crs:RGBTable` hash, no embedded table). Copy renders fine (verified step 3).
3. [x] **Infrastructure**: `AdobePaths`, `FileCameraCatalog` (real ~396 DCP folders + XMP by restriction), `DcpTool`, `ProfileInstaller`, `JsonInstalledStore` (+ trash/undo), `JsonSettings`, `GitHubUpdateService`, `EnvironmentProbe`, `AddInfrastructure()`.
   - Done when: integration check installs 1 Fujifilm look for Sony model as non-admin, appears in Camera Raw Profile Browser (owner confirms), removed cleanly. Confirm subfolder `Unlocker\` is scanned.
   - Done (2026-11-23): XMP scan (recursive, model from restriction, XmlReader header only, XMP wins on dup), installer DCP/XMP,
     store add/remove-to-trash/restore, update check. 13 new tests (temp dirs, fake + real dcpTool).
   - Integration (file-based C# script, non-admin): catalog = 11 brands / 422 models / 3454 looks. Installed `Fujifilm X-T5: BLEACH BYPASS` (XMP)
     + `Canon EOS R5: Portrait` (DCP) for Sony ILCE-7M4. Owner confirmed both show + apply in Profile Browser. Removed: folders empty, manifest `[]`.
   - Decision: corrupt manifest -> start empty (files untouched).
   - Surprise: no Fujifilm DCP folder on machine; XMP layout nested per model (adobe-paths.md updated).
4. [x] **Presentation**: Shell/Startup/Unlock/Installed/Settings VMs, `IDialogService`, `ILauncher`, `AddPresentation()`, VM tests with fakes. (2026-11-23)
   - Done: all VM behavior per mock unlock.js/installed.js (wording identical). 16 VM tests; suite 40/40.
   - Decision: new port `IClipboard` ("Copy details" in error InfoBar). New Core member `IInstalledStore.Folders` (DCP, XMP output dirs) replaces path literal in Presentation; Settings "Profile folders" shows these two (mock's %AppData% row dropped, per adobe-paths.md).
   - Decision: Unlock right pane = `Profiles` (brand mode) + `SearchResults` (`CameraProfileGroup` per brand, search mode). Tiles cached per brand+source model, checkbox toggle never rebuilds tiles (keeps keyboard focus).
   - Decision: `UnlockViewModel.ViewInstalledRequested` event; shell view navigates. Install records each file in store before honoring cancel, so rollback sees it.
   - Donate link = old app's banking image (branding kept).
5. [~] **WinUI views** per ux-flow.md. `AddWinUI()`, DPI window size, follows Windows theme. (2026-11-23, all but High Contrast)
   - Done when: screenshots match mock (light/dark/narrow), keyboard path works, High Contrast OK, `winui-code-review` clean.
   - Done: Startup / Unlock (first-run picker, brands, tiles, search, tray, install progress) / Installed (SettingsExpander per camera) / Settings (SettingsCard).
     MainWindow: 1280x800 DIP by DPI, min 760x560, Installed InfoBadge, Settings update dot, startup gating.
     Verified by real app screenshots vs mock: dark, light, narrow (780 px, compact nav). Real install of 2 camera profiles, remove + Undo, bulk remove dialog, empty state, search + empty search, selection clear + Undo, keyboard Tab path.
   - NOT verified: High Contrast (needs system contrast theme; owner to check). Build 0 warnings, tests 41/41.
   - Decision (owner): CommunityToolkit.WinUI Converters/Triggers/Helpers + SettingsControls; shared styles/resources in App.xaml; Fluent geometry radii.
   - Decision: app icon = old app's `icons8_camera_9.ico` (branding kept). Package logo PNGs still template placeholders: step 6.
   - Decision: Settings "Profile folders" = DCP + XMP output dirs (mock's %AppData% row dropped).
   - Bugs found + fixed: Settings page overflow (ScrollViewer/StackPanel), null DataContext in SettingsExpander template -> crash, a11y names (record ToString on list items, Install button), "Copied from" clipped when narrow. Details: conventions.md "Gotchas".
   - Deferred: strings not in .resw (English only); crash log only via App.UnhandledException (async-void crash bypassed it once).
   - Surprise: `winapp run --detach` started from a PowerShell tool call can die with that call; run attached in background instead.
   - Round 2 (owner feedback): renamed Look -> CameraProfile, InstalledProfile -> InstalledCameraProfile, IProfileInstaller -> ICameraProfileInstaller, ProfileRewriter -> Rules/CameraProfileRewriter. Feature folders: Infrastructure Catalog/Installing/Storage/Updates/Platform/Serialization; Presentation Common/Ports/Shell/Startup/Unlock/Installed/Settings; WinUI Controls/Helpers/Pages/Services; Tests Core/Infrastructure/Presentation/Fakes/Support. Monochrome removed. `UnlockViewModel.VisibleBrands` synced in place (keeps list selection). Build 0 warnings, tests 38/38, UI batch 16/16.
   - Round 3 (owner): logging via `ILogger<T>` (Abstractions in Infrastructure + Presentation; Debug + Event Log providers in App.xaml.cs, Event Log Warning+ under source ".NET Runtime", verified non-admin write). `IClipboard` + "Copy details" removed: error InfoBar shows ex.Message, details in Event Viewer. `ILauncher` async via `Windows.System.Launcher.LaunchUriAsync` / `LaunchFolderPathAsync`. CancellationToken added to launcher + `SetTarget`; store remove/restore deliberately without token (conventions.md). Crash log file replaced by `LogCritical`. Build 0 warnings, tests 38/38, UI batch 16/16.
   - Round 4 (ponytail audit + owner): dropped Microsoft.Extensions.Http (one shared HttpClient) and CommunityToolkit.WinUI.Helpers (unused); removed unused ReleaseInfo.Notes/PublishedAt, InstalledCameraProfile.At, StatusSeverity.Warning, DcpToolException; Helpers/Ui.cs removed (InfoBar severity via IsEqualStateTrigger); CameraPicker items = model name only, current camera shown as selected item. Bug fixed: VisualStateManager moved to page root (SplitView wrap had disabled Busy/Narrow states). Build 0 warnings, tests 38/38, UI batch 16/16.
   - Round 5 (owner): tray hint text removed; install progress = `ProgressRing` inside Install button (ProgressBar, ProgressValue/Max, TrayHint removed).
   - Surprise: an installed.json written by the earlier build (key "Look") loads with an empty Name after the rename. Pre-release, so no migration.
6. [ ] **Ship**: publish (format per owner), e2e manual in Camera Raw, README update.
   - Done (2026-11-23): review round (ponytail-review + ponytail-audit + winui-code-review). Fixed:
     unguarded I/O in Undo / rollback / SetTarget / restore / manifest load (IOException + UnauthorizedAccessException now show an Error status; 7 tests);
     Event Log filter Warning+ (was Information); store is IDisposable, disposed synchronously in Window.Closed (async handler let the process exit before the trash was emptied);
     a11y: installed state via CheckBox HelpText (FontIcon has no automation peer), explicit AutomationIds, Install button name while busy; StartupPage x:DefaultBindMode;
     "Copied from" ComboBox bound in XAML (BrandItemViewModel.ModelLabels + SourceLabel, TwoWay SelectedItem; index binding leaked selections across brands), code-behind FillModels removed;
     GitHub / Donate / InfoBar "Get Camera Raw" = HyperlinkButton NavigateUri (commands + service locator call removed);
     csproj: RID from Platform (NETSDK1032 on clean checkout), template leftovers removed; EnvironmentIssue.ProfilesMissing (one value); CameraModel.Brand removed; ICameraProfileInstaller takes sourceModel string; TestFiles inlined.
     Not done (decided): StatusActionTemplate stays per page (x:Bind DataTemplate cannot live in App.xaml without a code-behind ResourceDictionary); atomic-save block stays duplicated in two stores (sharing needs a new class); ArrayPool kept (owner asked for memory optimization).
   - Done (2026-11-24): review fixes. Installed tile = same target + name + source model, so same-name looks from other models no longer show installed; a tile that becomes installed leaves the selection (not while busy, so a failed run keeps it). DialogService serializes ShowAsync (SemaphoreSlim: WinUI allows one ContentDialog); success-dialog failure logged, InfoBar stays. Launcher.OpenFolderAsync creates the folder. UnlockPage Unloaded detaches BrandList + both repeaters (singleton VM leaked pages). SingleSelectionFollowsFocus=False on BrandList + CameraPicker (arrows move focus, Enter selects). Update check accepts only https://github.com page URLs. csproj: x64 + ARM64 only, RID from Platform again (ARM64 built win-x64), no AnyCPU, duplicate Msix block / pack metadata / Nullable / Optimize removed; WindowsAppSDKSelfContained kept (unpackaged F5, checked: bin exe starts). AppWindow.SetIcon absolute path. a11y: title bar pane toggle hidden with the pane; brand item name has "n selected of m camera profiles"; Installed nav item name has count; update card LiveSetting Polite; status action AutomationId = Label. Tests 86/86.
   - Done (2026-11-24): review fixes, data side. Output name `<Source> <Name> (for <Target>).<ext>`; manifest key = name + source + target. Load skips null/incomplete entries; Restore never overwrites. Catalog skips inaccessible folders, retries a failed scan, reads `Camera_<Name>.dcp`. XMP install deletes partial file on failure. DCP: all 3rd-illuminant tags dropped, IlluminantData1/2 from target, 2nd-illuminant look dropped when target calibration is single-illuminant. Store Remove/Restore take no CancellationToken. Tests 84/84. Owner e2e (2026-11-24): DCP (Canon EOS R5 Portrait/Landscape) + XMP (CLASSIC CHROME) for Sony ILCE-7M4 show + apply in Profile Browser; remove, Undo, re-remove OK. All 1503 Adobe Standard + 2936 camera DCPs here are dual-illuminant, no 3rd-illuminant tags: new calibration branches never hit on real files.
   - Decision (owner, 2026-11-24): assume first install. Old-app files are not detected or deleted, no elevation anywhere.
   - TODO at release: publish settings (SelfContained, PublishTrimmed, ReadyToRun) removed from Unlocker.WinUI.csproj 2026-11-23: PublishTrimmed without SelfContained fails Release build (NETSDK1102) on a clean checkout. Set them with the release format (MSIX vs exe).
   - Done (2026-11-23): tests moved to `tests/` (Unlocker.Tests + ui-tests.ps1, owner); Directory.Build.props, Directory.Packages.props, global.json moved to repo root so both trees share them.
   - Done (2026-11-23): `old-src/` + `mock-ui/` deleted (owner). Old app in git history (`cc278d2`); mock not kept. CI `.github/workflows/ci.yml` (windows-latest: build x64, test, build ARM64); Dependabot nuget + github-actions.
   - Done (2026-11-23): README rewritten for new app (download step generic until release format decided); CONTRIBUTING.md; `.github` PR template + issue forms (bug, feature, camera request, config with blank issues off).
   - Done (2026-11-23, owner): dcpTool.exe (+ iconv/libxml2/zlib dlls) removed. Own C# `DcpFile` rewrites only the 4 string tags of the DCP TIFF IFD, rest byte for byte, async + `ArrayPool` buffers. Parity vs dcpTool: 2936/2936 Adobe DCPs equal after decompile. Details: profile-format.md.
   - Done (2026-11-23, owner): DCP and XMP split into `DcpProfileInstaller`/`XmpProfileInstaller`, same flow: tag dictionary -> own `Rewrite` (shared `CameraProfileRewriter` rules + format tags) -> copy. `CameraProfileInstaller` only picks by format. XMP name now from `crs:Name` (`Camera X`, like DCP), not file name: same result on Adobe files. Author credit/signature from assembly attributes (Directory.Build.props). Static `CameraProfileRewriter`/`BrandCatalog`/`DcpFile` gone (brand families + file-name parse moved into `FileCameraCatalog`; "your brand" = brand whose models contain target camera).
   - Done (2026-11-23): package audit. Removed direct refs: `Microsoft.Extensions.DependencyInjection` (transitive via Logging), `Microsoft.Windows.SDK.BuildTools` (transitive via WindowsAppSDK.Base, resolves 10.0.26100.4654 = TFM). Kept: all CommunityToolkit.WinUI (Converters, Triggers incl. ControlSizeTrigger/IsEqualStateTrigger, SettingsControls), Logging + EventLog + Debug, WinApp + WinUIAnalyzer, MSTest.
