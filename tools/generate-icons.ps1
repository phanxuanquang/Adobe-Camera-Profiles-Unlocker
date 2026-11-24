<#
.SYNOPSIS
Regenerates the app and site icons from the SVG logo in tools/icon.

.DESCRIPTION
Renders each image straight from the SVG at its exact pixel size with headless Microsoft Edge
(transparent background) and packs the ICO files from PNG entries. Writes only the files the repo
already references (Unlocker.WinUI.csproj, Package.appxmanifest, site/*.html), under the same names.
The 32 px and smaller marks use the simplified drawing.

.EXAMPLE
./tools/generate-icons.ps1
./tools/generate-icons.ps1 -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$Svg = (Join-Path $PSScriptRoot 'icon\app-icon.svg'),
    [string]$SmallSvg = (Join-Path $PSScriptRoot 'icon\app-icon-small.svg')
)
$ErrorActionPreference = 'Stop'

$edge = @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") |
    Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $edge) { throw 'Microsoft Edge not found.' }

$root = Split-Path $PSScriptRoot
$assets = Join-Path $root 'src\Unlocker.WinUI\Assets'
$work = Join-Path ([IO.Path]::GetTempPath()) "generate-icons-$PID"
$uri = @{
    big   = 'data:image/svg+xml;base64,' + [Convert]::ToBase64String([IO.File]::ReadAllBytes((Resolve-Path $Svg)))
    small = 'data:image/svg+xml;base64,' + [Convert]::ToBase64String([IO.File]::ReadAllBytes((Resolve-Path $SmallSvg)))
}

function Get-PngSize([byte[]]$b) { "$($b[18] * 256 + $b[19])x$($b[22] * 256 + $b[23])" }  # IHDR, big-endian

# Renders the mark (Mark x Mark px) centred on a transparent Width x Height canvas.
function New-IconPng([string]$Path, [int]$Width, [int]$Height, [int]$Mark) {
    $html = Join-Path $work 'page.html'
    $src = $uri[$(if ($Mark -le 32) { 'small' } else { 'big' })]
    Set-Content $html "<!doctype html><body style=`"margin:0;width:${Width}px;height:${Height}px;display:grid;place-items:center;background:transparent`"><img src=`"$src`" width=`"$Mark`" height=`"$Mark`"></body>"
    if (Test-Path $Path) { Remove-Item $Path }
    # Piping to Out-Null makes PowerShell wait for msedge.exe to exit.
    & $edge --headless --disable-gpu --hide-scrollbars --force-device-scale-factor=1 --default-background-color=00000000 `
        "--user-data-dir=$work\profile" "--window-size=$Width,$Height" "--screenshot=$Path" ([Uri]$html).AbsoluteUri 2>&1 | Out-Null
    if (-not (Test-Path $Path) -or (Get-PngSize ([IO.File]::ReadAllBytes($Path))) -ne "${Width}x$Height") { throw "Render failed: $Path" }
}

# ICO with PNG entries (Windows Vista and later).
function New-Ico([string]$Path, [int[]]$Sizes) {
    $pngs = foreach ($s in $Sizes) { $p = Join-Path $work "ico-$s.png"; New-IconPng $p $s $s $s; , [IO.File]::ReadAllBytes($p) }
    $ms = [IO.MemoryStream]::new(); $w = [IO.BinaryWriter]::new($ms)
    $w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$Sizes.Count)
    $offset = 6 + 16 * $Sizes.Count
    for ($i = 0; $i -lt $Sizes.Count; $i++) {
        $dim = [byte]($Sizes[$i] % 256)  # 0 means 256
        $w.Write($dim); $w.Write($dim); $w.Write([byte]0); $w.Write([byte]0)
        $w.Write([uint16]1); $w.Write([uint16]32); $w.Write([uint32]$pngs[$i].Length); $w.Write([uint32]$offset)
        $offset += $pngs[$i].Length
    }
    foreach ($p in $pngs) { $w.Write($p) }
    $w.Flush(); [IO.File]::WriteAllBytes($Path, $ms.ToArray())
}

# Path, width, height, mark size. Tiles keep the WinUI template proportions.
$pngs = @(
    @("$assets\Square44x44Logo.scale-200.png", 88, 88, 88),
    @("$assets\Square44x44Logo.targetsize-24_altform-unplated.png", 24, 24, 24),
    @("$assets\Square44x44Logo.targetsize-48_altform-lightunplated.png", 48, 48, 48),
    @("$assets\Square150x150Logo.scale-200.png", 300, 300, 300),
    @("$assets\Wide310x150Logo.scale-200.png", 620, 300, 200),
    @("$assets\SplashScreen.scale-200.png", 1240, 600, 300),
    @("$assets\LockScreenLogo.scale-200.png", 48, 48, 48),
    @("$assets\StoreLogo.png", 50, 50, 50)
)
$icos = @(
    @("$assets\AppIcon.ico", @(16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256)),
    @("$root\site\assets\img\favicon.ico", @(16, 32, 48))
)

New-Item -ItemType Directory -Force $work, $assets -WhatIf:$false | Out-Null
try {
    foreach ($j in $pngs) { if ($PSCmdlet.ShouldProcess($j[0], "PNG $($j[1])x$($j[2])")) { New-IconPng @j; Write-Host "  $($j[0])  $($j[1])x$($j[2])" } }
    $logo = "$root\site\assets\img\logo.svg"  # site header and SVG favicon; Pages only ships site/
    if ($PSCmdlet.ShouldProcess($logo, 'Copy SVG')) { Copy-Item $Svg $logo; Write-Host "  $logo  SVG" }
    foreach ($j in $icos) { if ($PSCmdlet.ShouldProcess($j[0], "ICO $($j[1] -join ',')")) { New-Ico $j[0] $j[1]; Write-Host "  $($j[0])  $($j[1] -join ',')" } }
}
finally { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
