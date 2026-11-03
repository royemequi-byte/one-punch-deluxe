"""
Normaliza todos los sprite sheets transparentes:
- Separa 5 frames por sheet
- Alinea pies al mismo nivel (base)
- Centra horizontalmente en celda uniforme
- Exporta sheet uniforme listo para Unity
"""
from PIL import Image
import numpy as np
import os

INPUT_DIR  = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Transparentes"
OUTPUT_DIR = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Normalizados"
FRAMES     = 5
MARGIN     = 20   # px de margen alrededor del personaje

os.makedirs(OUTPUT_DIR, exist_ok=True)

def get_bbox(arr_rgba):
    alpha = arr_rgba[:, :, 3]
    rows = np.any(alpha > 10, axis=1)
    cols = np.any(alpha > 10, axis=0)
    if not rows.any():
        return None
    r0, r1 = int(np.where(rows)[0][0]),  int(np.where(rows)[0][-1])
    c0, c1 = int(np.where(cols)[0][0]),  int(np.where(cols)[0][-1])
    return c0, r0, c1+1, r1+1

def process(src, dst):
    img = Image.open(src).convert("RGBA")
    w, h = img.size
    fw = w // FRAMES

    # --- extraer frames y bboxes ---
    frames, bboxes = [], []
    for i in range(FRAMES):
        f = np.array(img.crop((i*fw, 0, (i+1)*fw, h)))
        frames.append(f)
        bboxes.append(get_bbox(f))

    valid = [bb for bb in bboxes if bb]
    if not valid:
        print(f"  SKIP (sin contenido): {os.path.basename(src)}")
        return

    # --- celda uniforme basada en el personaje más grande ---
    cell_w = max(x1-x0 for x0,y0,x1,y1 in valid) + MARGIN*2
    cell_h = max(y1-y0 for x0,y0,x1,y1 in valid) + MARGIN*2

    # --- construir sheet ---
    result = Image.new("RGBA", (cell_w * FRAMES, cell_h), (0,0,0,0))

    for i, (f, bb) in enumerate(zip(frames, bboxes)):
        if bb is None:
            continue
        x0, y0, x1, y1 = bb
        char = Image.fromarray(f[y0:y1, x0:x1])
        cw, ch = char.size

        # centrado horizontal, alineado por la base (pies abajo)
        px = i * cell_w + (cell_w - cw) // 2
        py = cell_h - ch - MARGIN
        result.paste(char, (px, py), char)

    result.save(dst, "PNG")
    print(f"  OK  {os.path.basename(dst)}  ({cell_w}x{cell_h} por celda)")

files = sorted(f for f in os.listdir(INPUT_DIR) if f.endswith(".png"))
print(f"Normalizando {len(files)} sheets...\n")
for fname in files:
    process(os.path.join(INPUT_DIR, fname), os.path.join(OUTPUT_DIR, fname))

print(f"\nListo → {OUTPUT_DIR}")
