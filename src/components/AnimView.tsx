import { useEffect, useRef, useState } from "react";

export interface AnimViewProps {
  keyCount: number;
}

const assetBase = ((import.meta as any).env?.BASE_URL as string) || "/";

const leftImage = {
  normal: `${assetBase}skins/AcornStick/AcornStickLeft.png`,
  punch: `${assetBase}skins/AcornStick/AcornStickLeftPunch.png`,
};

const rightImage = {
  normal: `${assetBase}skins/AcornStick/AcornStickRight.png`,
  punch: `${assetBase}skins/AcornStick/AcornStickRightPunch.png`,
};

const PUNCH_DURATION = 100; // ms

export function AnimView({ keyCount }: AnimViewProps) {
  const [punchSide, setPunchSide] = useState<"left" | "right" | null>(null);
  const prevKeyCount = useRef(keyCount);

  useEffect(() => {
    if (keyCount === 0) return;
    if (keyCount > prevKeyCount.current) {
      const side: "left" | "right" = keyCount % 2 === 1 ? "left" : "right";
      setPunchSide(side);
      const timer = setTimeout(() => setPunchSide(null), PUNCH_DURATION);
      prevKeyCount.current = keyCount;
      return () => clearTimeout(timer);
    }
    prevKeyCount.current = keyCount;
  }, [keyCount]);

  return (
    <>
      <img
        className="anim-layer anim-layer--left"
        src={punchSide === "left" ? leftImage.punch : leftImage.normal}
        alt={punchSide === "left" ? "Left Punch" : "Left"}
        draggable={false}
      />
      <img
        className="anim-layer anim-layer--right"
        src={punchSide === "right" ? rightImage.punch : rightImage.normal}
        alt={punchSide === "right" ? "Right Punch" : "Right"}
        draggable={false}
      />
    </>
  );
}
