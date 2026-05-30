#!/bin/bash

# Build psd_composite Python binary for Tauri sidecar
# This script creates a Python virtual environment, installs dependencies,
# and builds a standalone binary using PyInstaller

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Get repository root and paths
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PYTHON_DIR="$REPO_ROOT/src-python"
OUT_DIR="$REPO_ROOT/src-tauri/binaries"

# Detect current platform target
TARGET=$(rustc -vV 2>/dev/null | grep '^host:' | awk '{print $2}')
if [ -z "$TARGET" ]; then
    # Fallback if rustc is not available
    case "$(uname -m)" in
        x86_64)   TARGET="x86_64-apple-darwin" ;;
        arm64)    TARGET="aarch64-apple-darwin" ;;
        *)        log_error "Unsupported architecture: $(uname -m)"; exit 1 ;;
    esac
fi

echo "========================================="
echo "  Build psd_composite Sidecar"
echo "========================================="
echo ""
log_info "Target: $TARGET"
log_info "Python dir: $PYTHON_DIR"
log_info "Output dir: $OUT_DIR"
echo ""

# Check Python
if ! command -v python3 &> /dev/null; then
    log_error "Python 3 is not installed. Please install Python 3.8 or later."
    exit 1
fi
log_success "Python: $(python3 --version)"

# Create virtual environment if not exists
cd "$PYTHON_DIR"
if [ ! -d ".venv" ]; then
    log_info "Creating Python virtual environment..."
    python3 -m venv .venv
    log_success "Virtual environment created"
fi

# Activate virtual environment
source .venv/bin/activate

# Install dependencies
log_info "Installing Python dependencies..."
pip install -q -r requirements.txt
log_success "Python dependencies installed"

# Build with PyInstaller
log_info "Building psd_composite with PyInstaller..."
pyinstaller --onefile --noconfirm --name psd_composite psd_composite.py

# Copy binary to output directory
mkdir -p "$OUT_DIR"
cp dist/psd_composite "$OUT_DIR/psd_composite-${TARGET}"

echo ""
log_success "Build complete!"
log_info "Binary: $OUT_DIR/psd_composite-${TARGET}"
echo ""
log_info "You can now run: pnpm tauri dev"
echo ""
