# Profile formats + rewrite rules

Both formats = same flow: installer reads file -> tag dictionary -> `Rewrite(tags, sourceModel, targetModel)` -> writes copy.
- Shared rules: `Unlocker.Core/Rules/CameraProfileRewriter` (sealed, DI): `Name` "Camera X" -> "<SourceModel>: X" (unique in Profile Browser), `Copyright` = author credit (added when missing), `CameraModelRestriction` = target. Missing tags stay missing. Also `OutputFileName`.
- Format rules: `Rewrite` method of `DcpProfileInstaller` / `XmpProfileInstaller` (Infrastructure/Installing) = shared rules + own tags. `CameraProfileInstaller` picks by format.
- Shared key names (`Name`, `Copyright`, `CameraModelRestriction`); each installer maps its own tags to them. Never line-based string replace (old bug). Source never modified; output always new file.

Author credit = assembly `Copyright` (`© 2024 Phan Xuan Quang / Github: @phanxuanquang`), signature = `AssemblyMetadata("Author")` = `Authors`. Both set once in root `Directory.Build.props` (also the app's file properties). Owner decision: keep branding (Copyright + signature overwrite).

## DCP (binary)
Format (DNG spec "Camera Profiles"; checked on all 2936 Adobe DCPs here): TIFF, header `IIRC` (little endian, magic 0x4352 "RC" instead of 42),
IFD offset 8, one IFD, no SubIFDs, no pointers inside values. So every entry is self-contained.

`DcpProfileInstaller` + `DcpFile` (own C#, replace dcpTool.exe 2026-11-23):
1. Read source async into `ArrayPool` buffer (no LOH alloc per install). `DcpFile.Parse`: IFD entries (tag, type, count, slice of source buffer, no copy). Not `IIRC` / out of bounds -> `InvalidDataException` (big-endian `MMCR` not supported: no Adobe DCP uses it).
1b. Calibration (owner, 2026-11-24): read `<Target> Adobe Standard.dcp` (`AppPaths.AdobeStandardFile`), `DcpFile.CopyCalibrationFrom` swaps in its `ColorMatrix1/2` + `CalibrationIlluminant1/2` and drops the source's DNG 1.6 third-illuminant set. Source keeps `ForwardMatrix`, look table, tone curve, exposure offset, black render. Missing Adobe Standard: Warning log, source calibration kept (old behavior).
   Why (decompiled 2026-11-24, Nikon Z f / Fujifilm X-T30 III Adobe Standard, Nikon Z f Camera Standard, Sony ILCE-7RM5 Camera FL): a camera-matching DCP has the same `ColorMatrix` as that camera's Adobe Standard, and a `ForwardMatrix` identical across cameras (`0.7978 0.1352 0.0313 ...`). The camera-specific look lives in the 90x16x16 look table + tone curve. So target Adobe Standard matrices + source look = the calibration Adobe itself would pair with that look. Limit: the look table still holds source-sensor tweaks (baked in, not separable).
2. `GetStrings`: 4 string tags as dictionary:
   `CameraModelRestriction` (0xC614 UniqueCameraModel), `ProfileCalibrationSignature` (0xC6F4), `Name` (0xC6F8 ProfileName), `Copyright` (0xC6FE ProfileCopyright).
3. `DcpProfileInstaller.Rewrite`: shared rules + `ProfileCalibrationSignature`: `com.adobe` -> author signature. `SetStrings` puts tags back.
4. `WriteTo` pooled buffer, written async: header `IIRC`, IFD at 8 sorted by tag, out-of-line values word aligned after IFD. Strings NUL-terminated, type ASCII (2) when ASCII else BYTE (1) UTF-8 (same as DNG SDK `tag_string`). All other entries copied byte for byte (dcpTool re-serialized everything via DNG SDK).
5. Out name: `<Name> (for <Target>).dcp`. No temp files.

- Parity (2026-11-23): all 2936 Adobe DCPs rewritten by C#, then decompiled by dcpTool, equal to dcpTool XML of source + same edits.
- dcpTool source (GPLv2, sourceforge.net/projects/dcptool) only read for reference; `DcpFile` is written from the TIFF/DNG layout, no code copied.
- Install camera profiles sequentially (disk bound; parallel buys little, complicates cancel).

## XMP (text)
`crs` ns = `http://ns.adobe.com/camera-raw-settings/1.0/`. Real sample fields (attributes on `rdf:Description`):
`PresetType="Look"`, `UUID`, `CameraModelRestriction`, `Copyright`, `CameraProfile` (base DCP, e.g. `Camera PROVIA/Standard`), `RGBTable` (hash -> embedded table), `SupportsMonochrome`, `ConvertToGrayscale`.
Child elements: `crs:Name` / `crs:ShortName` / `crs:Group` (each `rdf:Alt/rdf:li`).

`XmpProfileInstaller` (`XDocument` only, never text replace): tags = `crs:` attributes `Copyright`, `CameraModelRestriction`, `UUID`, `CameraProfile` + `Name` (first `crs:Name/rdf:Alt/rdf:li`; Adobe files: one `x-default` li, value `Camera <Name>` like DCP). All li get the new name.
`XmpProfileInstaller.Rewrite` = shared rules +:
- `UUID` = new `Guid.NewGuid().ToString("N").ToUpperInvariant()`. Same UUID as original = collision.
- `CameraProfile` (base the look sits on, found by name for the target camera) = `Camera Standard` when `<Target>\<Target> Camera Standard.dcp` exists (323 of 396 camera-matching folders, 2026-11-24), else `Adobe Standard` (every supported camera). Owner, 2026-11-24. Before: always `Camera Standard`, so targets without it (Olympus, OM, some Sony/Pentax/Leica) had no base. If a camera profile renders wrong in Camera Raw, record in plan.md, do not silently change.
- Output: `...\Settings\Phan Xuan Quang\<Name> (for <Target>).xmp`


## Tests
Shared rules: `CameraProfileRewriterTests` (dictionaries). Format rules: `DcpProfileInstallerTests` / `XmpProfileInstallerTests` (`Rewrite`). DCP: `DcpFileTests.Sample()` builds tiny DCP in code (strings + SRATIONAL matrix + inline SHORT). Fixtures `tests/Unlocker.Tests/Fixtures/`: `sample.xmp` (Fujifilm X-T5 BLEACH BYPASS, stub without embedded table). Assert rewritten fields, other content unchanged, source file hash unchanged, new UUID differs.
