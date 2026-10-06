param(
    [string]$Version = "v1.0.1"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " [RESCENE Pomodoro Pet] Dual Release Packager" -ForegroundColor Cyan
Write-Host " Version: $Version" -ForegroundColor Cyan
Write-Host " Editions: Standalone (~100MB) & Lightweight (~15MB)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$distFolder = "dist"
if (Test-Path $distFolder) {
    Remove-Item $distFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $distFolder -Force | Out-Null

Write-Host "0. Stopping existing instance if running..." -ForegroundColor Yellow
Stop-Process -Name RescenePomodoro -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

# -------------------------------------------------------------
# 1. Standalone Edition (Self-Contained, ~100MB, Zero dependencies)
# -------------------------------------------------------------
Write-Host "1. Building Standalone Edition (.NET runtime embedded)..." -ForegroundColor Yellow
$outStandalone = "bin\Release\net8.0-windows\win-x64\standalone"
dotnet publish radiant-noether.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o $outStandalone

$pkgStandalone = Join-Path $distFolder "RescenePomodoro-$Version-Standalone-win-x64"
New-Item -ItemType Directory -Path $pkgStandalone -Force | Out-Null
Copy-Item (Join-Path $outStandalone "RescenePomodoro.exe") -Destination (Join-Path $pkgStandalone "RescenePomodoro.exe")
Copy-Item (Join-Path $outStandalone "RescenePomodoro.exe") -Destination (Join-Path $distFolder "RescenePomodoro-Standalone.exe")
Copy-Item "./Images" -Destination $pkgStandalone -Recurse
Copy-Item "./Sounds" -Destination $pkgStandalone -Recurse
Copy-Item "./README.md" -Destination $pkgStandalone
if (Test-Path "./실행하기.bat") { Copy-Item "./실행하기.bat" -Destination $pkgStandalone }

$zipStandalone = Join-Path $distFolder "RescenePomodoro-$Version-Standalone-win-x64.zip"
Write-Host "   Compressing Standalone ZIP ($zipStandalone)..." -ForegroundColor Yellow
Compress-Archive -Path "$pkgStandalone/*" -DestinationPath $zipStandalone -Force
Remove-Item $pkgStandalone -Recurse -Force

if (Test-Path "./publish") {
    Copy-Item (Join-Path $outStandalone "RescenePomodoro.exe") -Destination "./publish/RescenePomodoro.exe" -Force -ErrorAction SilentlyContinue
}

# -------------------------------------------------------------
# 2. Lightweight Edition (Framework-Dependent, ~15MB, Ultra compact)
# -------------------------------------------------------------
Write-Host "2. Building Lightweight Edition (requires .NET 8 Desktop Runtime)..." -ForegroundColor Yellow
$outLightweight = "bin\Release\net8.0-windows\win-x64\lightweight"
dotnet publish radiant-noether.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o $outLightweight

$pkgLightweight = Join-Path $distFolder "RescenePomodoro-$Version-Lightweight-win-x64"
New-Item -ItemType Directory -Path $pkgLightweight -Force | Out-Null
Copy-Item (Join-Path $outLightweight "RescenePomodoro.exe") -Destination (Join-Path $pkgLightweight "RescenePomodoro.exe")
Copy-Item (Join-Path $outLightweight "RescenePomodoro.exe") -Destination (Join-Path $distFolder "RescenePomodoro-Lightweight.exe")
Copy-Item "./Images" -Destination $pkgLightweight -Recurse
Copy-Item "./Sounds" -Destination $pkgLightweight -Recurse
Copy-Item "./README.md" -Destination $pkgLightweight
if (Test-Path "./실행하기.bat") { Copy-Item "./실행하기.bat" -Destination $pkgLightweight }

$zipLightweight = Join-Path $distFolder "RescenePomodoro-$Version-Lightweight-win-x64.zip"
Write-Host "   Compressing Lightweight ZIP ($zipLightweight)..." -ForegroundColor Yellow
Compress-Archive -Path "$pkgLightweight/*" -DestinationPath $zipLightweight -Force
Remove-Item $pkgLightweight -Recurse -Force

Write-Host ""
Write-Host "Both Release Editions packaged successfully!" -ForegroundColor Green
Write-Host "Artifacts in '$distFolder':" -ForegroundColor Green
Get-ChildItem -Path $distFolder | Select-Object Name, @{Name="Size_MB"; Expression={[math]::round($_.Length / 1MB, 2)}} | Format-Table -AutoSize
