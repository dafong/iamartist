export interface LayerInfo {
  id: number;
  name: string;
  width: number;
  height: number;
  top: number;
  left: number;
  visible: boolean;
}

export interface PsdMeta {
  width: number;
  height: number;
  layers: LayerInfo[];
}

export interface SmbConfig {
  host: string;
  share: string;
  username: string;
  password: string;
  remote_dir: string;
  workgroup?: string;
}

export interface UploadResult {
  local_path: string;
  remote_path: string;
}

export type ExportFormat = "png" | "jpeg";
