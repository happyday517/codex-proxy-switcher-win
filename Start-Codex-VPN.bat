@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "SCRIPT_PATH=%SCRIPT_DIR%Start-Codex.ps1"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_PATH%" -Mode Vpn

if errorlevel 1 (
    echo.
    echo Failed to start Codex with proxy. See the error above.
    pause
)
