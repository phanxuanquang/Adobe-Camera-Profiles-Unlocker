# UX flow

Source of truth for UI (HTML mock removed 2026-11-23). Owner overrides listed in plan.md "Decisions".

Wording: always "camera profile(s)", never "look" or bare "profile" (owner, 2026-11-23). Brand family names ("Film Simulations") stay.

## Shell
`NavigationView` Left: Unlock camera profiles, Installed (count badge = `InfoBadge`), footer Settings (dot `InfoBadge` when update available). Mica backdrop. Follows Windows theme, no theme setting. Window ~1280x800 DIP, min ~760 wide; size by DPI (`GetDpiForWindow`).

## Startup (StartupViewModel)
- Loading: `ProgressRing` + "Getting ready".
- `CameraRawMissing`: full page, "Get Camera Raw" (opens `https://www.adobe.com/go/acr_installer_win`) + "Try again". Never exit app on error.
- `ProfilesMissing` (DCP or XMP folder; log names which): warning `InfoBar`, app usable.
- No admin screen (no admin needed, see adobe-paths.md).
- Update check once, background, non-blocking -> Settings.

## Unlock page
- Header: title + "Your camera" button. Opens a `SplitView` pane from the right (`PanePlacement=Right`, `DisplayMode=Overlay`, light dismiss): search box + brand filter `ComboBox` ("All brands" + each brand) + models grouped by brand. Picking closes the pane. Not a dialog (owner).
- First run (no target): inline card "Which camera do you shoot with?" + same picker. Tray hidden.
- Left pane `ListView` brands: name, family, count `selected/total`, "Your brand" tag. `AutoSuggestBox` "Search camera profiles" above searches all brands. While searching, brands with 0 matches are hidden (no "0" rows).
- Right pane: `<Brand> <Family>` title, "Copied from" `ComboBox` (default "most camera profiles"), "Select all"/"Clear choices" button (owner), tiles: `CheckBox` + name + installed check icon w/ tooltip. No black-and-white marker (owner). No descriptions.
- Search mode: results grouped by brand, empty state + "Clear search".
- Bottom tray (always visible): selected chips (removable, max 4 + "+N more"), Clear (with Undo InfoBar), accent "Install N camera profiles".
- Install: tray text "Installing i of N: <name>", Install button shows `ProgressRing` + "Installing…", Cancel. No tray hint text when nothing selected (owner). Success `InfoBar` (View installed, Undo). Failure: rollback done items, error `InfoBar` with the exception message + "Try again". Full details go to Windows Event Log (no "Copy details").

## Installed page
One `SettingsExpander` per target camera, one `SettingsCard` per camera profile. Per item delete = instant + Undo InfoBar. Bulk (per camera / Remove all) = `ContentDialog` confirm listing names, verb button "Remove N". "Open folder". Empty state with "Unlock camera profiles". Legacy items labeled, delete triggers UAC once.

## Settings
Updates (version, Download, Check again), Profile folders (open), About (GitHub, Donate).

## A11y must
Visible labels, `AutomationProperties.Name` on icon buttons, keyboard full path, focus visible, targets >= 36px (tiles 48px), High Contrast verified, status via `InfoBar` (live region).
