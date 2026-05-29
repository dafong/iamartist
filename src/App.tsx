import { useState, useEffect } from "react";
import { getCurrentWindow } from "@tauri-apps/api/window";
import { listen } from "@tauri-apps/api/event";
import { open } from "@tauri-apps/plugin-dialog";
import { parsePsd } from "./api";
// import SpineView from "./components/SpineView";
import type { PsdMeta } from "./types";
import "./App.css";


export default function App() {
  const [psdPath, setPsdPath] = useState<string | null>(null);
  const [psdMeta, setPsdMeta] = useState<PsdMeta | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [keyCount, setKeyCount] = useState(0);

  useEffect(() => {
    const unlisten = listen<number>("key-press", (e) => setKeyCount(e.payload));
    return () => { unlisten.then((f) => f()); };
  }, []);

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
    } catch (e) {
      setError(`解析失败: ${e}`);
    } finally {
      setBusy(false);
    }
  }

  const filename = psdPath ? psdPath.split(/[\\/]/).pop() : null;

  return (
    <div className="root">
      {/* 精灵区域 — 透明背景，不可拖拽 */}
      <div className="sprite-area">
        <img className="sprite" src="/sprite.png" alt="sprite" draggable={false} />
      </div>

      {/* 工具条 — 固定在底部 */}
      <div className="toolbar">
        <div
          className="drag-handle"
          data-tauri-drag-region
          onMouseDown={() => getCurrentWindow().startDragging()}
        >
          <span className="drag-dots" />
        </div>

        <button className="tb-btn tb-btn--primary" onClick={handlePickPsd} disabled={busy} title="选择 PSD 文件">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <rect x="2" y="1" width="9" height="11" rx="1" stroke="currentColor" strokeWidth="1.4"/>
            <path d="M7 1v4h4" stroke="currentColor" strokeWidth="1.4" strokeLinejoin="round"/>
            <path d="M5 14h7" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/>
            <path d="M8.5 11v3" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/>
          </svg>
          <span>打开 PSD</span>
        </button>

        {filename && (
          <div className="tb-info">
            <span className="tb-filename" title={psdPath ?? ""}>{filename}</span>
            {psdMeta && (
              <span className="tb-meta">{psdMeta.width}×{psdMeta.height} · {psdMeta.layers.length}层</span>
            )}
          </div>
        )}

        {error && <span className="tb-error" title={error}>⚠</span>}

        <div className="tb-spacer" />

        <span className="tb-keycount" title="全局按键次数">{keyCount}</span>
      </div>
    </div>
  );
}
