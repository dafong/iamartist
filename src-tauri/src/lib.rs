mod psd_handler;
mod smb_handler;
use self::psd_handler::{export_layers, layer_transform, LayerTransform};
use self::smb_handler::upload_files;
use anyhow::Result;
use psd_handler::{ExportFormat, LayerInfo};
use serde::Serialize;
use smb_handler::SmbConfig;
use std::{env, fs};
use tempfile::tempdir;
use tracing::{error, info};

// ─── PSD commands ────────────────────────────────────────────────────────────

#[derive(Serialize)]
pub struct PsdMeta {
    pub width: u32,
    pub height: u32,
    pub layers: Vec<LayerInfo>,
    pub exports: Vec<Option<String>>,
    pub mark_trans: Option<LayerTransform>,
    pub trans_txt: Option<String>,
}

impl PsdMeta {
    fn dump_txt(&self) -> String {
        match &self.mark_trans {
            Some(trans) => format!(
                "{},{},{},{},{},{}",
                trans.offset_x, trans.offset_y, trans.width, trans.height, self.width, self.height
            ),
            None => String::new(),
        }
    }
}

#[tauri::command]
fn export_psd_and_upload(path: String) -> Result<()> {
    let (width, height, layers) = psd_handler::parse_psd(&path)?;

    let mut meta = PsdMeta {
        width,
        height,
        layers,
        exports: vec![],
        mark_trans: None,
        trans_txt: None,
    };

    let export_layer_names = ["1", "2-1", "2-2", "动作参考", "示意图"];
    let temp_dir = tempdir()?;

    let export_results = export_layer_names
        .iter()
        .map(|name| {
            if let Some(layer) = &meta.layers.iter().find(|layer| &layer.name == name) {
                let output_name = format!("{}.png", name);
                let output = export_layers(&path, &[layer.id], &temp_dir, &output_name, ExportFormat::Png)?;
                info!("[导出图层] [{}] 成功 => {}", name, output.display().to_string());
                Ok(Some(output.display().to_string()))
            } else {
                info!("[导出图层] [{}] 不存在", name);
                Ok(None)
            }
        })
        .collect::<Result<Vec<Option<String>>>>()?;

    meta.exports = export_results;

    //如果存在2-2 就给他弄一个2-2.txt
    if let Some(idx) = meta.layers.iter().position(|layer| &layer.name == "2-2") {
        if meta.exports[idx].is_some() {
            match layer_transform(&path, meta.layers[idx].id) {
                Ok(transform) => {
                    info!("[导出坐标] 图层[2-2] {}", meta.dump_txt());
                    meta.mark_trans = Some(transform)
                }
                Err(e) => error!("[导出坐标出错] => {}", e),
            }
        }
    } else {
        info!("[不存在坐标图层]");
    }
    let txt_path = temp_dir.path().join("2-2.txt");

    fs::write(&txt_path, meta.dump_txt())?;

    meta.trans_txt = Some(txt_path.display().to_string());

    //upload

    let cfg = SmbConfig {
        host: "disk.happyelements.net".to_string(),
        share: "g_pop".to_string(),
        username: "xinlei.fan".to_string(),
        password: "Oi9klk97&".to_string(),
        remote_dir: "unity资源".to_string(),
        workgroup: None,
    };

    let mut paths = meta.exports.iter().flatten().map(String::as_str).collect::<Vec<&str>>();
    if let Some(txt) = &meta.trans_txt {
        paths.push(txt);
    }

    upload_files(&cfg, paths.as_slice())?;
    Ok(())
}

// ─── Composite export command ────────────────────────────────────────────────

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
        .invoke_handler(tauri::generate_handler![smb_test, read_file, write_file])
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

#[cfg(test)]
mod tests {
    use tracing::info;

    use crate::psd_handler::layer_transform;
    use crate::{psd_handler, setup_tracing_for_test, PsdMeta};
    use std::env;
    #[test]
    fn test_export_txt() {
        setup_tracing_for_test();

        let psd_dir = env::current_dir().unwrap().join("..").join("psd");
        let psd_path = psd_dir.join("示例psd.psd");

        let (width, height, layers) = psd_handler::parse_psd(&psd_path).unwrap();

        let mut meta = PsdMeta {
            width,
            height,
            layers,
            exports: vec![],
            mark_trans: None,
            trans_txt: None,
        };
        let id = meta.layers.iter().find(|layer| &layer.name == "2-2").unwrap().id;
        let trans = layer_transform(&psd_path, id).unwrap();
        meta.mark_trans = Some(trans);
        info!("[trans] {}", meta.dump_txt());
    }
}
