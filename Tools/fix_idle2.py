from PIL import Image
import numpy as np
from collections import deque

SRC = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Para_Mejorar\Kiriku_Idle2_Sheet.jpeg"
OUT = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas\Kiriku_Idle_Sheet.png"

img = Image.open(SRC).convert("RGB")
arr = np.array(img, dtype=np.uint8)
h, w = arr.shape[:2]

WHITE = np.array([255, 255, 255], dtype=np.uint8)

def is_bg(px):
    return int(px[0]) + int(px[1]) + int(px[2]) > 600  # casi blanco

# --- 1. Eliminar marcas "L" y rectángulos blancos debajo de los pies ---
# Buscar componentes oscuras pequeñas en la franja inferior (último 20% de altura)
# que no estén conectadas al cuerpo principal del personaje

# Eliminar marcas pequeñas en la mitad inferior usando componentes conectados
# Se buscan regiones oscuras aisladas (< 500 px) debajo del 60% de altura
from scipy import ndimage

gray = np.mean(arr, axis=2)
dark_mask = gray < 180  # píxels oscuros

labeled, num = ndimage.label(dark_mask)
sizes = ndimage.sum(dark_mask, labeled, range(1, num+1))

for i, size in enumerate(sizes, start=1):
    if size < 500:
        coords = np.where(labeled == i)
        # solo borrar si está en la mitad inferior
        if coords[0].min() > h * 0.6:
            arr[coords] = WHITE

Image.fromarray(arr).save(OUT, "PNG")
print("Listo:", OUT)
