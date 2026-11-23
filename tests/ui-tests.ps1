# UI smoke tests for Unlocker.WinUI (winui-ui-testing skill template).
# Target: local by default. Windows Sandbox has no Adobe Camera Raw, so only the "Camera Raw missing" screen is testable there.
# Preconditions (local): Camera Raw installed, "Your camera" already set, no profiles installed by this app.
# Side effects: installs 1 Fujifilm camera profile into C:\ProgramData\Adobe\CameraRaw\Settings\Unlocker, then removes it.
# Usage: .\tests\ui-tests.ps1 -Target local (any working directory)
param(
    [ValidateSet('sandbox', 'local')][string]$Target = 'local',
    [int]$AppPid = 0,
    [ValidateSet('sandbox', 'local')][string]$AppScope,
    [ValidateNotNullOrEmpty()][string]$WorkflowId = [guid]::NewGuid().ToString('N'),
    [string]$ArtifactDirectory = (Join-Path $PSScriptRoot "ui-test-artifacts\$([guid]::NewGuid().ToString('N'))")
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$ScopeArgs = @(if ($Target -eq 'sandbox') { '--on'; 'sandbox' })
$previousWorkflowId = $env:WINAPP_UI_WORKFLOW_ID
$env:WINAPP_UI_WORKFLOW_ID = $WorkflowId
$results = [Collections.Generic.List[object]]::new()
$screenshots = [Collections.Generic.List[string]]::new()
$uiStarted = $false
$XmpOut = 'C:\ProgramData\Adobe\CameraRaw\Settings\Unlocker'

function Invoke-WinAppChecked {
    param([string[]]$Arguments)
    $global:LASTEXITCODE = $null
    $output = @(& winapp @Arguments)
    $code = $global:LASTEXITCODE
    if ($null -eq $code -or $code -ne 0) {
        throw "winapp $($Arguments -join ' ') failed (exit $code). $($output -join "`n")"
    }
    $output
}

function Invoke-Ui {
    param([string[]]$Arguments, [string]$Window)
    $selector = if ($Window) { @('-w', $Window) } else { @('-a', "$AppPid") }
    Invoke-WinAppChecked (@('ui') + $Arguments + $ScopeArgs + $selector)
}

function Test-UI {
    param([string]$Name, [scriptblock]$Script)
    try {
        & $Script | Out-Null
        $results.Add(@{ name = $Name; status = 'PASS' })
    } catch {
        $results.Add(@{ name = $Name; status = 'FAIL'; detail = "$_" })
    }
}

function Save-Screenshot {
    param([string]$Name, [switch]$Popups)
    $path = Join-Path $ArtifactDirectory $Name
    if (Test-Path -LiteralPath $path) { throw "Choose a new evidence path: $path" }
    $cmd = @('screenshot', '--output', $path) + @(if ($Popups) { '--capture-screen' })
    Invoke-Ui $cmd -Window $hwnd | Out-Null
    if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or (Get-Item -LiteralPath $path).Length -eq 0) {
        throw "Screenshot was not delivered to the host: $path"
    }
    $screenshots.Add($path)
}

function Get-InstalledXmp { @(Get-ChildItem -LiteralPath $XmpOut -Filter '*.xmp' -ErrorAction SilentlyContinue) }

try {
    $ArtifactDirectory = [IO.Path]::GetFullPath($ArtifactDirectory)
    New-Item -ItemType Directory -Force -Path $ArtifactDirectory | Out-Null
    if ($AppPid -lt 0) { throw 'AppPid must be positive, or zero to launch.' }
    if ($AppPid -gt 0) {
        if ($AppScope -ne $Target) {
            throw 'Reusing a PID requires matching -AppScope and -Target from the same live target.'
        }
    } else {
        $launch = Invoke-WinAppChecked (@('run', (Join-Path $PSScriptRoot '..\src\Unlocker.WinUI')) + $ScopeArgs + @('--detach', '--json')) |
            Where-Object { $_ -notmatch '^\s*(Restored|Determining|Unlocker\.|Build succeeded|\d+ (Warning|Error)|Time Elapsed|dotnet build)' } |
            Out-String | ConvertFrom-Json
        if ($launch.Error -or -not $launch.ProcessId -or [int]$launch.ProcessId -le 0) {
            throw "No app ProcessId. $($launch.Error)"
        }
        $AppPid = [int]$launch.ProcessId
        if ($Target -eq 'local' -and $launch.ProcessScope -and $launch.ProcessScope -ne 'local') {
            throw 'Launch response belongs to a different target.'
        }
    }

    $uiStarted = $true
    $hwnd = $null
    for ($i = 0; $i -lt 20 -and -not $hwnd; $i++) {
        $windows = @(Invoke-Ui @('list-windows', '--json') | ConvertFrom-Json)
        $main = @($windows.windows + $windows) | Where-Object { $_.hwnd -and $_.hwnd -ne '0' -and $_.title -eq 'Adobe Camera Profiles Unlocker' } |
            Select-Object -First 1
        if ($main) { $hwnd = [string]$main.hwnd } else { Start-Sleep -Milliseconds 500 }
    }
    if (-not $hwnd) { throw 'No app window found in the selected target.' }

    Test-UI 'Precondition: no profiles installed by app' {
        if ((Get-InstalledXmp).Count) { throw "Clean $XmpOut first (remove via app)." }
    }
    Test-UI 'Startup reaches Unlock page' {
        Invoke-Ui @('wait-for', 'ProfileSearch', '-t', '20000')
        Invoke-Ui @('wait-for', 'BrandList', '-t', '3000')
    }
    Test-UI 'Initial screenshot delivered' { Save-Screenshot '01-unlock.png' }

    Test-UI 'Pick Fujifilm and select one camera profile' {
        Invoke-Ui @('invoke', 'Fujifilm, Film Simulations')
        Invoke-Ui @('wait-for', 'Fujifilm Film Simulations', '-t', '3000')
        Invoke-Ui @('invoke', 'BLEACH BYPASS', '--action', 'toggle-on')
        Invoke-Ui @('wait-for', 'InstallButton', '-p', 'Name', '--value', 'Install 1 camera profile', '-t', '3000')
        Invoke-Ui @('wait-for', 'Remove BLEACH BYPASS from selection', '-t', '3000')
    }
    Test-UI 'Selection screenshot' { Save-Screenshot '02-selected.png' }

    Test-UI 'Install writes one XMP and reports success' {
        Invoke-Ui @('invoke', 'InstallButton')
        Invoke-Ui @('wait-for', 'Installed 1 camera profile for', '--contains', '-t', '15000')
        Invoke-Ui @('wait-for', 'NavInstalled', '-t', '2000')
        $files = Get-InstalledXmp
        if ($files.Count -ne 1 -or $files[0].Name -notlike 'BLEACH BYPASS (for *).xmp') { throw "Expected 1 XMP, got: $($files.Name -join ', ')" }
    }
    Test-UI 'Install success screenshot' { Save-Screenshot '03-installed.png' }

    Test-UI 'View installed lists the profile' {
        Invoke-Ui @('invoke', 'View installed')
        Invoke-Ui @('wait-for', 'RemoveAllButton', '-t', '3000')
        Invoke-Ui @('wait-for', 'Remove BLEACH BYPASS for', '--contains', '-t', '3000')
    }

    Test-UI 'Single remove is instant, Undo restores file' {
        Invoke-Ui @('invoke', 'RemoveProfileButton')
        Invoke-Ui @('wait-for', 'Removed BLEACH BYPASS for', '--contains', '-t', '3000')
        if ((Get-InstalledXmp).Count) { throw 'File still present after remove.' }
        Invoke-Ui @('invoke', 'Undo')
        Invoke-Ui @('wait-for', 'RemoveProfileButton', '-t', '3000')
        if ((Get-InstalledXmp).Count -ne 1) { throw 'Undo did not restore the file.' }
    }

    Test-UI 'Bulk remove asks first, then empties' {
        Invoke-Ui @('invoke', 'RemoveGroupButton')
        Invoke-Ui @('wait-for', 'Remove 1', '-t', '3000')
        Save-Screenshot '04-confirm-dialog.png'
        Invoke-Ui @('invoke', 'Remove 1')
        Invoke-Ui @('wait-for', 'EmptyUnlockButton', '-t', '3000')
        if ((Get-InstalledXmp).Count) { throw 'File still present after bulk remove.' }
    }
    Test-UI 'Empty state screenshot' { Save-Screenshot '05-empty.png' }

    Test-UI 'Search with no match shows empty state and clears' {
        Invoke-Ui @('invoke', 'NavUnlock')
        Invoke-Ui @('wait-for', 'ProfileSearch', '-t', '3000')
        Invoke-Ui @('set-value', 'TextBox', 'zzzz')
        Invoke-Ui @('wait-for', 'No camera profiles match "zzzz"', '-t', '3000')
        Save-Screenshot '06-search-empty.png'
        Invoke-Ui @('invoke', 'ClearSearchButton')
        Invoke-Ui @('wait-for', 'ClearSearchButton', '--gone', '-t', '3000')
    }

    Test-UI 'Search hides brands without matches' {
        Invoke-Ui @('set-value', 'TextBox', 'bleach')
        Invoke-Ui @('wait-for', 'Fujifilm, Film Simulations', '-t', '3000')
        Invoke-Ui @('wait-for', 'Canon, Picture Styles', '--gone', '-t', '3000')
        Save-Screenshot '07-search-brands.png'
        Invoke-Ui @('set-value', 'TextBox', '')
        Invoke-Ui @('wait-for', 'Canon, Picture Styles', '-t', '3000')
    }

    Test-UI 'Your camera opens a right pane with brand filter' {
        Invoke-Ui @('invoke', 'TargetButton')
        Invoke-Ui @('wait-for', 'CameraBrandFilter', '-t', '3000')
        Invoke-Ui @('wait-for', 'CameraModelList', '-t', '3000')
        Save-Screenshot '08-camera-pane.png'
        Invoke-Ui @('invoke', 'CloseCameraPaneButton')
        Invoke-Ui @('wait-for', 'CameraBrandFilter', '--gone', '-t', '3000')
    }

    Test-UI 'Settings shows update and folders' {
        Invoke-Ui @('invoke', 'Settings')
        Invoke-Ui @('wait-for', 'CheckUpdatesButton', '-t', '3000')
        Invoke-Ui @('wait-for', 'DcpCard', '-t', '3000')
        Invoke-Ui @('wait-for', 'XmpCard', '-t', '3000')
        Save-Screenshot '09-settings.png'
    }

    Test-UI 'App controls have AutomationIds' {
        $inspection = Invoke-Ui @('inspect', '--interactive', '--json') -Window $hwnd | ConvertFrom-Json
        function Get-Elements($nodes) {
            foreach ($node in $nodes) {
                if ($null -ne $node) { $node; Get-Elements $node.children }
            }
        }
        $allElements = @(Get-Elements @($inspection.windows | ForEach-Object { $_.elements }))
        if (-not $allElements.Count) { throw 'Inspection returned no elements; not an accessibility PASS.' }
        $appElements = @($allElements | Where-Object {
            $_.type -match 'Button|TextBox|ComboBox|CheckBox|ToggleSwitch|TabItem|Edit' -and
            $_.name -notmatch 'Minimize|Maximize|Close|System|Toggle Navigation' -and
            $_.className -notmatch 'PickerHost|#32770|CabinetWClass'
        })
        if (-not $appElements.Count) { throw 'No app controls were audited.' }
        $missingId = @($appElements | Where-Object { -not $_.automationId })
        if ($missingId.Count) {
            throw "Missing AutomationIds: $(($missingId | ForEach-Object { "$($_.type) '$($_.name)'" }) -join ', ')"
        }
    }
} catch {
    $results.Add(@{ name = 'Target setup / test execution'; status = 'FAIL'; detail = "$_" })
} finally {
    try {
        if ($uiStarted) { Invoke-WinAppChecked (@('ui', 'yield') + $ScopeArgs) | Out-Null }
    } catch {
        $results.Add(@{ name = 'Release UI workflow'; status = 'FAIL'; detail = "$_" })
    } finally {
        $env:WINAPP_UI_WORKFLOW_ID = $previousWorkflowId
    }
}

$failed = @($results | Where-Object { $_.status -eq 'FAIL' }).Count
$report = [ordered]@{
    target = $Target; appPid = $AppPid; workflowId = $WorkflowId
    passed = $results.Count - $failed; failed = $failed
    results = @($results.ToArray()); screenshots = @($screenshots.ToArray())
}
$resultsPath = Join-Path $PSScriptRoot 'test-results.json'
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultsPath -Encoding utf8
Write-Host "Passed: $($report.passed) | Failed: $failed | Results: $resultsPath"
if ($failed) { exit 1 }
exit 0
