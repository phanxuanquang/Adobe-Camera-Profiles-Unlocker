# Decisions

Choices that look odd without context. Read the relevant entry before "fixing" one of them. Add a new entry when you make a choice someone could reasonably undo. The step-by-step history of the 2026 rewrite is in the git log of the `migration` branch; the old app's code is at commit `cc278d2`.

## Product

| Decision | Why |
|---|---|
| One word, **camera profile**, in UI and code. | Users mixed up "look", "profile" and "preset". |
| No admin rights, no elevation, ever. | Users can write to Camera Raw's profile folders and own what they create. Files from the old admin app are **not** handled: we assume a first install. |
| Output in our own subfolder `Phan Xuan Quang`, file name `<Source> <Name> (for <Target>)`. | Keeps our files apart from Adobe's; the source model in the name lets two brands' "Portrait" coexist. |
| Author credit and calibration signature are written into every copy (`Directory.Build.props`). | Owner keeps the branding of the original app. |
| App follows the Windows theme; no theme setting. | Less UI, matches the OS. |
| No update check at startup; Settings checks each time it opens and links to the release page. | No network call on launch; users download by hand. |
| No black-and-white marker, no camera profile descriptions. | Owner: noise. |
| English only for now. | Small audience; strings stay in one place so localization stays possible. |

## Camera profile output

| Decision | Why |
|---|---|
| Own C# `DcpFile` instead of dcpTool. | No native exe in the app. Rewrites only 4 string tags; every other entry byte for byte. Verified identical on all 2936 Adobe DCPs. |
| DCP takes the target camera's calibration from its Adobe Standard; keeps the source's look. | Matches how Adobe pairs calibration and look. See [camera-profiles.md](camera-profiles.md#dcp-binary). |
| Adobe Standard: `v2` before v1; found by `UniqueCameraModel` when the file name differs. | v2 is Adobe's newer calibration of the same camera. Some file names are not the model name, so a name-only lookup silently kept the source calibration. |
| XMP base = `Camera Standard` if the target has one, else `Adobe Standard`. | Some targets (Olympus, OM, some Sony/Pentax/Leica) have no Camera Standard; before this they rendered with no base. |
| XMP base: keep the source's base when the target has it (as a DCP, or built in for the same brand); `Camera Standard v2` before v1; no base added when the source has none. | Fujifilm to Fujifilm lost PROVIA, Nikon v2 targets got v1, and RICOH GR IV Monochrome copies gained a base the original never had. Rules: [camera-profiles.md](camera-profiles.md#xmp-text). |
| XMP built-in base kept only from a list (`Camera PROVIA/Standard`), not "any base with no DCP". | v3.1.1 kept any base without a file; Nikon D780's `Camera Standard v2` exists nowhere, so copies to other Nikons pointed at a missing base. |
| New UUID for every XMP copy. | Same UUID as the original collides in Camera Raw. |
| Install sequentially. | Disk bound; parallel buys little and complicates cancel and rollback. |
| `ArrayPool` buffers for DCP reads. | Owner asked for low memory use; avoids large-object-heap allocations per install. |

## Code structure

| Decision | Why |
|---|---|
| Four projects, Presentation free of UI and Infrastructure. | The UI framework can be swapped (e.g. WPF) without touching ViewModels. |
| No mediator, repositories or use-case classes. | Use cases fit in ViewModels; add a layer only for a second caller. |
| Plain `ServiceCollection`, no Generic Host, no `appsettings.json`. | Faster cold start; no value needs runtime config. |
| Plain JSON files, no SQLite / EF Core. | Tiny data, one process, whole-file read and write. |
| Store remove/restore are not cancellable. | A cancel between file moves and the manifest save would desync them. |
| `StatusActionTemplate` is duplicated per page; the atomic-save block is duplicated in the two JSON stores. | Sharing needs a code-behind ResourceDictionary / a new class: more code than the duplication. |
| Plain WinUI `InfoBar`, no custom status control or notification service. | Built-in control covers it. |
| Errors show the exception message; details go to the Windows Event Log (source `.NET Runtime`). | A custom event source needs admin to register. No "Copy details" button. |

## Build and release

| Decision | Why |
|---|---|
| x64 + ARM64 only, never AnyCPU. | WinUI native parts are per architecture. |
| `WindowsAppSDKSelfContained` always on. | F5 / unpackaged runs fail with `REGDB_E_CLASSNOTREG` without the runtime in the output. |
| Release = unpackaged, self-contained zip per architecture on GitHub Releases, built by `release.yml` from a `v*` tag. | No signing certificate needed, same channel as the old app, matches the in-app update check. MSIX would need a trusted certificate. |
| ReadyToRun on, trimming off, no single file. | ReadyToRun: faster start for +15 MB zip. Trimming breaks WinUI/Toolkit reflection. WinUI does not support single-file. |
| `Microsoft.WindowsAppSDK` metapackage kept although it ships ~40 MB of unused AI/ML runtimes. | Switching to `Microsoft.WindowsAppSDK.WinUI` lets CommunityToolkit pull Windows App SDK 1.6 and breaks the build. Retry when the Toolkit moves to 2.x. |
| Not code-signed yet. | Cost. README tells users "More info > Run anyway". |

## Website

| Decision | Why |
|---|---|
| Static `site/` on GitHub Pages, libraries from CDNs (pinned + SRI), no build step or npm. | One page that changes rarely; nothing to install or update locally, and the Pages workflow just uploads the folder. |
| Download data read live from the GitHub API, with every link defaulting to `/releases/latest`. | No version to bump on the site at release time; an API failure never blocks a download. |
| FAQ uses native `<details>` styled as WinUI Expanders; screenshots zoom in a native `<dialog>`. No Bootstrap JS. | Keyboard, Esc, focus return and find-in-page work with no extra script. |
| Fluent (WinUI 3) look, light theme only, accent `#0078D4`. | Owner: the site should look like the app and Windows. |
| No macOS section. | Owner: the app is Windows only; the manual Mac steps stay in the README. |
| One donation QR code, `site/assets/img/qr-vietcombank.png`, used by both the site and the README. | Owner: Vietcombank only. One copy to replace; no third-party image hosting. |
| English only. | Same as the app. |

## App icon

| Decision | Why |
|---|---|
| Logo: one landscape split by a tilted gap, violet on the left, coral on the right (same photo, two camera profiles). iOS-style continuous corners, no baked shadows. | Owner's pick after rounds against the Windows and Apple app-icon guidelines; at least half the pixels pass 3:1 on light and dark taskbars. |
| Only the icon files the repo references are generated, not the full MSIX `targetsize-*` set. | The app ships as an unpackaged zip; extra MSIX assets would never be used. |
