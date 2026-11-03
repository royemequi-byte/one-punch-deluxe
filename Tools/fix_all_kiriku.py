from PIL import Image
import numpy as np

SRC_DIR = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Para_Mejorar"
DST_DIR = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas"
WHITE = np.array([255, 255, 255])

# Sheets a procesar con fix de marcas L
sheets = [
    "Kiriku_Attack_Sheet.jpeg",
    "Kiriku_Block_Sheet.jpeg",
    "Kiriku_Hit_Sheet.jpeg",
    "Kiriku_Hit2_Sheet.jpeg",
    "Kiriku_Hook_Sheet.jpeg",
    "Kiriku_Idle2_Sheet.jpeg",
    "Kiriku_Idle3_Sheet.jpeg",
    "Kiriku_Jab_Sheet.jpeg",
    "Kiriku_Knockdown_Sheet.jpeg",
    "Kiriku_Telegraph_Sheet.jpeg",
    "Kiriku_Weave_Sheet.jpeg",
]

def fix_marks(arr, h, w, frame_w):
    """Elimina marcas L/caja de la franja inferior de cada frame."""
    strip_top = int(h * 0.72)

    for fi in range(5):
        x0 = fi * frame_w
        x1 = x0 + frame_w

        # Buscar en el 48% derecho del frame con threshold alto
        search_x0 = x0 + int(frame_w * 0.52)
        strip = arr[strip_top:, search_x0:x1]
        gray = strip.mean(axis=2)
        dark_cols = np.where((gray < 215).any(axis=0))[0]

        if len(dark_cols) == 0:
            continue

        # Detectar gap entre personaje y marca
        diffs = np.diff(dark_cols)
        gaps = np.where(diffs > 4)[0]

        if len(gaps) > 0:
            mark_start = dark_cols[gaps[-1] + 1]
            mark_end = dark_cols[-1] + 1
        else:
            # sin gap: toda la zona derecha podría ser marca
            # solo pintar si es estrecho (< 80px)
            if dark_cols[-1] - dark_cols[0] < 80:
                mark_start = dark_cols[0]
                mark_end = dark_cols[-1] + 1
            else:
                continue

        # Extender 25px a la izquierda para capturar bordes de caja
        abs_x0 = max(x0, search_x0 + mark_start - 25)
        abs_x1 = search_x0 + mark_end
        arr[strip_top:, abs_x0:abs_x1] = WHITE

import os

for fname in sheets:
    src = os.path.join(SRC_DIR, fname)
    out_name = os.path.splitext(fname)[0] + ".png"
    dst = os.path.join(DST_DIR, out_name)

    img = Image.open(src).convert("RGB")
    arr = np.array(img)
    h, w = arr.shape[:2]
    frame_w = w // 5

    fix_marks(arr, h, w, frame_w)
    Image.fromarray(arr).save(dst, "PNG")
    print(f"  OK: {out_name}")

print("\nTodos los Kirikú procesados.")
