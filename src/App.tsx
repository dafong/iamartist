import { useCallback, useEffect, useState } from "react";
import { getCurrentWindow } from "@tauri-apps/api/window";
import { LogicalSize, PhysicalPosition } from "@tauri-apps/api/dpi";
import { AnimView } from "./components/AnimView";
import { useIncrementKeyCount, useKeyCount } from "./store";
import "./App.css";

const BASE_HEIGHT = 280;
const PANEL_HEIGHT = 215;

export default function App() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [activeTab, setActiveTab] = useState(0);
  const keyCount = useKeyCount();
  const incrementKeyCount = useIncrementKeyCount();

  const toggleMenu = useCallback(() => {
    setMenuOpen((v) => !v);
  }, []);

  const closeMenu = useCallback(() => {
    setMenuOpen(false);
  }, []);

  // 菜单展开/收起时只改变窗口高度，保持窗口左上角(top)不变，向下扩展/收起。
  // AnimView 始终位于 sprite-area 底部，而 sprite-area 高度恒为 BASE_HEIGHT
  // （总高度 = BASE_HEIGHT + 面板高度），因此 AnimView 在屏幕上的位置保持不变。
  useEffect(() => {
    const resizeWindow = async () => {
      const win = getCurrentWindow();
      const targetHeight = menuOpen ? BASE_HEIGHT + PANEL_HEIGHT : BASE_HEIGHT;
      const inner = await win.innerSize();
      const sf = await win.scaleFactor();

      // 调整前记录窗口左上角（物理像素）。macOS 下窗口原点在左下角，
      // setSize 会保持底边不动而把顶边上移，导致 AnimView 跟着上移；
      // 因此 resize 后把 top 重新钉回原处，让窗口只向下增长。
      const pos = await win.outerPosition();

      // 用逻辑像素设置尺寸（逻辑像素 = CSS 像素，无需手动乘缩放因子）。
      // 宽度保持当前值（innerSize 是物理像素，转成逻辑像素再传入）。
      const logicalWidth = inner.width / sf;
      await win.setSize(new LogicalSize(logicalWidth, targetHeight));
      await win.setPosition(new PhysicalPosition(pos.x, pos.y));
    };
    resizeWindow();
  }, [menuOpen]);

  useEffect(() => {
    const handleKeyDown = () => {
      incrementKeyCount();
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [incrementKeyCount]);

  const tabs = ["图层", "导出", "设置"];

  return (
    <div className="root">
      <div className="sprite-area">
        <div className="anim-container">
          {/* AnimView + 工具条共享容器，可拖拽 */}
          <div
            className="anim-view"
            data-tauri-drag-region
            onMouseDown={() => getCurrentWindow().startDragging()}
          >
            <AnimView keyCount={keyCount} />

            <div className="anim-toolbar">
              <span className="toolbar-keycount">{keyCount}</span>
              <button className="toolbar-menu-btn" onClick={toggleMenu}>
                <svg width="18" height="14" viewBox="0 0 18 14" fill="none">
                  <rect x="3" y="1" width="12" height="2" rx="1" fill="currentColor" />
                  <rect x="3" y="6" width="12" height="2" rx="1" fill="currentColor" />
                  <rect x="3" y="11" width="12" height="2" rx="1" fill="currentColor" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* 菜单面板 — 在 AnimView 下方展开 */}
      {menuOpen && (
        <div className="menu-panel">
          <div className="menu-header">
            <div className="menu-tabs">
              {tabs.map((label, i) => (
                <button
                  key={i}
                  className={`tab${activeTab === i ? " tab--active" : ""}`}
                  onClick={() => setActiveTab(i)}
                >
                  {label}
                </button>
              ))}
            </div>
            <button className="close-btn" onClick={closeMenu}>
              <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                <path d="M2 2l10 10M12 2L2 12" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
              </svg>
            </button>
          </div>
          <div className="menu-content">
            {/* 后续添加各面板内容 */}
          </div>
        </div>
      )}
    </div>
  );
}
