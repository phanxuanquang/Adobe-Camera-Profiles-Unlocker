# Adobe Camera Profiles Unlocker: Claude instructions

Windows app (WinUI 3, .NET 10) that copies Camera Raw camera profiles from one camera model to another. Maintenance mode: fix bugs, keep dependencies current, small features. Code in `src/`, tests in `tests/`, docs in `docs/` (start at `docs/README.md`).

## Every task
1. Read only the docs the task needs: `docs/README.md` has the map.
2. Before changing something that looks odd, check `docs/decisions.md`.
3. Use the matching skills (table in `docs/development.md`, "AI assistants").

## Communication
- Reply to the owner in Vietnamese. Code, comments, commits and docs in English.
- Ask only for owner-level choices (product behavior, release, cost). Otherwise pick a sensible default, say so, continue.
- Smallest correct change: standard library / platform first, no speculative abstractions.

## Hard rules
- Never modify Adobe originals under `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera\`, `...\Adobe Standard\` or `...\Settings\Adobe\`. Copy, then rewrite the copy.
- Never delete files that are not in the app manifest.
- No admin rights, no elevation. Assume a first install: old-app files are not handled.
- Layering (`docs/architecture.md`): Presentation never references a UI framework or Infrastructure. DI lifetimes per the same file.
- Before writing XAML: `winapp find-ui` / `winapp find-api`, no guessed APIs. Unknown .NET API: Microsoft Learn MCP.
- x64 + ARM64 only, never `AnyCPU`. Keep `Package.appxmanifest`.
- Commit or push only when the owner asks. Work on a branch, PR to `master`.

## Verify before "done"
- `dotnet build` 0 warnings, `dotnet test` green.
- UI change: run the app, screenshot, compare with `docs/ui.md`. Light, dark, narrow, High Contrast. Never judge UI from code alone.
- Output file change: the owner checks the Camera Raw Profile Browser.
- A fact changed -> update the doc that owns it. A choice made -> add it to `docs/decisions.md`.

## Commands
```powershell
cd src
dotnet build Unlocker.slnx                       # builds tests too
dotnet test ../tests/Unlocker.Tests
winapp run Unlocker.WinUI --detach --json         # then: winapp ui screenshot -a <pid> -o shot.png
..\tests\ui-tests.ps1 -Target local               # UI smoke batch, look at every PNG
dotnet publish Unlocker.WinUI -c Release -p:Platform=x64 -o out\x64   # release folder (CI does this per tag)
```
