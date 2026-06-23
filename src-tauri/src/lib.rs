mod psd_handler;
mod smb_handler;

use std::{env, fs};

use psd_handler::{ExportFormat, LayerInfo};
use serde::{Deserialize, Serialize};
use smb_handler::SmbConfig;
use tracing::{error, info};

use self::psd_handler::{export_layers, LayerTransform};

// ─── PSD commands ────────────────────────────────────────────────────────────

#[derive(Serialize)]
pub struct PsdMeta {
    pub width: u32,
    pub height: u32,
    pub layers: Vec<LayerInfo>,
    pub exports: Option<ExportLayer>,
    pub mark_trans: Option<LayerTransform>,
}

#[derive(Serialize)]
pub struct ExportLayer {
    pub name: String,
    pub output: Option<String>,
}

#[tauri::command]
fn export_psd(path: String) -> Result<PsdMeta, String> {
    let (width, height, layers) = psd_handler::parse_psd(&path).map_err(|e| e.to_string())?;

    let meta = PsdMeta {
        width,
        height,
        layers,
        exports: None,
        mark_trans: None,
    };

    let export_layer_names = ["1", "2-1", "2-2", "动作参考", "示意图"];

    let _ = &export_layer_names.iter().for_each(|name| {
        if let Some(layer) = &meta.layers.iter().find(|layer| &layer.name == name) {
            let output_name = format!("{}.png", name);
            match export_layers(&path, &[layer.id], &output_name, ExportFormat::Png) {
                Ok(output) => info!("[导出图层] [{}] 成功 => {}", name, output),
                Err(e) => error!("[导出图层] [{}] 失败=>{}", name, e),
            }
        } else {
            info!("[导出图层] [{}] 不存在", name)
        }
    });

    Ok(meta)
}

#[derive(Deserialize)]
pub struct ExportRequest {
    pub psd_path: String,
    pub layer_ids: Vec<usize>,
    pub output_path: String,
    pub format: ExportFormat,
}

// ─── Composite export command ────────────────────────────────────────────────

#[derive(Deserialize)]
pub struct CompositeRequest {
    pub psd_path: String,
    pub output_path: String,
    pub visible_layer_names: Vec<String>,
}

#[tauri::command]
fn read_file(path: String) -> Result<String, String> {
    match fs::read_to_string(&path) {
        Ok(content) => Ok(content),
        Err(e) => Err(format!("read file[{}] failed => {}", path, e)),
    }
}

#[tauri::command]
fn write_file(path: String, contents: String) -> Result<(), String> {
    match fs::write(&path, contents) {
        Ok(_) => Ok(()),
        Err(e) => Err(format!("write file[{}] failed =>{}", path, e)),
    }
}

// ─── SMB commands ─────────────────────────────────────────────────────────────

#[derive(Deserialize)]
pub struct UploadRequest {
    pub config: SmbConfig,
    pub files: Vec<String>,
}

#[derive(Serialize)]
pub struct UploadResult {
    pub local_path: String,
    pub remote_path: String,
}

#[tauri::command]
fn smb_upload(req: UploadRequest) -> Result<Vec<UploadResult>, String> {
    let results = smb_handler::upload_files(&req.config, &req.files).map_err(|e| e.to_string())?;
    Ok(results
        .into_iter()
        .map(|(local, remote)| UploadResult {
            local_path: local,
            remote_path: remote,
        })
        .collect())
}

#[tauri::command]
fn smb_test(config: SmbConfig) -> Result<Vec<String>, String> {
    smb_handler::test_connection(&config).map_err(|e| e.to_string())
}

// ─── App entry point ──────────────────────────────────────────────────────────

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_updater::Builder::new().build())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_shell::init())
        .invoke_handler(tauri::generate_handler![export_psd, smb_upload, smb_test, read_file, write_file])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

#[cfg(test)]
pub fn setup_tracing_for_test() {
    let _ = tracing_subscriber::fmt()
        .without_time()
        .with_target(false)
        .with_file(true)
        .with_line_number(true)
        .with_test_writer()
        .try_init();
}
