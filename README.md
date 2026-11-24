# © 2024 Phan Xuan Quang / Adobe Camera Profiles Unlocker

A Windows app that copies camera profiles (camera-matching color profiles such as Fujifilm *Classic Chrome* or Canon *Portrait*) from one camera model to another, so Adobe Camera Raw, Lightroom Classic and Photoshop show them for **your** camera.

![Pick camera profiles from other brands and install them for your camera](.github/assets/unlock.png)

## :sparkles: Features
- Pick your camera once. The app remembers it.
- Browse camera profiles by brand, or search across all brands.
- Choose which model to copy from ("Copied from"). The model with the most camera profiles is picked by default.
- Install many camera profiles at once, with Cancel and Undo.
- See and remove what you installed, per camera. Removing also has Undo.
- No install and no admin rights needed. Adobe's original files are never changed.
- Follows the Windows light or dark theme.

## :pushpin: Supported camera brands
Canon, Fujifilm, Leica, Nikon, Olympus, OM Digital Solutions, Panasonic Lumix, Pentax, Ricoh, Sigma and Sony.

What you see in the app depends on the Camera Raw version installed on your PC.

## :computer: System requirements

| | Minimum | Recommended |
|---|---|---|
| Windows | Windows 10 version 1809 (x64 or ARM64) | Windows 11 |
| Adobe Camera Raw | 15.0 | [Latest version](https://www.adobe.com/go/acr_installer_win) |
| Adobe Photoshop or Lightroom Classic | 2022 | Latest version |

## :blue_book: How to use

### Windows
1. Download the zip for your PC from the [Releases page](https://github.com/phanxuanquang/Adobe-Camera-Profiles-Unlocker/releases/latest): `win-x64` for most PCs, `win-arm64` for ARM PCs (for example Snapdragon).
2. Extract the zip to a folder and run `Adobe Camera Profiles Unlocker.exe`. No install needed.
   The app is not code-signed, so Windows SmartScreen may warn on first run: select **More info**, then **Run anyway**.
3. Pick **your camera**.
4. Pick a brand, tick the camera profiles you want, then select **Install**.
5. Restart Photoshop or Lightroom.
6. In Camera Raw or Lightroom, open the **Profile** browser. The new camera profiles are listed as `<Source camera>: <Profile name>`.

| Pick your camera | Manage installed camera profiles |
|---|---|
| ![Your camera pane with search and brand filter](.github/assets/camera.png) | ![Installed page grouped by camera](.github/assets/installed.png) |

To remove camera profiles, open the **Installed** page.

### macOS (manual)
The app runs on Windows only. To use the camera profiles on a Mac:
1. Install them on a Windows PC for the Mac user's camera.
2. Copy `C:\ProgramData\Adobe\CameraRaw\CameraProfiles\Phan Xuan Quang` to `/Library/Application Support/Adobe/CameraRaw/CameraProfiles/` on the Mac.
3. Copy `C:\ProgramData\Adobe\CameraRaw\Settings\Phan Xuan Quang` to `/Library/Application Support/Adobe/CameraRaw/Settings/` on the Mac.
4. Restart Photoshop or Lightroom.

## :exclamation: Important notes
- Camera profiles apply to **raw** files only (for example `.ARW` for Sony, `.NEF` for Nikon).
- You **must** restart Adobe apps after you install or remove camera profiles.
- Installed files go to the `Phan Xuan Quang` subfolders of `C:\ProgramData\Adobe\CameraRaw\CameraProfiles` and `C:\ProgramData\Adobe\CameraRaw\Settings`, named `<Source camera> <Profile name> (for <Your camera>)`.
- To uninstall: remove your camera profiles on the **Installed** page first, then delete the app folder and `%LocalAppData%\CameraProfilesUnlocker`.

## :handshake: Contributing
Bug reports, ideas and pull requests are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) first; maintainer docs are in [docs/](docs/README.md).

## :gift: Support me

<a href="https://i.imgur.com/9YpRPQs.jpeg" target="_blank">
  <img src="https://i.imgur.com/IUK9CFo.png" height=72 alt="Vietcombank" />
</a>
<a href="https://me.momo.vn/phanxuanquang" target="_blank">
  <img src="https://i.imgur.com/0r18xHl.png" height=72 alt="Momo" />
</a>
<a href="https://i.imgur.com/00NqiL8.jpeg" target="_blank">
  <img src="https://i.imgur.com/PSCoduQ.png" height=72 alt="ZaloPay" />
</a>
