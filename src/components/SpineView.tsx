import { useEffect, useRef } from "react";
import { Application, extensions } from "pixi.js";
import { SpineLoader, SpineLoaderExtension } from "@pixi/spine-pixi";

// 注册 Spine 解析器
extensions.add(SpineLoaderExtension);

interface SpineViewProps {
  /** public/ 下的 .skel 或 .json 路径，例如 "/spine/char.skel" */
  skeletonPath: string;
  /** public/ 下的 .atlas 路径，例如 "/spine/char.atlas" */
  atlasPath: string;
  /** 初始播放的动画名，例如 "idle" */
  animation?: string;
  loop?: boolean;
  width?: number;
  height?: number;
}

export default function SpineView({
  skeletonPath,
  atlasPath,
  animation = "idle",
  loop = true,
  width = 220,
  height = 220,
}: SpineViewProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const appRef = useRef<Application | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    let destroyed = false;
    const app = new Application();
    appRef.current = app;

    (async () => {
      await app.init({
        canvas,
        width,
        height,
        backgroundAlpha: 0,
        antialias: true,
        autoDensity: true,
        resolution: window.devicePixelRatio || 1,
      });

      if (destroyed) return;

      const spineObj = await SpineLoader.load({
        skeleton: skeletonPath,
        atlas: atlasPath,
      });

      if (destroyed) return;

      // 居中放置
      spineObj.x = width / 2;
      spineObj.y = height;

      // 自动缩放到画布高度的 80%
      const bounds = spineObj.getBounds();
      const scale = (height * 0.8) / bounds.height;
      spineObj.scale.set(scale);

      app.stage.addChild(spineObj);
      spineObj.state.setAnimation(0, animation, loop);
    })();

    return () => {
      destroyed = true;
      app.destroy(false, { children: true });
      appRef.current = null;
    };
  }, [skeletonPath, atlasPath, animation, loop, width, height]);

  return (
    <canvas
      ref={canvasRef}
      width={width}
      height={height}
      style={{ display: "block", pointerEvents: "none" }}
    />
  );
}
