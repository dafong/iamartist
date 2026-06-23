#!/usr/bin/env bash
# Build the `psd_handler` PyInstaller sidecar for the current platform and place
# it in src-tauri/binaries/ with the Rust target-triple suffix Tauri expects.
#
# PyInstaller cannot cross-compile: run this on macOS/Linux for those targets,
# and run build_sidecar.bat on Windows for the Windows target.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PYTHON_DIR="$REPO_ROOT/src-python"
SCRIPT="$REPO_ROOT/src-tauri/psd_handler.py"
OUT_DIR="$REPO_ROOT/src-tauri/binaries"

TARGET=$(rustc -vV | grep '^host:' | awk '{print $2}')
echo "Building psd_handler sidecar for target: $TARGET"

cd "$PYTHON_DIR"

if [ ! -d ".venv" ]; then
    python3 -m venv .venv
fi
source .venv/bin/activate

pip install -q -r requirements.txt

pyinstaller --onefile --noconfirm --name psd_handler "$SCRIPT"

mkdir -p "$OUT_DIR"
cp dist/psd_handler "$OUT_DIR/psd_handler-${TARGET}"

echo "Done: $OUT_DIR/psd_handler-${TARGET}"
