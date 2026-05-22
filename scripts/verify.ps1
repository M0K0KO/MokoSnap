$ErrorActionPreference = "Stop"

Write-Host "== MokoSnap verify =="

if (!(Test-Path ".\MokoSnap.sln")) {
    Write-Host "No MokoSnap.sln found yet. Skipping dotnet verification."
    Write-Host "VERIFY PASSED: repository bootstrap only"
    exit 0
}

Write-Host "`n[1/3] dotnet restore"
dotnet restore .\MokoSnap.sln

Write-Host "`n[2/3] dotnet build"
dotnet build .\MokoSnap.sln --configuration Debug --no-restore

Write-Host "`n[3/3] dotnet test"
dotnet test .\MokoSnap.sln --configuration Debug --no-build

Write-Host "`nVERIFY PASSED"