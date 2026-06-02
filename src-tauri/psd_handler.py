#!/usr/bin/env python3
"""
PSD handler – called by the Tauri (Rust) side via subprocess.
Usage:
  python psd_handler.py parse <psd_path>
  python psd_handler.py export <psd_path> <output_path> <png|jpg> [layer_id ...]

All results are written to stdout as JSON.
"""
import json
import os
import sys
from pathlib import Path

from psd_tools import PSDImage


def parse_psd(psd_path: str) -> dict:
    """Return {width, height, layers: [{id, name, width, height, top, left, visible}]}."""
    psd = PSDImage.open(psd_path)
    layers = []
    for i, layer in enumerate(psd.descendants()):
        # psd-tools ≥ 1.9: bbox for pixel layers, top/left for groups
        if hasattr(layer, "kind") and layer.kind == "pixel":
            x1, y1, x2, y2 = layer.bbox
            top, left, w, h = y1, x1, x2 - x1, y2 - y1
        else:
            top, left = layer.top, layer.left
            w, h = layer.width, layer.height
        layers.append(
            {
                "id": i,
                "name": layer.name,
                "width": w,
                "height": h,
                "top": top,
                "left": left,
                "visible": layer.visible,
            }
        )
    return {"width": psd.width, "height": psd.height, "layers": layers}


def composite_layers(psd_path: str, layer_ids: list[int], output_path: str, fmt: str) -> dict:
    """Composite only the specified layers into a single image and save.
    Returns {path: str}."""
    psd = PSDImage.open(psd_path)
    descendants = list(psd.descendants())
    layer_id_set = set(layer_ids)

    # Temporarily set visibility: only enabled layers are visible
    saved_visibility = []
    for i, layer in enumerate(descendants):
        saved_visibility.append(layer.visible)
        layer.visible = i in layer_id_set

    try:
        image = psd.composite()
    finally:
        for layer, orig in zip(descendants, saved_visibility):
            layer.visible = orig

    ext = fmt.lower()
    os.makedirs(Path(output_path).parent, exist_ok=True)
    if ext in ("jpg", "jpeg"):
        image = image.convert("RGB")
    image.save(output_path)

    return {"path": str(output_path)}


def main() -> None:
    if len(sys.argv) < 3:
        print(json.dumps({"error": "Usage: psd_handler.py parse|export ..."}))
        sys.exit(1)

    cmd = sys.argv[1]

    if cmd == "parse":
        result = parse_psd(sys.argv[2])
        print(json.dumps(result))

    elif cmd == "export":
        psd_path = sys.argv[2]
        output_path = sys.argv[3]
        fmt = sys.argv[4]
        layer_ids = [int(x) for x in sys.argv[5:]]
        result = composite_layers(psd_path, layer_ids, output_path, fmt)
        print(json.dumps(result))

    else:
        print(json.dumps({"error": f"Unknown command: {cmd}"}), file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
