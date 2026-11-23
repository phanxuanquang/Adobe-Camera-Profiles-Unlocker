# Conventions

## C#
- File-scoped namespaces. `sealed` by default. `internal` impls. Records for models.
- Nullable on, warnings = errors. No `!` suppression without comment.
- Async all I/O, suffix `Async`, last param `CancellationToken ct = default`. No `.Result` / `.Wait()` / `async void` (except XAML event handlers, none expected: use commands).
- No `Parallel.*` for file lists of this size. Plain `foreach` / LINQ.
- Stdlib first: `XDocument`, `System.Text.Json` (source-gen), `HttpClient`, `Directory.EnumerateFiles`. No new package without reason.
- Deliberate shortcut -> `// ponytail: <ceiling>, <upgrade path>`.
- Comments explain why, not what. Only when needed: non-obvious constraint, workaround, safety rule. No doc-pointer or "Singleton." comments (lifetimes live in di-lifetimes.md).
- One type per file (class, record, interface, enum). File name = type name. Folder = namespace.

## MVVM (CommunityToolkit.Mvvm)
- `partial` VMs, `[ObservableProperty]` (partial property form), `[RelayCommand]`.
- VM ctor = interfaces only. No UI types in Presentation.
- Strings user sees live in VM / XAML; wording per ux-flow.md.

## XAML (WinUI)
- Pages/UserControls set `x:DefaultBindMode="OneWay"` (WinUI analyzer WUI2011 fails build on implicit OneTime). Immutable record data: explicit `Mode=OneTime` (else WMC1506 warning). `TextBox` two-way -> `UpdateSourceTrigger=PropertyChanged`.
- `{ThemeResource}` brushes only, no hex colors. Light/Dark/HighContrast.
- Reuse before writing: CommunityToolkit.WinUI Converters/Triggers + SettingsControls (`SettingsCard`, `SettingsExpander`). Converters keyed in `App.xaml` (`Not`, `CollapsedWhenTrue`, `VisibleWhenAny`...). x:Bind bool -> Visibility is implicit, no converter.
- Shared layout/geometry resources + styles in `App.xaml` (`PagePadding`, `PageSpacing`, `ItemSpacing`, `CardGridStyle`, `TileGridStyle`, `SecondaryTextStyle`, `IconButtonStyle`). No literal paddings/colors/radii in pages.
- Geometry (learn.microsoft.com/windows/apps/design/signature-experiences/geometry): page cards/dialogs `OverlayCornerRadius` (8), in-page controls/tiles/chips `ControlCornerRadius` (4), never pills. Inner radius <= outer.
- Gotchas (hit in step 5):
  - `ScrollViewer > StackPanel(MaxWidth, Margin)` reports desired width > window: NavigationView grows, right side clipped. Use `ScrollViewer > Grid(Padding) > Panel(MaxWidth)` (toolkit Settings sample).
  - Buttons inside `SettingsExpander` ItemTemplate get `DataContext = null`. Pass item with `Tag="{x:Bind Mode=OneTime}"`, read `Tag` in handler.
  - `Window` is not a FrameworkElement: x:Bind in `MainWindow.xaml` can't use resource converters (CS1503 in generated code). Set those in code.
  - x:Bind indexer on `IReadOnlyList` (`List[0]`) -> XAML compiler WMC9999 internal error. Set in code-behind.
  - XAML compiler "WMC9999 Could not find any resources" masks real error: bisect the last XAML edit.
  - x:Bind casts only pick the type of a path step; enum-to-enum casts fail (WMC9999). Map VM enums to control enums with `ui:IsEqualStateTrigger` + `Setter`, no C# helper.
  - `VisualStateManager.VisualStateGroups` (state triggers) work only on the page root element. Wrapping the root (e.g. in `SplitView`) silently disables them: move the groups up.
- No `ScrollViewer` around `ListView`/`GridView`. No custom ControlTemplate when built-in fits.
- Icon buttons: `AutomationProperties.Name` + `ToolTipService.ToolTip`.
- Verify controls/APIs with `winapp find-ui` / `winapp find-api` before writing (tooling.md).

## TODO markers
- Skeleton uses `TODO(step N):` where N = plan.md step. Find work: `grep -rn "TODO(step 2)" src`.
- Stubs that would run at startup return safe defaults; action stubs throw `NotImplementedException("TODO(step N)...")`.
- Remove the TODO comment when implemented. Step done = `grep "TODO(step N)"` returns nothing.

## Tests
- One test project, folders mirror layers: `Core/`, `Infrastructure/`, `Presentation/` (one test class per subject), `Fakes/` (one fake per file), `Support/` (`TempFolderTest` base, `TestFiles`), `Fixtures/`.
- File-system tests derive from `TempFolderTest` (temp paths, never real Camera Raw folders). Installer uses `FakeDcpTool`; `DcpToolTests` run the real exe.
- No mocking framework unless fakes get painful; hand-written fakes.
- Run: `dotnet test`.

## Git
- Branch off `master` before commit; commit only when asked.
- `.gitignore` ignores `.remember`, `.playwright-mcp`, UI test output in `tests/`.

## Logging
- `ILogger<T>` (constructor injection) in Infrastructure, Presentation and WinUI services. Libraries reference only `Microsoft.Extensions.Logging.Abstractions`; providers live in `App.xaml.cs`.
- Providers: Debug (Information+) and Windows Event Log (Warning+, source ".NET Runtime" under Application log; a custom source would need admin to create). Users read failures in Event Viewer; the UI shows only the exception message.
- Log what a user or maintainer needs after a failure: swallowed exceptions (Warning), failed operations (Error, with exception), key state changes (Information: catalog scan counts, installs, removals, target camera). No logging in pure Core rules or per-item hot loops.
- Structured templates (`"Installed {Name} for {Target}"`), never interpolated strings.
- Unhandled exceptions: `App.UnhandledException` -> `LogCritical`.

## CancellationToken
- Every async I/O method takes `CancellationToken ct = default` and passes it on. `[RelayCommand]` methods take `CancellationToken` so commands can cancel.
- Deliberate exceptions: store remove/restore and recording an installed file use no token (`CancellationToken.None`): a cancel between file moves and the manifest save would leave them out of sync. Dialogs are user-driven: no token.
