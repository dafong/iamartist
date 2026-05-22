import type { LayerInfo } from "../types";

interface Props {
  layers: LayerInfo[];
  selected: Set<number>;
  onToggle: (id: number) => void;
  onSelectAll: () => void;
  onClear: () => void;
}

export default function LayerList({ layers, selected, onToggle, onSelectAll, onClear }: Props) {
  if (layers.length === 0) return null;

  return (
    <section className="panel">
      <div className="layer-header">
        <h2>图层列表 ({layers.length})</h2>
        <div className="layer-actions">
          <button onClick={onSelectAll}>全选</button>
          <button onClick={onClear}>清空</button>
        </div>
      </div>
      <ul className="layer-list">
        {layers.map((layer) => (
          <li
            key={layer.id}
            className={selected.has(layer.id) ? "selected" : ""}
            onClick={() => onToggle(layer.id)}
          >
            <input
              type="checkbox"
              checked={selected.has(layer.id)}
              onChange={() => onToggle(layer.id)}
              onClick={(e) => e.stopPropagation()}
            />
            <span className="layer-name">{layer.name}</span>
            <span className="layer-meta">
              {layer.width}×{layer.height}
              {!layer.visible && " (隐藏)"}
            </span>
          </li>
        ))}
      </ul>
    </section>
  );
}
