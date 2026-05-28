import { useState, useEffect } from "react";
import { getCurrentWindow } from "@tauri-apps/api/window";
import { check } from "@tauri-apps/plugin-updater";
import { ask } from "@tauri-apps/plugin-dialog";
import { open } from "@tauri-apps/plugin-dialog";
import { parsePsd, exportLayers, smbUpload } from "./api";
import LayerList from "./components/LayerList";
import SmbConfigPanel from "./components/SmbConfigPanel";
import type { ExportFormat, ExportedFile, PsdMeta, SmbConfig, UploadResult } from "./types";
import "./App.css";

const DEFAULT_SMB: SmbConfig = {
  host: "",
  share: "",
  username: "",
  password: "",
  remote_dir: "/",
  workgroup: "WORKGROUP",
};

export default function App() {
  const [psdPath, setPsdPath] = useState<string | null>(null);
  const [psdMeta, setPsdMeta] = useState<PsdMeta | null>(null);
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [outputDir, setOutputDir] = useState<string>("");
  const [format, setFormat] = useState<ExportFormat>("png");
  const [smbConfig, setSmbConfig] = useState<SmbConfig>(DEFAULT_SMB);
  const [exportedFiles, setExportedFiles] = useState<ExportedFile[]>([]);
  const [uploadResults, setUploadResults] = useState<UploadResult[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [updateStatus, setUpdateStatus] = useState<"idle" | "checking" | "downloading" | "ready">("idle");

  useEffect(() => {
    checkForUpdate();
  }, []);

  async function checkForUpdate() {
    setUpdateStatus("checking");
    try {
      const update = await check();
      if (!update?.available) { setUpdateStatus("idle"); return; }
      setUpdateStatus("ready");
      const yes = await ask(`发现新版本 ${update.version}，是否立即更新？`, { title: "有新版本", kind: "info" });
      if (!yes) return;
      setUpdateStatus("downloading");
      await update.downloadAndInstall();
    } catch {
      setUpdateStatus("idle");
    }
  }

  async function handlePickPsd() {
    const result = await open({ filters: [{ name: "PSD", extensions: ["psd"] }] });
    if (!result) return;
    const path = typeof result === "string" ? result : (result as { path: string }).path;
    setBusy(true);
    setError(null);
    try {
      const meta = await parsePsd(path);
      setPsdPath(path);
      setPsdMeta(meta);
      setSelected(new Set(meta.layers.filter((l) => l.visible).map((l) => l.id)));
      setExportedFiles([]);
      setUploadResults([]);
    } catch (e) {
      setError(`解析 PSD 失败: ${e}`);
    } finally {
      setBusy(false);
    }
  }

  async function handlePickOutputDir() {
    const result = await open({ directory: true });
    if (result) {
      const dir = typeof result === "string" ? result : (result as { path: string }).path;
      setOutputDir(dir);
    }
  }

  async function handleExport() {
    if (!psdPath || selected.size === 0 || !outputDir) return;
    setBusy(true);
    setError(null);
    try {
      const files = await exportLayers(psdPath, [...selected], outputDir, format);
      setExportedFiles(files);
    } catch (e) {
      setError(`导出失败: ${e}`);
    } finally {
      setBusy(false);
    }
  }

  async function handleUpload() {
    if (exportedFiles.length === 0) return;
    setBusy(true);
    setError(null);
    try {
      const filePaths = exportedFiles.map((f) => f.file_path);
      const results = await smbUpload(smbConfig, filePaths);
      setUploadResults(results);
    } catch (e) {
      setError(`上传失败: ${e}`);
    } finally {
      setBusy(false);
    }
  }

  function toggleLayer(id: number) {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  return (
    <div className="app">
      <header
        data-tauri-drag-region
        onMouseDown={(e) => { if (e.button === 0) getCurrentWindow().startDragging(); }}
      >
        <h1>iamartist</h1>
        <p className="subtitle">PSD 图层导出 · SMB 上传</p>
        {updateStatus === "checking" && <p className="update-tip">检查更新中…</p>}
        {updateStatus === "downloading" && <p className="update-tip">下载更新中…</p>}
        {updateStatus === "ready" && <p className="update-tip update-tip--ready">发现新版本</p>}
      </header>

      {error && <div className="error-banner">{error}</div>}

      {/* Step 1: PSD 选择 */}
      <div className="section">
        <div className="row">
          <button onClick={handlePickPsd} disabled={busy} className="btn-primary">
            选择 PSD 文件
          </button>
          {psdPath && (
            <span className="file-path" title={psdPath}>
              {psdPath.split(/[\\/]/).pop()}
            </span>
          )}
        </div>
        {psdMeta && (
          <p className="meta-info">
            画布 {psdMeta.width}×{psdMeta.height}px · {psdMeta.layers.length} 个图层
          </p>
        )}
        {psdMeta && (
          <LayerList
            layers={psdMeta.layers}
            selected={selected}
            onToggle={toggleLayer}
            onSelectAll={() => setSelected(new Set(psdMeta.layers.map((l) => l.id)))}
            onClear={() => setSelected(new Set())}
          />
        )}
      </div>

      {/* Step 2: 导出设置 */}
      {psdMeta && (
        <div className="section">
          <h2>导出设置</h2>
          <div className="row">
            <button onClick={handlePickOutputDir} disabled={busy}>
              选择输出目录
            </button>
            {outputDir && <span className="file-path">{outputDir}</span>}
          </div>
          <div className="row">
            <label>格式</label>
            <select value={format} onChange={(e) => setFormat(e.target.value as ExportFormat)}>
              <option value="png">PNG</option>
              <option value="jpeg">JPEG</option>
            </select>
          </div>
          <button
            className="btn-primary"
            onClick={handleExport}
            disabled={busy || selected.size === 0 || !outputDir}
          >
            {busy ? "导出中…" : `导出 ${selected.size} 个图层`}
          </button>
        </div>
      )}

      {/* Step 3: 已导出文件 */}
      {exportedFiles.length > 0 && (
        <div className="section">
          <h2>已导出文件 ({exportedFiles.length})</h2>
          <ul className="file-list">
            {exportedFiles.map((f) => (
              <li key={f.file_path}>
                <span className="layer-name">{f.layer_name}</span>
                <span className="file-path">{f.file_path.split(/[\\/]/).pop()}</span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Step 4: SMB 上传 */}
      {exportedFiles.length > 0 && (
        <div className="section">
          <SmbConfigPanel config={smbConfig} onChange={setSmbConfig} />
          <button
            className="btn-primary"
            onClick={handleUpload}
            disabled={busy || !smbConfig.host}
          >
            {busy ? "上传中…" : "上传到 SMB 网盘"}
          </button>
        </div>
      )}

      {/* 上传结果 */}
      {uploadResults.length > 0 && (
        <div className="section success-section">
          <h2>上传完成 ✓</h2>
          <ul className="file-list">
            {uploadResults.map((r) => (
              <li key={r.remote_path}>
                <span className="layer-name">{r.local_path.split(/[\\/]/).pop()}</span>
                <span className="file-path">→ {r.remote_path}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
