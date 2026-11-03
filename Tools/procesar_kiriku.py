"""
procesar_kiriku.py
Elimina el fondo blanco de todos los sprites de Kirikú.
Los frames ya están bien sliceados (254x353 cada uno, 5 por sheet).
Solo necesitan fondo transparente.
"""

import os
import numpy as np
from PIL import Image
from collections import deque

CARPETA = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Art\Personajes\Kiriku"

SPRITES = [
    "kiriku_idle.png",
    "kiriku_jab.png",
    "kiriku_hook.png",
    "kiriku_attack.png",
    "kiriku_hit.png",
    "kiriku_hit2.png",
    "kiriku_block.png",
    "kiriku_weave.png",
    "kiriku_telegraph.png",
    "kiriku_knockdown.png",
    "kiriku_idle2.png",
    "kiriku_idle3.png",
]

def remove_white_bg(img, tolerance=18):
    """
    BFS desde los bordes. Tolerance=18 (mas conservador que Wally=22)
    para no borrar los frames 'fantasma' de motion blur.
    """
    img = img.convert("RGBA")
    arr = np.array(img, dtype=np.uint8)
    h, w = arr.shape[:2]

    near_white = np.all(arr[:, :, :3] >= (255 - tolerance), axis=2)

    visited = np.zeros((h, w), dtype=bool)
    queue = deque()

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

def main():
    ok = 0
    for nombre in SPRITES:
        ruta = os.path.join(CARPETA, nombre)
        if not os.path.exists(ruta):
            print(f"  [FALTA] {nombre}")
            continue

        img = Image.open(ruta)
        modo_original = img.mode

        # Verificar si las ESQUINAS ya son transparentes (fondo real removido)
        if modo_original == "RGBA":
            arr = np.array(img)
            w, h = img.size
            corners = [arr[0,0,3], arr[0,w-1,3], arr[h-1,0,3], arr[h-1,w-1,3]]
            if all(c == 0 for c in corners):
                print(f"  [OK ya] {nombre} (esquinas ya transparentes)")
                ok += 1
                continue

        print(f"  Procesando {nombre} ({img.size[0]}x{img.size[1]})... ", end="", flush=True)
        resultado = remove_white_bg(img, tolerance=18)
        resultado.save(ruta)

        # Verificar resultado
        arr_out = np.array(resultado)
        pixeles_transparentes = (arr_out[:, :, 3] == 0).sum()
        print(f"OK  ({pixeles_transparentes} px eliminados)")
        ok += 1

    print(f"\nListo: {ok}/{len(SPRITES)} sprites procesados.")
    print("Ahora ve a Unity -> en Project haz clic derecho en")
    print("Assets/Art/Personajes/Kiriku -> Reimport All")

if __name__ == "__main__":
    main()
