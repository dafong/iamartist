@echo off
REM Build the `psd_handler` PyInstaller sidecar for Windows and place it in
REM src-tauri\binaries\ with the Rust target-triple suffix Tauri expects.
REM PyInstaller cannot cross-compile: run this on Windows for the Windows target.
setlocal enabledelayedexpansion

set REPO_ROOT=%~dp0..
set PYTHON_DIR=%REPO_ROOT%\src-python
set SCRIPT=%REPO_ROOT%\src-tauri\psd_handler.py
set OUT_DIR=%REPO_ROOT%\src-tauri\binaries

for /f "tokens=2" %%i in ('rustc -vV ^| findstr /b "host:"') do set TARGET=%%i
echo Building psd_handler sidecar for target: %TARGET%

cd /d "%PYTHON_DIR%"

if not exist ".venv" (
    python -m venv .venv
)
call .venv\Scripts\activate.bat

pip install -q -r requirements.txt

pyinstaller --onefile --noconfirm --name psd_handler "%SCRIPT%"

if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"
copy /y dist\psd_handler.exe "%OUT_DIR%\psd_handler-%TARGET%.exe"

echo Done: %OUT_DIR%\psd_handler-%TARGET%.exe
