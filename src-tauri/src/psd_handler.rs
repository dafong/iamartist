use anyhow::{Context, Result};
use image::{ImageBuffer, Rgba};
use psd::Psd;
use serde::{Deserialize, Serialize};
use std::path::Path;

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

/// Parse a PSD file and return metadata for all layers.
pub fn parse_psd(path: &str) -> Result<(u32, u32, Vec<LayerInfo>)> {
    let bytes = std::fs::read(path).with_context(|| format!("reading {path}"))?;
    let psd = Psd::from_bytes(&bytes).map_err(|e| anyhow::anyhow!("{e:?}"))?;

    let width = psd.width();
    let height = psd.height();

    let layers: Vec<LayerInfo> = psd
        .layers()
        .iter()
        .enumerate()
        .map(|(id, layer)| LayerInfo {
            id,
            name: layer.name().to_string(),
            width: layer.width() as u32,
            height: layer.height() as u32,
            top: layer.layer_top(),
            left: layer.layer_left(),
            visible: true,
        })
        .collect();

    Ok((width, height, layers))
}

/// Export selected layers to PNG files in output_dir.
/// Returns list of (layer_name, output_file_path).
pub fn export_layers(
    psd_path: &str,
    layer_ids: &[usize],
    output_dir: &str,
    format: ExportFormat,
) -> Result<Vec<(String, String)>> {
    let bytes = std::fs::read(psd_path).with_context(|| format!("reading {psd_path}"))?;
    let psd = Psd::from_bytes(&bytes).map_err(|e| anyhow::anyhow!("{e:?}"))?;
    let layers = psd.layers();

    std::fs::create_dir_all(output_dir)?;

    let mut results = Vec::new();

    for &id in layer_ids {
        let layer = layers
            .get(id)
            .with_context(|| format!("layer index {id} out of range"))?;

        let w = layer.width() as u32;
        let h = layer.height() as u32;

        if w == 0 || h == 0 {
            continue;
        }

        let rgba: Vec<u8> = layer.rgba();

        let img: ImageBuffer<Rgba<u8>, Vec<u8>> =
            ImageBuffer::from_raw(w, h, rgba).with_context(|| "ImageBuffer size mismatch")?;

        let safe_name = sanitize_filename(layer.name());
        let ext = format.extension();
        let out_path = Path::new(output_dir)
            .join(format!("{safe_name}.{ext}"))
            .to_string_lossy()
            .to_string();

        match format {
            ExportFormat::Png => img.save(&out_path)?,
            ExportFormat::Jpeg => {
                let rgb = image::DynamicImage::ImageRgba8(img).to_rgb8();
                rgb.save(&out_path)?;
            }
        }

        results.push((layer.name().to_string(), out_path));
    }

    Ok(results)
}

#[derive(Debug, Serialize, Deserialize, Clone, Copy)]
#[serde(rename_all = "lowercase")]
pub enum ExportFormat {
    Png,
    Jpeg,
}

impl ExportFormat {
    fn extension(&self) -> &'static str {
        match self {
            ExportFormat::Png => "png",
            ExportFormat::Jpeg => "jpg",
        }
    }
}

fn sanitize_filename(name: &str) -> String {
    name.chars()
        .map(|c| if c.is_alphanumeric() || c == '-' || c == '_' { c } else { '_' })
        .collect()
}
