@echo off
setlocal enabledelayedexpansion

set REPO_ROOT=%~dp0..
set PYTHON_DIR=%REPO_ROOT%\src-python
set OUT_DIR=%REPO_ROOT%\src-tauri\binaries

for /f "tokens=2" %%i in ('rustc -vV ^| findstr /b "host:"') do set TARGET=%%i
echo Building psd_composite sidecar for target: %TARGET%

cd /d "%PYTHON_DIR%"

if not exist ".venv" (
    python -m venv .venv
)
call .venv\Scripts\activate.bat

pip install -q -r requirements.txt

pyinstaller --onefile --noconfirm --name psd_composite psd_composite.py

if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"
copy /y dist\psd_composite.exe "%OUT_DIR%\psd_composite-%TARGET%.exe"

echo Done: %OUT_DIR%\psd_composite-%TARGET%.exe
