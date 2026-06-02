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

    use std::env;

    use crate::psd_handler::ExportFormat;

    use super::{export_layers, parse_psd};

    #[test]
    fn test_parse_psd() {
        let path = env::current_dir().unwrap().join("..").join("psd").join("童年稻草堆.psd");

        let (width, height, _) = parse_psd(&path).unwrap();
        println!("psd size is {}x{}", width, height);
    }
    #[test]
    fn test_parse_export() {
        let path = env::current_dir().unwrap().join("..").join("psd").join("童年稻草堆.psd");
        let (_, _, layers) = parse_psd(&path).unwrap();
        println!(
            "{}",
            layers
                .iter()
                .map(|l| format!("[{}]={}", &l.name, l.id))
                .collect::<Vec<String>>()
                .join("\n")
        );
        let output = env::current_dir().unwrap().join("..").join("export.png");
        let ids = layers.iter().map(|l| l.id).collect::<Vec<usize>>();
        let _ = export_layers(&path, &ids, &output, ExportFormat::Png).expect("export layers error");
    }
}
