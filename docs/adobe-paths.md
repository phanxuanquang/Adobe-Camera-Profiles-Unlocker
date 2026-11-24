# Camera Raw paths + permissions

Verified 2026-11-23 on dev machine, Camera Raw latest, non-admin shell.

## Read (Adobe originals, read-only)
- DCP camera profiles: `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera\<Model>\<Model> Camera <Name>.dcp`
- Adobe Standard DCPs (sensor calibration source, read only): `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Adobe Standard\<Model> Adobe Standard.dcp` (1503 files, 2026-11-24)
  - ~396 model folders. Folder name = model name (`Canon EOS 1000D`).
- XMP camera profiles: `C:\ProgramData\Adobe\CameraRaw\Settings\Adobe\Profiles\Camera\<Brand>\<Model>\<Model> Camera <Name>.xmp`
  - Mostly nested per model (533 files); a few flat in brand folder (6, e.g. `Fujifilm GFX 100`). Scan recursively. Model = `crs:CameraModelRestriction`, NOT folder name. (Old app mistook brand folder for model.)
  - 451 files embed `crs:Table_<hash>`; 88 Fujifilm stubs only reference `crs:RGBTable` hash (table built into Camera Raw). Copied stubs still render (verified).
  - No Fujifilm DCP camera profiles on dev machine: Fujifilm ones come from XMP only.
- Install check: `C:\ProgramData\Adobe\CameraRaw` exists.
- Camera profile name = file name after `<Model> Camera `.

## Write (app output)
Adobe docs: DCP -> `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\`, XMP -> `C:\ProgramData\Adobe\CameraRaw\Settings\`.
App uses own subfolder: `...\CameraProfiles\Phan Xuan Quang\`, `...\Settings\Phan Xuan Quang\` (renamed from `Unlocker` 2026-11-24, owner; files already installed under `Unlocker` stay tracked by full path in the manifest). `Unlocker` subfolders verified 2026-11-23 (owner, Profile Browser, Sony ILCE-7M4 raw): both subfolders scanned, XMP + DCP copies show and apply.

NOT `%AppData%\Adobe\CameraRaw\Settings` (per-image settings DB). Old app also wrote `%AppData%\...\CameraProfiles`: drop.

## Permissions: no admin needed
ACL `ProgramData\Adobe\CameraRaw\CameraProfiles`:
- `BUILTIN\Users`: ReadAndExecute + Write (create files)
- `CREATOR OWNER`: GENERIC_ALL -> user fully owns files it creates (overwrite/move/delete OK, tested)
- Adobe original files: write DENIED for non-admin (tested)

Rules:
- App runs as normal user. No `requireAdministrator`.
- Never write Adobe originals. Copy then rewrite copy.
- No elevation. Old-app files (admin-owned `* (for *).dcp` in `CameraProfiles` root) are not detected or deleted: owner decision 2026-11-24, assume first install.

## App-own data
- `%LocalAppData%\CameraProfilesUnlocker\settings.json`: target camera
- `%LocalAppData%\CameraProfilesUnlocker\installed.json`: manifest of created files
- Both JSON files: written to `<file>.tmp` then `File.Move(overwrite)` (atomic on same volume). Crash/cancel never leaves half-written JSON. Plain JSON (System.Text.Json source-gen), no SQLite/EF Core: tiny data, one process, whole-file read/write (owner asked 2026-11-23, declined).
- `%LocalAppData%\CameraProfilesUnlocker\trash\`: removed files kept for Undo, emptied on exit
