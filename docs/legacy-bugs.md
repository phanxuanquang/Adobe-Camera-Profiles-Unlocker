# Old app bugs: never repeat

Old app = WinUI 3 Neo + `Engineer` lib (net8.0), all logic in `MainWindow.xaml.cs`. Folder removed 2026-11-23; source stays in git history on `master` (commit `cc278d2`).

| Bug | Where | New rule |
|---|---|---|
| Writes Adobe original XMP in place | `MainWindow.xaml.cs` `ContentModifier.ModifyXmp(newProfile, ...)` | Copy first, rewrite copy |
| Reset deletes ALL `.dcp` in shared folder (other apps' / user's too) | `ResetButton_Click` | Delete only manifest-tracked files |
| Required admin | `IsUserAdmin` gate | Not needed (adobe-paths.md) |
| Init runs on every `Activated` (dup list, repeat update check) | `MainWindow_Activated` | Run once at startup |
| Substring match picks wrong profile (`Standard` hits `Standard v2`) | `FirstOrDefault(dir => dir.Contains(profile))` | Keep full `CameraProfile` record with path |
| Validation `||` always passes | `ModelDirs.Exists(... Contains(out) || Contains(in))` | Picker from list; no free text |
| dcpTool relative path, temp in app dir, exit code ignored | `DcpHelper` | `AppContext.BaseDirectory`, `%TEMP%`, check exit |
| Line-based XML edit; `Contains("Copyright")` replaces any line | `ContentModifier` | `XDocument` |
| XMP folder (brand) treated as camera model | `GetFolders(VariantProfileFoldersDirAlt, false)` | Model from `crs:CameraModelRestriction` |
| XMP copies keep same `crs:UUID` | n/a | New UUID |
| Every error = modal then `Application.Current.Exit()` | everywhere | Inline state + retry |
| Donate link inside success dialog | `successDialog` | Settings > About only |
| Octokit for one GET; DataGrid toolkit; manual OS version check | csproj | `HttpClient`; `ListView`/`GridView`; `TargetPlatformMinVersion` |
