import { invoke } from "@tauri-apps/api/core";
import type {
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
  outputPath: string,
  format: ExportFormat
): Promise<string> =>
  invoke("export_layers", {
    req: { psd_path: psdPath, layer_ids: layerIds, output_path: outputPath, format },
  });

export const smbUpload = (
  config: SmbConfig,
  files: string[]
): Promise<UploadResult[]> =>
  invoke("smb_upload", { req: { config, files } });

export const smbTest = (config: SmbConfig): Promise<string[]> =>
  invoke("smb_test", { config });

export const read_file = (path:string):Promise<string>=> invoke("read_file",{path})

export const write_file = (path:string,contents:string):Promise<string>=> invoke("write_file",{path,contents})
