# Builds draft release notes from conventional commit subjects since the previous v* tag.
# Usage: ./release-notes.ps1 -Tag v3.1.0 [-Repo owner/name] [-Previous v3.0.0]
param(
    [Parameter(Mandatory)][string]$Tag,
    [string]$Repo = 'phanxuanquang/Adobe-Camera-Profiles-Unlocker',
    [string]$Previous = (git describe --tags --abbrev=0 --match 'v*' --exclude $Tag $Tag 2>$null)
)

$range = if ($Previous) { "$Previous..$Tag" } else { $Tag }
$groups = [ordered]@{ Features = @(); Fixes = @(); Maintenance = @() }
foreach ($subject in git log --no-merges --format=%s $range) {
    if ($subject -match '^(\w+)(\([^)]*\))?!?:\s*(.+)$') {
        $type = $Matches[1]
        $text = $Matches[3].Substring(0, 1).ToUpper() + $Matches[3].Substring(1)
    } else {
        $type = 'other'
        $text = $subject
    }
    $group = switch ($type) { 'feat' { 'Features' } 'fix' { 'Fixes' } default { 'Maintenance' } }
    $groups[$group] += "- $text"
}

$sections = @(foreach ($name in 'Features', 'Fixes') {
    if ($groups[$name]) { "### $name`n" + ($groups[$name] -join "`n") }
})
if ($groups.Maintenance) {
    $sections += "<details><summary>Maintenance ($($groups.Maintenance.Count))</summary>`n`n" + ($groups.Maintenance -join "`n") + "`n</details>"
}
$changes = if ($sections) { $sections -join "`n`n" } else { '- No user-facing changes.' }

$notes = (Get-Content (Join-Path $PSScriptRoot 'release-notes-template.md') -Raw).Replace('{{CHANGES}}', $changes)
if ($Previous) { $notes += "`n**Full changelog**: https://github.com/$Repo/compare/$Previous...$Tag`n" }
$notes
