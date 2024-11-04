# © 2024 Phan Xuan Quang / Adobe Camera Profiles Unlocker

A tool to unlock hidden Camera Profiles in Adobe Lightroom and Adobe Camera Raw, enabling enhanced color matching for a wide range of camera brands.

## :pushpin: Supported Camera Brands
The complete list of supported camera models can be found [**HERE**](https://github.com/phanxuanquang/Adobe-Camera-Profiles-Unlocker/blob/master/Supported%20Camera%20Models.md).

| No. | Brand Name            |
|-----|------------------------|
| 01  | Olympus               |
| 02  | OM Digital Solutions  |
| 03  | Panasonic Lumix       |
| 04  | Pentax                |
| 05  | Ricoh GR              |
| 06  | Sigma fp              |
| 07  | Sony                  |
| 08  | Canon                 |
| 09  | Leica                 |
| 10  | Nikon                 |
| 11  | Fujifilm              |

## :computer: System Requirements

|                       | Minimum Version                  | Recommended Version                                      |
|-----------------------|----------------------------------|----------------------------------------------------------|
| Windows        | Windows 10 version 1809         | Windows 10 version 2004 or Windows 11                    |
| Adobe Camera Raw  | Version 15.0                    | [Latest version](https://www.adobe.com/go/acr_installer_win) |
| Adobe Photoshop or Adobe Lightroom Classic | Version 2022 | Latest version                                           |

## :blue_book: Usage Instructions

### For Windows Users
1. **Download**: Get the **Adobe.Camera.Profiles.Unlocker.Neo.zip** file from the latest [release page](https://github.com/phanxuanquang/Adobe-Camera-Profiles-Unlocker/releases/latest).
2. **Watch the Tutorial**: Follow the video tutorial linked below.
   [![Guidance video](https://i.imgur.com/cbNApyi.png)](https://vt.tiktok.com/ZSY2vmhyH)
3. **Restart Adobe Applications**: Restart Adobe Photoshop or Lightroom to apply the changes.

### For macOS Users

1. **Prepare Profiles on Windows**: Follow the instruction video for Windows above.
2. **Transfer Profiles to Mac**: Copy the `.dcp` files from `C:\Users\%username%\AppData\Roaming\Adobe\CameraRaw\CameraProfiles` on the Windows device to `/Library/Application Support/Adobe/CameraRaw/Settings/Adobe/Profiles` in the target Mac.
3. **Restart Adobe Applications**: Restart Adobe Photoshop or Lightroom to apply the changes.

## :exclamation: Important Notes

- Profiles specific to Adobe and Fujifilm are only supported on **Windows**.
- Profiles apply **exclusively** to RAW photo formats (e.g., `.ARW` for Sony, `.NEF` for Nikon).
- Restarting Adobe applications after profile installation is **mandatory** to activate the new profiles.
- Profiles can be found under `Camera Matching` or `Profiles` in the **Camera Profile** section of Adobe Camera Raw or Lightroom.
- Using *the latest version of Adobe Camera Raw* is recommended over Lightroom for optimal performance.
