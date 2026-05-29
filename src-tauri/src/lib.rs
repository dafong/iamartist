mod key_listener;
mod psd_handler;
mod smb_handler;

use psd_handler::{ExportFormat, LayerInfo};
use serde::{Deserialize, Serialize};
use smb_handler::SmbConfig;

// ─── PSD commands ────────────────────────────────────────────────────────────

#[derive(Serialize)]
pub struct PsdMeta {
    pub width: u32,
    pub height: u32,
    pub layers: Vec<LayerInfo>,
}

#[tauri::command]
fn parse_psd(path: String) -> Result<PsdMeta, String> {
    let (width, height, layers) = psd_handler::parse_psd(&path).map_err(|e| e.to_string())?;
    Ok(PsdMeta {
        width,
        height,
        layers,
    })
}

#[derive(Deserialize)]
pub struct ExportRequest {
    pub psd_path: String,
    pub layer_ids: Vec<usize>,
    pub output_dir: String,
    pub format: ExportFormat,
}

#[derive(Serialize)]
pub struct ExportedFile {
    pub layer_name: String,
    pub file_path: String,
}

#[tauri::command]
fn export_layers(req: ExportRequest) -> Result<Vec<ExportedFile>, String> {
    let results =
        psd_handler::export_layers(&req.psd_path, &req.layer_ids, &req.output_dir, req.format)
            .map_err(|e| e.to_string())?;

    Ok(results
        .into_iter()
        .map(|(name, path)| ExportedFile {
            layer_name: name,
            file_path: path,
        })
        .collect())
}

// ─── Composite export command ────────────────────────────────────────────────

#[derive(Deserialize)]
pub struct CompositeRequest {
    pub psd_path: String,
    pub output_path: String,
    pub visible_layer_names: Vec<String>,
}

#[tauri::command]
async fn export_composite(app: tauri::AppHandle, req: CompositeRequest) -> Result<String, String> {
    use tauri_plugin_shell::ShellExt;
    let names_json = serde_json::to_string(&req.visible_layer_names).map_err(|e| e.to_string())?;
    let output = app
        .shell()
        .sidecar("psd_composite")
        .map_err(|e| e.to_string())?
        .args([&req.psd_path, &req.output_path, &names_json])
        .output()
        .await
        .map_err(|e| e.to_string())?;
    if output.status.success() {
        Ok(req.output_path)
    } else {
        Err(String::from_utf8_lossy(&output.stderr).to_string())
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

// ─── Key count command ────────────────────────────────────────────────────────

#[tauri::command]
fn get_key_count() -> u64 {
    key_listener::total()
}

// ─── App entry point ──────────────────────────────────────────────────────────

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_updater::Builder::new().build())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_shell::init())
        .invoke_handler(tauri::generate_handler![
            parse_psd,
            export_layers,
            export_composite,
            smb_upload,
            smb_test,
            get_key_count,
        ])
        .setup(|app| {
            key_listener::start(app.handle().clone());
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
