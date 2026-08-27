#!/usr/bin/env python3
"""Split cocos2d TexturePacker (format 2) .plist/.png atlases into individual PNGs.

Usage:
    python split_plist.py <atlas.plist> <atlas.png> <out_dir>

Handles: frame rect, rotated (90 CW in TP format-2), offset, sourceSize.
Restores each sprite to its full sourceSize canvas with correct trim offset,
so Unity can import them with original dimensions.
"""
import re
import sys
import os
import plistlib
from PIL import Image


def parse_rect(s):
    """Parse '{{x,y},{w,h}}' or '{x,y}' -> tuple of the numbers found."""
    return [int(n) for n in re.findall(r"-?\d+", s)]


def split(plist_path, png_path, out_dir):
    with open(plist_path, "rb") as f:
        data = plistlib.load(f)

    atlas = Image.open(png_path).convert("RGBA")
    os.makedirs(out_dir, exist_ok=True)

    count = 0
    for name, info in data["frames"].items():
        fx, fy, fw, fh = parse_rect(info["frame"])
        off = parse_rect(info["offset"])
        ox, oy = (off + [0, 0])[:2]
        sx, sy = parse_rect(info["sourceSize"])
        # sourceColorRect = {{cx,cy},{cw,ch}} where cw==fw (unless rotated)
        scr = info.get("sourceColorRect", "{{0,0},{%d,%d}}" % (fw, fh))
        cx, cy = parse_rect(scr)[0], parse_rect(scr)[1]
        rotated = bool(info.get("rotated", False))

        region = atlas.crop((fx, fy, fx + (fh if rotated else fw),
                             fy + (fw if rotated else fh)))
        if rotated:
            # TP format-2 'rotated' = stored rotated 90deg clockwise; rotate back CCW
            region = region.rotate(90, expand=True)

        # Paste into full source-size transparent canvas at trim position.
        # Cocos y-axis is bottom-up; offset oy is positive-up. Convert to top-down.
        canvas = Image.new("RGBA", (sx, sy), (0, 0, 0, 0))
        px = cx
        py = sy - cy - region.height + (oy if oy else 0)
        py = max(0, min(py, sy - region.height))
        px = max(0, min(px, sx - region.width))
        canvas.paste(region, (px, py))

        out_name = name.replace("/", "_")
        canvas.save(os.path.join(out_dir, out_name))
        count += 1
        print(f"  {out_name}: {sx}x{sy} (frame {'rot' if rotated else ''}{fw}x{fh})")

    print(f"-> {count} sprites -> {out_dir}")


if __name__ == "__main__":
    if len(sys.argv) != 4:
        print(__doc__)
        sys.exit(1)
    split(sys.argv[1], sys.argv[2], sys.argv[3])
