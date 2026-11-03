from PIL import Image
import numpy as np
from collections import deque
import os

def quitar_fondo(data, tolerancia=5):
    H, W = data.shape[:2]
    mask = np.zeros((H, W), dtype=bool)
    cola = deque()
    ref = data[0, 0, :3].astype(int)

    for x in range(W):
        for y in [0, H-1]:
            if not mask[y, x]:
                mask[y, x] = True
                cola.append((y, x))
    for y in range(H):
        for x in [0, W-1]:
            if not mask[y, x]:
                mask[y, x] = True
                cola.append((y, x))

    while cola:
        y, x = cola.popleft()
        diff = int(np.abs(data[y, x, :3].astype(int) - ref).max())
        if diff > tolerancia:
            continue
        data[y, x, 3] = 0
        for dy, dx in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
            ny, nx = y + dy, x + dx
            if 0 <= ny < H and 0 <= nx < W and not mask[ny, nx]:
                mask[ny, nx] = True
                cola.append((ny, nx))
    return data


WALLY = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Art\Personajes\Wally"

# Solo procesar los sprites nuevos que tienen fondo blanco opaco
nuevos = ["wally_idle.png", "wally_straight.png", "wally_block.png",
          "wally_hit.png", "wally_jab.png", "wally_hook.png"]

for fname in nuevos:
    path = os.path.join(WALLY, fname)
    img = Image.open(path).convert("RGBA")
    data = np.array(img)

    # Solo procesar si el fondo es opaco (alpha=255 en esquina)
    if data[0, 0, 3] == 255:
        data = quitar_fondo(data.copy(), tolerancia=5)
        Image.fromarray(data).save(path)
        print(f"OK (fondo quitado): {fname}")
    else:
        print(f"OK (ya transparente): {fname}")

print("Listo.")
