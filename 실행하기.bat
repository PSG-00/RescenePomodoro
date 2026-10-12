@echo off
cd /d "%~dp0"
if exist "RescenePomodoro.exe" (
    start "" "RescenePomodoro.exe"
    exit /b
)
if exist "bin\Release\net8.0-windows\win-x64\standalone\RescenePomodoro.exe" (
    start "" "bin\Release\net8.0-windows\win-x64\standalone\RescenePomodoro.exe"
    exit /b
)
if exist "bin\Debug\net8.0-windows\RescenePomodoro.exe" (
    start "" "bin\Debug\net8.0-windows\RescenePomodoro.exe"
    exit /b
)
