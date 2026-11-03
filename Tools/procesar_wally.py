"""
procesar_wally.py
Elimina el fondo blanco de cada sprite sheet de Wally y
los copia a Assets/Art/Personajes/Wally/ listos para Unity.
"""

import os
import numpy as np
from PIL import Image
from collections import deque

SRC  = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\input\wally_nuevo"
DEST = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Art\Personajes\Wally"

# nombre_destino → archivo_fuente
ANIMACIONES = {
    "wally_idle":       "wally_stance_sequence.png",
    "wally_straight":   "wally_punch_sequence.png",
    "wally_jab":        "wally_punch_sequence.png",
    "wally_hook":       "wally_punch_sequence.png",
    "wally_hit":        "wally_hit_sequence.png",
    "wally_knockdown":  "wally_hit_sequence2.png",
    "wally_slip":       "wally_slip_sequence4.png",
    "wally_block":      "wally_slip_sequence.png",
    "wally_duck":       "wally_slip_sequence3.png",
    "wally_backstep":   "wally_slip_sequence2.png",
    "wally_weaveleft":  "wally_motion_study.png",
    "wally_weaveright": "wally_motion_sequence.png",
    "wally_victory":    "wally_stance_sequence.png",
    "wally_defeat":     "wally_hit_sequence2.png",
}

def remove_white_bg(img, tolerance=22):
    """BFS desde todos los píxeles del borde que sean casi blancos."""
    img = img.convert("RGBA")
    arr = np.array(img, dtype=np.uint8)
    h, w = arr.shape[:2]

    near_white = np.all(arr[:, :, :3] >= (255 - tolerance), axis=2)

    visited = np.zeros((h, w), dtype=bool)
    queue = deque()

    # Semillas: borde completo
    for c in range(w):
        for r in (0, h - 1):
            if near_white[r, c] and not visited[r, c]:
                visited[r, c] = True
                queue.append((r, c))
    for r in range(1, h - 1):
        for c in (0, w - 1):
            if near_white[r, c] and not visited[r, c]:
                visited[r, c] = True
                queue.append((r, c))

    dirs = [(-1, 0), (1, 0), (0, -1), (0, 1)]
    while queue:
        r, c = queue.popleft()
        arr[r, c, 3] = 0
        for dr, dc in dirs:
            nr, nc = r + dr, c + dc
            if 0 <= nr < h and 0 <= nc < w and not visited[nr, nc] and near_white[nr, nc]:
                visited[nr, nc] = True
                queue.append((nr, nc))

    return Image.fromarray(arr)

def crop_to_multiple_of_5(img):
    """Recorta el ancho al múltiplo de 5 más cercano (hacia abajo)."""
    w, h = img.size
    new_w = w - (w % 5)
    if new_w != w:
        img = img.crop((0, 0, new_w, h))
    return img

def main():
    ok = 0
    for nombre, fuente in ANIMACIONES.items():
        src_path  = os.path.join(SRC, fuente)
        dest_path = os.path.join(DEST, nombre + ".png")

        if not os.path.exists(src_path):
            print(f"  [FALTA] {fuente}")
            continue

        print(f"  {nombre} <- {fuente} ...", end=" ", flush=True)
        img = Image.open(src_path)
        img = remove_white_bg(img)
        img = crop_to_multiple_of_5(img)
        img.save(dest_path)
        print(f"OK  {img.size[0]}x{img.size[1]}  ({img.size[0]//5}px/frame)")
        ok += 1

    print(f"\nListo: {ok}/{len(ANIMACIONES)} sprites guardados en Assets/Art/Personajes/Wally/")

if __name__ == "__main__":
    main()
