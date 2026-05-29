#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PYTHON_DIR="$REPO_ROOT/src-python"
OUT_DIR="$REPO_ROOT/src-tauri/binaries"

TARGET=$(rustc -vV | grep '^host:' | awk '{print $2}')
echo "Building psd_composite sidecar for target: $TARGET"

cd "$PYTHON_DIR"

if [ ! -d ".venv" ]; then
    python3 -m venv .venv
fi
source .venv/bin/activate

pip install -q -r requirements.txt

pyinstaller --onefile --noconfirm --name psd_composite psd_composite.py

mkdir -p "$OUT_DIR"
cp dist/psd_composite "$OUT_DIR/psd_composite-${TARGET}"

echo "Done: $OUT_DIR/psd_composite-${TARGET}"
