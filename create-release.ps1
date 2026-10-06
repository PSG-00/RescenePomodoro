param(
    [string]$Version = "v1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " [RESCENE Pomodoro Pet] Release Packager" -ForegroundColor Cyan
Write-Host " Version: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$distFolder = "dist"
$packageFolder = Join-Path $distFolder "RescenePomodoro-$Version-win-x64"
$zipPath = Join-Path $distFolder "RescenePomodoro-$Version-win-x64.zip"

if (Test-Path $distFolder) {
    Remove-Item $distFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $packageFolder -Force | Out-Null

Write-Host "0. Stopping existing instance if running..." -ForegroundColor Yellow
Stop-Process -Name RescenePomodoro -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

Write-Host "1. Building standalone Release executable..." -ForegroundColor Yellow
$publishOut = "bin\Release\net8.0-windows\win-x64\publish"
dotnet publish radiant-noether.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

Write-Host "2. Copying release files..." -ForegroundColor Yellow
$builtExe = Join-Path $publishOut "RescenePomodoro.exe"
Copy-Item $builtExe -Destination $packageFolder
Copy-Item $builtExe -Destination $distFolder
if (Test-Path "./publish") {
    Copy-Item $builtExe -Destination "./publish/RescenePomodoro.exe" -Force -ErrorAction SilentlyContinue
}
Copy-Item "./Images" -Destination $packageFolder -Recurse
Copy-Item "./Sounds" -Destination $packageFolder -Recurse
Copy-Item "./README.md" -Destination $packageFolder
if (Test-Path "./실행하기.bat") {
    Copy-Item "./실행하기.bat" -Destination $packageFolder
}

Write-Host "3. Compressing into ZIP ($zipPath)..." -ForegroundColor Yellow
Compress-Archive -Path "$packageFolder/*" -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Release packaging completed successfully!" -ForegroundColor Green
Write-Host "Artifacts created in '$distFolder':" -ForegroundColor Green
Get-ChildItem -Path $distFolder | Select-Object Name, @{Name="Size_MB"; Expression={[math]::round($_.Length / 1MB, 2)}} | Format-Table -AutoSize
