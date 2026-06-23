#!/usr/bin/env python3
"""
PSD handler – called by the Tauri (Rust) side via subprocess.
Usage:
  python psd_handler.py parse <psd_path>
  python psd_handler.py export <psd_path> <output_path> <png|jpg> [layer_id ...]
  python psd_handler.py transform <psd_path> <layer_id>
  python psd_handler.py composite <psd_path> <output_path> <visible_names_json>

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


def layer_transform(psd_path: str, layer_id: int) -> dict:
    """Return a layer's sprite size and the offset of its rect center from the canvas center.

    The offset uses the canvas center as the origin and the sprite rect center as the
    measured point, in pixels:
      offset_x: positive to the right of canvas center
      offset_y: positive above canvas center (image y is flipped to an up-positive axis)

    Returns {id, name, width, height, offset_x, offset_y}.
    """
    psd = PSDImage.open(psd_path)
    descendants = list(psd.descendants())
    if layer_id < 0 or layer_id >= len(descendants):
        return {"error": f"Layer id {layer_id} out of range (0..{len(descendants) - 1})"}

    layer = descendants[layer_id]
    if hasattr(layer, "kind") and layer.kind == "pixel":
        x1, y1, x2, y2 = layer.bbox
        top, left, w, h = y1, x1, x2 - x1, y2 - y1
    else:
        top, left = layer.top, layer.left
        w, h = layer.width, layer.height

    # Sprite rect center in image coordinates (origin top-left, y down).
    sprite_cx = left + w / 2
    sprite_cy = top + h / 2
    canvas_cx = psd.width / 2
    canvas_cy = psd.height / 2

    offset_x = sprite_cx - canvas_cx
    offset_y = canvas_cy - sprite_cy  # flip to up-positive

    return {
        "id": layer_id,
        "name": layer.name,
        "width": w,
        "height": h,
        "offset_x": offset_x,
        "offset_y": offset_y,
    }


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

    elif cmd == "transform":
        psd_path = sys.argv[2]
        layer_id = int(sys.argv[3])
        result = layer_transform(psd_path, layer_id)
        print(json.dumps(result))

    else:
        print(json.dumps({"error": f"Unknown command: {cmd}"}), file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
