import { invoke } from "@tauri-apps/api/core";
import type {
  ExportedFile,
  ExportFormat,
  PsdMeta,
  SmbConfig,
  UploadResult,
} from "./types";

export const parsePsd = (path: string): Promise<PsdMeta> =>
  invoke("parse_psd", { path });

export const exportLayers = (
  psdPath: string,
  layerIds: number[],
  outputDir: string,
  format: ExportFormat
): Promise<ExportedFile[]> =>
  invoke("export_layers", {
    req: { psd_path: psdPath, layer_ids: layerIds, output_dir: outputDir, format },
  });

export const smbUpload = (
  config: SmbConfig,
  files: string[]
): Promise<UploadResult[]> =>
  invoke("smb_upload", { req: { config, files } });

export const smbTest = (config: SmbConfig): Promise<string[]> =>
  invoke("smb_test", { config });
