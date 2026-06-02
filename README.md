# iamartist

TODO: 项目的简单描述

## 技术栈

- **桌面框架**: [Tauri 2](https://tauri.app/) (Rust 后端 + Web 前端)
- **前端**: React 19 + TypeScript + [PixiJS 8](https://pixijs.com/)（Spine 动画渲染）
- **PSD 处理**: Python 3 + [psd-tools](https://github.com/psd-tools/psd-tools) + Pillow（通过子进程调用）
- **SMB 上传**: Rust 原生 `mount_smbfs`（macOS）/ `net use`（Windows）
- **包管理**: pnpm workspace

## 项目结构

```
├── src/                    # React 前端
│   ├── api.ts              # Tauri invoke 封装
│   ├── types.ts            # TypeScript 类型定义
│   └── components/         # UI 组件（LayerList, SmbConfigPanel, SpineView）
├── src-tauri/              # Rust 后端
│   ├── src/
│   │   ├── lib.rs          # Tauri 命令注册
│   │   ├── psd_handler.rs  # PSD 解析 & 图层合成（→ Python 子进程）
│   │   └── smb_handler.rs  # SMB 挂载 & 文件上传
│   ├── psd_handler.py      # Python PSD 处理脚本
│   └── Cargo.toml
├── src-python/             # Python 虚拟环境
├── psd/                    # 测试用 PSD 文件
├── test_psd_handler.sh     # Python 脚本测试
└── setup.sh                # 项目初始化脚本
```

## 环境要求

| 依赖 | 说明 |
|------|------|
| [Rust](https://rustup.rs/) | Tauri 编译 |
| [Node.js](https://nodejs.org/) ≥ 18 | 前端构建 |
| [pnpm](https://pnpm.io/) | 包管理 |
| [Python 3](https://www.python.org/) | PSD 处理 |
| `psd-tools` + `Pillow` | Python 依赖（见下方安装） |

### Python 环境

```bash
python3 -m venv src-python/.venv
source src-python/.venv/bin/activate
pip install psd-tools Pillow
```

## 开发

```bash
# 安装前端依赖
pnpm install

# 启动开发服务器（热重载）
pnpm tauri dev

# 仅编译检查 Rust 端
cd src-tauri && cargo check
```

## 测试

### Python PSD 处理

```bash
./test_psd_handler.sh
```

脚本会：
1. 解析 `psd/童年稻草堆.psd` 并输出所有图层元数据
2. 合成指定图层为一张 PNG

### Rust 测试

```bash
cd src-tauri

# PSD 解析 + 合成
cargo test test_parse_psd -- --nocapture
cargo test test_parse_export -- --nocapture

# SMB 连接测试
cargo test test_smb_conn -- --nocapture
```

## Tauri 命令

| 命令 | 说明 |
|------|------|
| `parse_psd` | 解析 PSD 文件，返回画布尺寸和图层列表 |
| `export_layers` | 合成指定图层为单张 PNG/JPEG |
| `smb_upload` | 上传文件到 SMB 共享网盘 |
| `smb_test` | 测试 SMB 连接并列出远程目录 |

详细的类型定义见 `src/types.ts`。

