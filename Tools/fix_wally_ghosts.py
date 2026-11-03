from PIL import Image
import numpy as np

DST_DIR = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas"
SRC_DIR = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Ilustraciones_Nuevas"
WHITE = np.array([255, 255, 255])

sheets = [
    ("Wally_Hit2_Sheet.jpeg",  "Wally_Hit2_Sheet.png"),
    ("Wally_Idle_Sheet.png",   "Wally_Idle_Sheet.png"),
]

def remove_white_rect_artifacts(arr, h, w, frame_w):
    """
    En frames ghost, elimina rectángulos blancos/grises que quedan
    debajo de los pies. Son manchas casi-blancas (220-250) con forma rectangular.
    """
    strip_top = int(h * 0.70)

    for fi in range(5):
        x0 = fi * frame_w
        x1 = x0 + frame_w
        strip = arr[strip_top:, x0:x1]

        # detectar columnas con mancha casi-blanca (ligeramente gris, no puro blanco)
        gray = strip.mean(axis=2)
        # mancha: gris claro (200-248) — más oscuro que fondo blanco puro (>248)
        artifact_mask = (gray > 200) & (gray < 248)

        artifact_cols = np.where(artifact_mask.any(axis=0))[0]
        if len(artifact_cols) == 0:
            continue

        # solo actuar si hay una mancha significativa (>15px de ancho)
        diffs = np.diff(artifact_cols)
        gaps = np.where(diffs > 10)[0]
        groups = np.split(artifact_cols, gaps + 1)

        for group in groups:
            if len(group) < 15:
                continue
            abs_x0 = x0 + group[0]
            abs_x1 = x0 + group[-1] + 1
            arr[strip_top:, abs_x0:abs_x1] = WHITE

import os

for (src_name, dst_name) in sheets:
    src = os.path.join(SRC_DIR, src_name)
    dst = os.path.join(DST_DIR, dst_name)

    img = Image.open(src).convert("RGB")
    arr = np.array(img)
    h, w = arr.shape[:2]
    frame_w = w // 5

    remove_white_rect_artifacts(arr, h, w, frame_w)
    Image.fromarray(arr).save(dst, "PNG")
    print(f"  OK: {dst_name}")

print("\nListo.")
