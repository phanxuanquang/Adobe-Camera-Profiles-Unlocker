# Profile formats + rewrite rules

Rewrite = pure static functions in `Unlocker.Core/Rules/CameraProfileRewriter.cs`. `XDocument` only. Never line-based string replace (old bug). Source never modified; output always new file.

Author credit const: `© 2024 Phan Xuan Quang / Github: @phanxuanquang`. Owner decision: keep legacy branding (Copyright + signature overwrite).

## DCP (binary)
Per camera profile:
1. `dcpTool.exe -d "<src.dcp>" "<tmp.xml>"` (decompile)
2. Rewrite XML:
   - `<ProfileName>`: `Camera` -> `<SourceModel>:` (e.g. `Fujifilm X-T5: CLASSIC CHROME`), unique in Profile Browser
   - `<Copyright>` = author credit
   - `<ProfileCalibrationSignature>`: `com.adobe` -> `Phan Xuan Quang`
   - `<UniqueCameraModelRestriction>` = target model
3. `dcpTool.exe -c "<tmp.xml>" "<out.dcp>"` (compile). Out name: `<Name> (for <Target>).dcp`
4. Delete tmp (finally block).

dcpTool:
- Binaries: `dcpTool.exe`, `iconv.dll`, `libxml2.dll`, `zlib1.dll` in Infrastructure `Assets/` (taken from old app), `CopyToOutputDirectory=PreserveNewest`.
- Path from `AppContext.BaseDirectory`. Never relative.
- Exit code verified: bad input -> exit 1, no output.
- `CreateNoWindow`, redirect stderr, `await WaitForExitAsync(ct)`, kill process tree on cancel, non-zero exit -> throw `InvalidOperationException` with stderr.
- Install camera profiles sequentially (dcpTool + disk bound; parallel buys little, complicates cancel).

## XMP (text)
`crs` ns = `http://ns.adobe.com/camera-raw-settings/1.0/`. Real sample fields (attributes on `rdf:Description`):
`PresetType="Look"`, `UUID`, `CameraModelRestriction`, `Copyright`, `CameraProfile` (base DCP, e.g. `Camera PROVIA/Standard`), `RGBTable` (hash -> embedded table), `SupportsMonochrome`, `ConvertToGrayscale`.
Child elements: `crs:Name` / `crs:ShortName` / `crs:Group` (each `rdf:Alt/rdf:li`).

Rewrite:
- `crs:CameraModelRestriction` = target model
- `crs:Copyright` = author credit
- `crs:UUID` = new `Guid.NewGuid().ToString("N").ToUpperInvariant()`. Same UUID as original = collision.
- `crs:Name` li -> `<SourceModel>: <Name>`
- `crs:CameraProfile` = `Camera Standard` (legacy behavior: target lacks source base DCP). If a camera profile renders wrong in Camera Raw, record in plan.md, do not silently change.
- Output: `...\Settings\Unlocker\<Name> (for <Target>).xmp`


## Tests
Fixtures `tests/Unlocker.Tests/Fixtures/`: `sample.dcp.xml` (`dcpTool -d` of Canon EOS R5 Portrait, LookTable/ToneCurve bulk trimmed to keep repo small) + `sample.xmp` (Fujifilm X-T5 BLEACH BYPASS, stub without embedded table). Assert rewritten fields, other content unchanged, source file hash unchanged, new UUID differs.
