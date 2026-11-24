# Adobe Camera Profiles Unlocker: Claude instructions

Rewrite of Windows app that copies camera profiles (Camera Raw) between camera models. Code `src/`. Old app removed (git history on `master`, commit `cc278d2`). UI spec = `docs/ux-flow.md` (HTML mock removed).

## Start every session
1. Read `docs/plan.md`: find current step.
2. Read only docs that step needs (index `docs/README.md`).
3. Invoke matching skills BEFORE acting (`docs/tooling.md`). Skills are mandatory, not optional.

## Communication
- Reply to owner in Vietnamese. Code, comments, commits, docs in English.
- Caveman terse (caveman skill on). Ponytail scope (ponytail skill on): smallest correct change, stdlib/platform first, no speculative abstractions.
- Ask only for owner-level decisions (listed in plan.md "Open questions"). Otherwise pick sensible default, state it, continue.

## Hard rules
- Never modify Adobe originals under `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Camera\` or `...\Settings\Adobe\`. Copy, rewrite copy.
- Never delete files not in app manifest.
- App runs non-admin. No elevation. Assume first install: no old-app files handled.
- Layering per `docs/architecture.md`: Presentation never refs UI framework or Infrastructure.
- DI lifetimes per `docs/di-lifetimes.md`.
- Before writing XAML: `winapp find-ui` / `winapp find-api` (no guessed APIs). Unknown .NET API: Microsoft Learn MCP.
- Never `AnyCPU`. Keep `Package.appxmanifest`.
- No commit / push unless owner asks. Branch off `master` first.

## Verify before "done"
- `dotnet build` 0 warnings, `dotnet test` green.
- UI: run app (`winapp run`), screenshot, check against `docs/ux-flow.md`. Judge visually, never from code alone. Light, dark, narrow, High Contrast.
- Profile output: check Camera Raw Profile Browser shows look (ask owner to confirm when needed).
- Update `docs/plan.md` status + decisions. New durable fact -> correct docs file (one topic per file), caveman English.

## Commands
```powershell
cd src
dotnet build Unlocker.slnx                      # builds tests too
dotnet test ../tests/Unlocker.Tests
winapp run Unlocker.WinUI --detach --json        # then: winapp ui screenshot -a <pid> -o shot.png
grep -rn "TODO(step 2)" .                         # work items of current step
..\tests\ui-tests.ps1 -Target local            # UI smoke batch, look at every PNG
```
