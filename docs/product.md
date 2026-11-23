# Product

Windows desktop app. Copies camera-maker color profiles ("camera profiles") from one camera model to another, so Camera Raw / Lightroom / Photoshop show them for user's camera.

Example: Fujifilm `CLASSIC CHROME` usable on Sony ILCE-7M4 raw files.

## Glossary
- **Camera profile** = one camera-matching color profile (Fujifilm `CLASSIC CHROME`, Canon `Portrait`). Only UI word (never "look"). Code types `CameraProfile`, `InstalledCameraProfile`.
- **Brand** = camera maker. Family names: Fujifilm Film Simulations, Canon Picture Styles, Nikon Picture Controls, Sony Creative Looks, Panasonic Photo Styles, Leica Film Styles. Unknown brand -> "Camera Profiles".
- **Source model** = camera whose camera profiles get copied. Default = brand model with most camera profiles.
- **Target / Your camera** = user's camera. Set once, persisted.
- **DCP** = DNG Camera Profile, binary. Edited via `dcpTool.exe` (DCP <-> XML).
- **XMP profile** = text XML, `crs:` namespace. Fujifilm, Nikon, Panasonic, Ricoh, Sigma newer camera profiles.
- **Installed camera profile** = file app created. Tracked in manifest. Only these may be removed.

## Users
Photographers, non-technical. Windows 10 1809+ / 11. Camera Raw installed (ships with Lightroom Classic / Photoshop; both reuse Camera Raw profiles). Must restart Adobe app to see new camera profiles.

## Out of scope
Mac. Editing camera profile content. Lens profiles. Presets. Localization (English only for now; keep strings in one place so it can be added).
