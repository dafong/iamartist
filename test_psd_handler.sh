#!/usr/bin/env bash
# test_psd_handler.sh — 测试 Python PSD 处理脚本
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
VENV_DIR="$ROOT_DIR/src-python/.venv"
PY_SCRIPT="$ROOT_DIR/src-tauri/psd_handler.py"
PSD_FILE="$ROOT_DIR/psd/童年稻草堆.psd"
TMP_DIR="$(mktemp -d)"

cleanup() { rm -rf "$TMP_DIR"; }
trap cleanup EXIT

echo "=== 激活 Python 虚拟环境 ==="
source "$VENV_DIR/bin/activate"

echo ""
echo "=== 1. 测试 parse 命令 ==="
python "$PY_SCRIPT" parse "$PSD_FILE" | python -m json.tool

echo ""
echo "=== 2. 测试 export 命令（合成图层 0,3 为一张 PNG）==="
python "$PY_SCRIPT" export "$PSD_FILE" "$TMP_DIR/composite.png" png 0 3 | python -m json.tool
ls -lh "$TMP_DIR"

echo ""
echo "=== 全部测试通过 ✅ ==="
