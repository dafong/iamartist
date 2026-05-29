import sys
import json
from psd_tools import PSDImage


def main():
    if len(sys.argv) != 4:
        print("usage: psd_composite <psd_path> <output_path> <visible_names_json>", file=sys.stderr)
        sys.exit(1)

    psd_path = sys.argv[1]
    output_path = sys.argv[2]
    visible_names = set(json.loads(sys.argv[3]))

    psd = PSDImage.open(psd_path)
    result = psd.composite(layer_filter=lambda layer: layer.name in visible_names)
    result.save(output_path)


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(str(e), file=sys.stderr)
        sys.exit(1)
