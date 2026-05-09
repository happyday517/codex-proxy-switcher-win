@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "SCRIPT_PATH=%SCRIPT_DIR%Start-Codex.ps1"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_PATH%" -Mode Native

if errorlevel 1 (
    echo.
    echo Failed to start Codex without proxy. See the error above.
    pause
)
