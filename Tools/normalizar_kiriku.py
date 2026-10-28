"""
Normaliza sprites de Kirikú: detecta automáticamente las filas con contenido
usando el canal alpha, recorta esas filas (ancho completo del strip) y escala
verticalmente hasta CANVAS_H. El ancho permanece igual para no romper el layout
de frames que Unity ya tiene configurado.

Fondo: transparente (0,0,0,0). No toca sprites cuyo contenido ya ocupe ≥98% del alto.
"""
from PIL import Image
import os

KIRIKU   = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Art\Personajes\Kiriku"
CANVAS_W = 1370
CANVAS_H = 353

ARCHIVOS = [
    "kiriku_attack.png",
    "kiriku_block.png",
    "kiriku_hit.png",
    "kiriku_hit2.png",
    "kiriku_hook.png",
    "kiriku_idle.png",
    "kiriku_idle2.png",
    "kiriku_idle3.png",
    "kiriku_jab.png",
    "kiriku_knockdown.png",
    "kiriku_telegraph.png",
    "kiriku_weave.png",
]

for fname in ARCHIVOS:
    path = os.path.join(KIRIKU, fname)
    if not os.path.exists(path):
        print(f"FALTA: {fname}")
        continue

    img = Image.open(path).convert("RGBA")
    W, H = img.size

    # Detectar bbox por canal alpha (funciona con fondos transparentes)
    _, _, _, a_ch = img.split()
    bbox = a_ch.getbbox()

    if bbox is None:
        print(f"SKIP (vacío): {fname}")
        continue

    x0, y0, x1, y1 = bbox
    ch = y1 - y0  # alto del contenido visible

    # Si ya ocupa ≥98 % del canvas, no hace falta reescalar
    if ch >= CANVAS_H * 0.98:
        print(f"OK (sin cambio): {fname}  contenido={ch}px ya llena el canvas")
        continue

    escala = CANVAS_H / ch

    # Recortar filas con contenido (ancho completo para preservar frames)
    contenido = img.crop((0, y0, W, y1))   # W × ch

    # Escalar solo en vertical (stretch); el ancho no cambia para no
    # alterar los límites de frames que el Sprite Editor de Unity ya tiene.
    contenido = contenido.resize((W, CANVAS_H), Image.LANCZOS)

    # Canvas transparente — pegar con máscara alpha
    canvas = Image.new("RGBA", (CANVAS_W, CANVAS_H), (0, 0, 0, 0))
    canvas.paste(contenido, (0, 0), contenido)
    canvas.save(path)

    print(f"OK: {fname}  {ch}px -> {CANVAS_H}px  (x{escala:.2f})  bbox=({x0},{y0}->{x1},{y1})")

print("\nListo.")
