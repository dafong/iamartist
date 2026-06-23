use anyhow::{Context, Result};
use serde::{Deserialize, Serialize};
use std::path::Path;
use std::process::Command;

/// Metadata for a single layer.
#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct LayerInfo {
    pub id: usize,
    pub name: String,
    pub width: u32,
    pub height: u32,
    pub top: i32,
    pub left: i32,
    pub visible: bool,
}

/// Structure expected from Python's `parse` command.
#[derive(Debug, Deserialize)]
struct ParseOutput {
    width: u32,
    height: u32,
    layers: Vec<LayerInfo>,
}

/// Structure expected from Python's `export` command.
#[derive(Debug, Deserialize)]
struct ExportOutput {
    path: String,
}

/// A layer's sprite size and the offset of its rect center from the canvas center.
///
/// `offset_x` is positive to the right of canvas center; `offset_y` is positive
/// above canvas center (the image y axis is flipped to an up-positive axis).
/// Values are in pixels and may be fractional.
#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct LayerTransform {
    pub id: usize,
    pub name: String,
    pub width: u32,
    pub height: u32,
    pub offset_x: f64,
    pub offset_y: f64,
}

/// Structure expected from Python's `transform` command (success or error).
#[derive(Debug, Deserialize)]
#[serde(untagged)]
enum TransformOutput {
    Ok(LayerTransform),
    Err { error: String },
}

/// Locate the Python script.  Looks next to the current executable first,
/// then falls back to the Cargo manifest directory (dev mode).
fn python_script_path() -> Result<std::path::PathBuf> {
    let base = std::env::current_exe()
        .ok()
        .and_then(|p| p.parent().map(|d| d.to_path_buf()))
        .unwrap_or_else(|| Path::new(".").to_path_buf());
    let mut path = base.join("psd_handler.py");
    if !path.exists() {
        path = Path::new(env!("CARGO_MANIFEST_DIR")).join("psd_handler.py");
    }
    Ok(path)
}

fn python_interpreter() -> &'static str {
    if cfg!(target_os = "windows") {
        "python"
    } else {
        "python3"
    }
}

/// Parse a PSD file and return metadata for all layers.
pub fn parse_psd<P: AsRef<Path>>(path: &P) -> Result<(u32, u32, Vec<LayerInfo>)> {
    let script = python_script_path()?;
    let psd_path = path.as_ref().to_string_lossy().to_string();

    let output = Command::new(python_interpreter())
        .arg(&script)
        .arg("parse")
        .arg(&psd_path)
        .output()
        .context("Failed to launch Python process")?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        return Err(anyhow::anyhow!("Python error: {}", stderr));
    }

    let stdout = String::from_utf8_lossy(&output.stdout);
    let parsed: ParseOutput = serde_json::from_str(&stdout).context("Failed to parse Python output")?;

    Ok((parsed.width, parsed.height, parsed.layers))
}

/// Composite only the specified layers into a single PNG/JPEG file.
/// Returns the output file path.
pub fn export_layers<P: AsRef<Path>>(psd_path: &P, layer_ids: &[usize], output_path: &P, format: ExportFormat) -> Result<String> {
    let script = python_script_path()?;

    let mut args: Vec<String> = vec![
        "export".into(),
        psd_path.as_ref().display().to_string(),
        output_path.as_ref().display().to_string(),
        format.extension().to_string(),
    ];
    args.extend(layer_ids.iter().map(|id| id.to_string()));

    let output = Command::new(python_interpreter())
        .arg(&script)
        .args(&args)
        .output()
        .context("Failed to launch Python process")?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        return Err(anyhow::anyhow!("Python error: {}", stderr));
    }

    let stdout = String::from_utf8_lossy(&output.stdout);
    let parsed: ExportOutput = serde_json::from_str(&stdout).context("Failed to parse Python export output")?;

    Ok(parsed.path)
}

/// Return a layer's sprite size and the offset of its rect center from the
/// canvas center (origin = canvas center, in pixels).
pub fn layer_transform<P: AsRef<Path>>(psd_path: &P, layer_id: usize) -> Result<LayerTransform> {
    let script = python_script_path()?;

    let output = Command::new(python_interpreter())
        .arg(&script)
        .arg("transform")
        .arg(psd_path.as_ref().display().to_string())
        .arg(layer_id.to_string())
        .output()
        .context("Failed to launch Python process")?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        return Err(anyhow::anyhow!("Python error: {}", stderr));
    }

    let stdout = String::from_utf8_lossy(&output.stdout);
    let parsed: TransformOutput = serde_json::from_str(&stdout).context("Failed to parse Python transform output")?;

    match parsed {
        TransformOutput::Ok(transform) => Ok(transform),
        TransformOutput::Err { error } => Err(anyhow::anyhow!("Python error: {}", error)),
    }
}

#[derive(Debug, Serialize, Deserialize, Clone, Copy)]
#[serde(rename_all = "lowercase")]
pub enum ExportFormat {
    Png,
    Jpeg,
}

impl ExportFormat {
    pub fn extension(&self) -> &'static str {
        match self {
            ExportFormat::Png => "png",
            ExportFormat::Jpeg => "jpg",
        }
    }
}

#[cfg(test)]
mod tests {
    // Can be re-implemented with a test PSD file when needed.

    use std::{env, fs};

    use tracing::{error, info};

    use crate::psd_handler::ExportFormat;
    use crate::setup_tracing_for_test;

    use super::{export_layers, layer_transform, parse_psd};

    #[test]
    fn test_parse_psd() {
        setup_tracing_for_test();
        let path = env::current_dir().unwrap().join("..").join("psd").join("示例psd.psd");

        let (width, height, layers) = parse_psd(&path).unwrap();
        println!("[psd] dimension is {}x{}", width, height);
        println!(
            "{}",
            layers
                .iter()
                .map(|l| format!("layer [{}] = {}", l.id, &l.name))
                .collect::<Vec<String>>()
                .join("\n")
        );
    }

    #[test]
    fn test_parse_export() {
        setup_tracing_for_test();
        let psd_dir = env::current_dir().unwrap().join("..").join("psd");
        let psd_path = psd_dir.join("示例psd.psd");

        let (width, height, layers) = parse_psd(&psd_path).unwrap();

        let export_layer_names = ["1", "2-1", "2-2", "动作参考", "示意图"];

        export_layer_names.iter().for_each(|name| {
            if let Some(layer) = layers.iter().find(|layer| &layer.name == name) {
                let output_dir = psd_dir.join("export");
                let _ = fs::create_dir_all(&output_dir);
                let output_file = output_dir.join(format!("{}.png", name));
                match export_layers(&psd_path, &[layer.id], &output_file, ExportFormat::Png) {
                    Ok(_) => info!("[导出图层] [{}] 成功", name),
                    Err(e) => error!("[导出图层] [{}] 失败=>{}", name, e),
                }
            } else {
                info!("[导出图层] [{}] 不存在", name)
            }
        });
    }

    #[test]
    fn test_layer_transform() {
        setup_tracing_for_test();
        let psd_dir = env::current_dir().unwrap().join("..").join("psd");
        let psd_path = psd_dir.join("示例psd.psd");

        let (width, height, layers) = parse_psd(&psd_path).unwrap();
        let id = layers.iter().find(|layer| layer.name == "2-2").unwrap().id;

        let transform = layer_transform(&psd_path, id).unwrap();
        let msg = format!(
            r#"
        offset => ({},{})
        size   => ({},{})
        bgsize => ({},{})"#,
            transform.offset_x, transform.offset_y, transform.width, transform.height, width, height
        );
        println!("{}", msg);
    }
}
