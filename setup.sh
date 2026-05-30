#!/bin/bash

# Tauri + React + TypeScript Project Setup Script
# This script installs all necessary dependencies for the iamartist project

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

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_command() {
    if command -v $1 &> /dev/null; then
        log_success "$1 is installed: $($1 --version 2>&1 | head -n 1)"
        return 0
    else
        log_error "$1 is not installed"
        return 1
    fi
}

echo "========================================="
echo "  iamartist Project Setup"
echo "========================================="
echo ""

# Check macOS
if [[ "$(uname)" != "Darwin" ]]; then
    log_error "This script is designed for macOS. For other platforms, please install dependencies manually."
    exit 1
fi

# 1. Install Xcode Command Line Tools (required for Rust and native dependencies)
log_info "Checking Xcode Command Line Tools..."
if xcode-select -p &> /dev/null; then
    log_success "Xcode Command Line Tools is installed"
else
    log_info "Installing Xcode Command Line Tools..."
    xcode-select --install
    log_success "Xcode Command Line Tools installed"
fi

# 2. Install Homebrew if not present
log_info "Checking Homebrew..."
if command -v brew &> /dev/null; then
    log_success "Homebrew is installed: $(brew --version | head -n 1)"
else
    log_info "Installing Homebrew..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
    log_success "Homebrew installed"
fi

# 3. Install Rust (required for Tauri)
log_info "Checking Rust..."
if command -v rustc &> /dev/null; then
    log_success "Rust is installed: $(rustc --version)"
else
    log_info "Installing Rust..."
    curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
    source "$HOME/.cargo/env"
    log_success "Rust installed: $(rustc --version)"
fi

# 4. Install Node.js (required for React/Vite)
log_info "Checking Node.js..."
if command -v node &> /dev/null; then
    log_success "Node.js is installed: $(node --version)"
else
    log_info "Installing Node.js via Homebrew..."
    brew install node
    log_success "Node.js installed: $(node --version)"
fi

# 5. Install pnpm (recommended package manager)
log_info "Checking pnpm..."
if command -v pnpm &> /dev/null; then
    log_success "pnpm is installed: $(pnpm --version)"
else
    log_info "Installing pnpm..."
    npm install -g pnpm
    log_success "pnpm installed: $(pnpm --version)"
fi

# 6. Install Python (required for some node-gyp dependencies)
log_info "Checking Python..."
if command -v python3 &> /dev/null; then
    log_success "Python is installed: $(python3 --version)"
else
    log_info "Installing Python via Homebrew..."
    brew install python
    log_success "Python installed: $(python3 --version)"
fi

# 7. Install Tauri system dependencies (Linux only, skip on macOS)
# On macOS, most dependencies are provided by Xcode Command Line Tools

# 8. Configure pnpm to allow build scripts (required for esbuild and other native dependencies)
log_info "Configuring pnpm to allow build scripts..."
cat > pnpm-workspace.yaml << EOF
allowBuilds:
  esbuild: true
EOF
log_success "pnpm configuration updated"

# 9. Install project dependencies
log_info "Installing project dependencies..."
pnpm install
log_success "Project dependencies installed"

# 10. Verify Rust target for cross-compilation (if needed)
log_info "Checking Rust targets..."
rustup target list --installed
log_success "Rust targets verified"

echo ""
echo "========================================="
log_success "Setup complete!"
echo "========================================="
echo ""
echo "You can now run:"
echo "  pnpm dev        - Start development server"
echo "  pnpm tauri dev  - Run Tauri app in development mode"
echo "  pnpm build      - Build the web app"
echo "  pnpm tauri build - Build the production Tauri app"
echo ""
