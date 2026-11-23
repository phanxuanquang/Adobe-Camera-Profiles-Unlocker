# DI + lifetimes

Container: `Microsoft.Extensions.DependencyInjection`, plain `ServiceCollection`. NO Generic Host (slower cold start, unused config/logging).

```csharp
// Unlocker.WinUI/App.xaml.cs
Services = new ServiceCollection()
    .AddInfrastructure()
    .AddPresentation()
    .AddWinUI()
    .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
```

| Service | Lifetime | Why |
|---|---|---|
| `AppPaths` (record) | Singleton | Immutable |
| `ICameraCatalog` -> `FileCameraCatalog` | Singleton | Scan once, cache `Lazy<Task<...>>`. Names + paths only, never file content |
| `IInstalledStore` -> `JsonInstalledStore` | Singleton | Single source of truth for Unlock + Installed |
| `ISettings` -> `JsonSettings` | Singleton | Load once |
| `IDcpTool` -> `DcpTool` | Singleton | Stateless, process per call |
| `ICameraProfileInstaller` -> `CameraProfileInstaller` | Singleton | Stateless, deps singleton |
| `IUpdateService` -> `GitHubUpdateService` | Singleton, one shared `HttpClient` | One request per session: no IHttpClientFactory. Generic User-Agent `CameraProfilesUnlocker` |
| `IEnvironmentProbe` | Transient | Startup only |
| `ShellViewModel`, `UnlockViewModel`, `InstalledViewModel`, `SettingsViewModel` | Singleton | Keep state across tab switches |
| `StartupViewModel` | Transient | One-shot |
| `IDialogService`, `ILauncher` | Singleton | Stateless |
| `ILogger<T>` | `AddLogging()` in App.xaml.cs | Libraries reference only `Microsoft.Extensions.Logging.Abstractions`. Providers (Debug, EventLog) only in the WinUI composition root |
| `MainWindow` | Singleton (AddWinUI) | Created by DI to get `ShellViewModel`; `DialogService` needs its XamlRoot |
| Pages | NOT registered. Frame creates them; `NavigationCacheMode=Disabled` | Visual tree freed on leave. Page resolves singleton VM via `App.Services.GetRequiredService<T>()` (only service-locator spot) |

## Rules
- No Scoped (no scope in desktop app).
- Singleton never captures transient (`ValidateScopes` + review).
- Constructor injection only. `GetRequiredService` only in composition root / page factory.
- Generic registration + Mvvm source generators -> trim-safe.
- JSON: `System.Text.Json` source-gen `JsonSerializerContext` (trim/AOT safe).
- Dispose `ServiceProvider` on app exit; `JsonInstalledStore : IAsyncDisposable` empties trash.
