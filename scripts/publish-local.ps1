$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$publishRoot = Join-Path $repoRoot "artifacts\publish\MokoSnap"
$appOutput = Join-Path $publishRoot "MokoSnap.App"
$nativeHostOutput = Join-Path $publishRoot "MokoSnap.NativeHost"
$appProject = Join-Path $repoRoot "src\MokoSnap.App\MokoSnap.App.csproj"
$nativeHostProject = Join-Path $repoRoot "src\MokoSnap.NativeHost\MokoSnap.NativeHost.csproj"

Write-Host "== MokoSnap local publish =="

if (Test-Path $publishRoot) {
    Remove-Item -Recurse -Force -LiteralPath $publishRoot
}

New-Item -ItemType Directory -Force -Path $appOutput | Out-Null
New-Item -ItemType Directory -Force -Path $nativeHostOutput | Out-Null

Write-Host "`n[1/2] Publish MokoSnap.App"
dotnet publish $appProject --configuration Release --output $appOutput --framework net8.0-windows --self-contained false

Write-Host "`n[2/2] Publish MokoSnap.NativeHost"
dotnet publish $nativeHostProject --configuration Release --output $nativeHostOutput --framework net8.0 --self-contained false

$appExe = Join-Path $appOutput "MokoSnap.App.exe"
$nativeHostExe = Join-Path $nativeHostOutput "MokoSnap.NativeHost.exe"

if (!(Test-Path $appExe)) {
    throw "Published app exe was not found: $appExe"
}

if (!(Test-Path $nativeHostExe)) {
    throw "Published native host exe was not found: $nativeHostExe"
}

Write-Host "`nPublish directory: $publishRoot"
Write-Host "MokoSnap app exe: $appExe"
Write-Host "MokoSnap native host exe: $nativeHostExe"
Write-Host "`nPUBLISH PASSED"
