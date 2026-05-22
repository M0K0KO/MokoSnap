$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$publishScript = Join-Path $repoRoot "scripts\publish-local.ps1"
$issPath = Join-Path $repoRoot "installer\MokoSnap.iss"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$installerOutput = Join-Path $repoRoot "artifacts\installer\MokoSnapSetup.exe"

function Find-InnoSetupCompiler {
    $command = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = @()
    if (${env:ProgramFiles(x86)}) {
        $candidates += Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"
    }

    if (${env:ProgramFiles}) {
        $candidates += Join-Path ${env:ProgramFiles} "Inno Setup 6\ISCC.exe"
    }

    if (${env:LOCALAPPDATA}) {
        $candidates += Join-Path ${env:LOCALAPPDATA} "Programs\Inno Setup 6\ISCC.exe"
    }

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    return $null
}

Write-Host "== MokoSnap installer build =="

Write-Host "`n[1/3] Publish local release output"
& $publishScript

Write-Host "`n[2/3] Locate Inno Setup compiler"
$iscc = Find-InnoSetupCompiler
if (!$iscc) {
    throw "Inno Setup compiler ISCC.exe was not found. Install Inno Setup 6 from https://jrsoftware.org/isinfo.php, then rerun ./scripts/build-installer.ps1. No installer was built."
}

Write-Host "ISCC.exe: $iscc"

Write-Host "`n[3/3] Compile installer"
New-Item -ItemType Directory -Force -Path $installerDir | Out-Null
& $iscc "/O$installerDir" "/FMokoSnapSetup" $issPath

if (!(Test-Path $installerOutput)) {
    throw "Installer build completed but output was not found: $installerOutput"
}

Write-Host "`nInstaller path: $installerOutput"
Write-Host "`nINSTALLER BUILD PASSED"
