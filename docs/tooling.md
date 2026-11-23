# Tooling map: use it, don't guess

Installed on dev machine (2026-11-23): .NET SDK 6.0.428 / 8.0.428 / 11.0.100 (targets net10.0), WinApp CLI 0.7.1, Python (for `http.server`).

## Skills (invoke via Skill tool BEFORE doing task)
| Task | Skill |
|---|---|
| Any new feature / creative step | `superpowers:brainstorming` (short), then `superpowers:writing-plans` if multi-step |
| Executing plan.md step | `superpowers:executing-plans` |
| Write code with tests | `superpowers:test-driven-development` |
| Bug / test fail / build error | `superpowers:systematic-debugging` |
| Before claiming done | `superpowers:verification-before-completion` |
| Scope every code change | `ponytail:ponytail` (always on). Review bloat: `ponytail:ponytail-review` |
| Terse output | `caveman:caveman` (always on). Internal chatter: `anthropic-skills:caveman-mode` |
| Toolchain check / repair | `winui:winui-setup` |
| Create / build / run / crash WinUI | `winui:winui-dev-workflow` (`winapp new`, `winapp run . --detach --json`, WinUI analyzer pkg) |
| Author / review XAML, pick controls | `winui:winui-design` (+ `winappcli:winapp-find-ui`, `winappcli:winapp-find-api`) |
| Review before commit | `winui:winui-code-review`, `code-review`, `simplify` |
| UI automation tests, screenshots of real app | `winui:winui-ui-testing`, `winappcli:winapp-ui-automation` |
| Isolated test run | `winappcli:winapp-sandbox` |
| MSIX / signing / release | `winui:winui-packaging`, `winappcli:winapp-package`, `winappcli:winapp-signing`, `winappcli:winapp-manifest` |
| Future WPF frontend | `winui:winui-wpf-migration` (control map, reverse direction), `winappcli:winapp-frameworks` |
| Test project setup / run / filters | `dotnet-test:dotnet-test-frameworks`, `dotnet-test:run-tests`, `dotnet-test:filter-syntax`, `dotnet-test:writing-mstest-tests` |
| Test quality | `dotnet-test:test-anti-patterns`, `dotnet-test:assertion-quality` |
| Directory.Build.props / CPM | `dotnet-msbuild:directory-build-organization`, `dotnet-nuget:convert-to-cpm` |
| Build failure deep dive | `dotnet-msbuild:binlog-generation` -> `dotnet-msbuild:binlog-failure-analysis` |
| Perf / allocations | `dotnet-diag:analyzing-dotnet-performance`, `dotnet-diag:microbenchmarking` |
| Unknown .NET/WinAppSDK API | `microsoft-docs:microsoft-docs`, `microsoft-docs:microsoft-code-reference` |
| Security pass (file writes, process launch, elevation) | `security-review` |
| Commit | `commit-commands:commit` (only when owner asks) |

## winapp CLI (key commands)
```powershell
winapp find-ui "<intent>"                  # real WinUI Gallery/Toolkit samples
winapp find-ui --id <id> --json
winapp find-api check-property InfoBar Severity IsOpen --json --project-dir src/Unlocker.WinUI
winapp find-api members NavigationView --filter selected --json --project-dir src/Unlocker.WinUI
winapp run src/Unlocker.WinUI --detach --json
winapp ui list-windows ; winapp ui inspect -a <App> ; winapp ui screenshot -a <App> -o shot.png
```
Run `find-api` from restored project (`--project-dir`), not repo root.

## MCP servers
- **Microsoft Learn** (`mcp__claude_ai_Microsoft_Learn__*`): docs search/fetch + code samples for .NET 10, WinAppSDK, CommunityToolkit.Mvvm. Use before guessing API.
- **IDE** (`mcp__claude-vscode__getDiagnostics`, `LSP`): compiler diagnostics without full build.

## Agents (Agent tool) when work is broad / parallel
- `winui:winui-dev`: WinUI feature build-out.
- `winappcli:winapp`: packaging, manifest, UI automation.
- `dotnet-test:code-testing-generator`: bulk test generation.
- `dotnet-msbuild:msbuild`: build/project-file issues.
- `dotnet-diag:optimizing-dotnet-performance`: perf audit.
- `Explore` / `caveman:cavecrew-investigator`: locate code. `caveman:cavecrew-reviewer`: diff review.
Brief agents with file paths + doc links; don't delegate one-file edits.

## Verification habits
- UI = look at it: real app screenshot (`winapp ui screenshot`) vs ux-flow.md, light + dark + narrow + High Contrast. Never judge UI from code only.
- Build clean (`dotnet build -warnaserror`), `dotnet test` green before saying done.
- UI smoke batch: `.\tests\ui-tests.ps1 -Target local` (skill `winui:winui-ui-testing`). 16 checks: startup, select, real install of 1 XMP, remove + Undo, bulk-remove dialog, empty states, search, search hides brands, "Your camera" right pane, settings, AutomationId audit. Writes `tests\test-results.json` + PNGs in `tests\ui-test-artifacts\` (git-ignored): look at every PNG.
  - Local, not Sandbox: guest has no Camera Raw. Preconditions: "Your camera" set, nothing installed by app. Cleans up its own install.
  - Selectors = AutomationIds from `x:Name` / `AutomationProperties.AutomationId`. Every interactive control needs one (audit fails otherwise).
  - Launching with `winapp run --detach` from a short-lived shell can kill the app when the shell exits; run the batch in one call.
