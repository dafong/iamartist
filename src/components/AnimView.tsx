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

export function AnimView({ keyCount }: AnimViewProps) {
  const leftPunch = keyCount % 4 === 1;
  const rightPunch = keyCount % 4 === 3;

  return (
    <div className="anim-view">
      <img
        className="anim-layer anim-layer--left"
        src={leftPunch ? leftImage.punch : leftImage.normal}
        alt={leftPunch ? "Left Punch" : "Left"}
        draggable={false}
      />
      <img
        className="anim-layer anim-layer--right"
        src={rightPunch ? rightImage.punch : rightImage.normal}
        alt={rightPunch ? "Right Punch" : "Right"}
        draggable={false}
      />
    </div>
  );
}
