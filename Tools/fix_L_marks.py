from PIL import Image
import numpy as np

SRC = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Para_Mejorar\Kiriku_Idle2_Sheet.jpeg"
OUT = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas\Kiriku_Idle_Sheet.png"

img = Image.open(SRC).convert("RGB")
arr = np.array(img)
h, w = arr.shape[:2]  # 449 x 1458
frame_w = w // 5      # 291
WHITE = np.array([255, 255, 255])

strip_top = int(h * 0.72)  # desde fila ~323 — captura el borde superior de las cajas

# Coordenadas exactas de las marcas (detectadas empíricamente)
# Formato: (x_inicio, x_fin) — se pinta blanco desde strip_top hasta el fondo
marcas = [
    (191, 261),   # Frame 1
    (489, 552),   # Frame 2
    (776, 838),   # Frame 3
    (950, 1115),  # Frame 4 — extendido a la izquierda para capturar línea delgada
    (1357, 1382), # Frame 5
]

for i, (mx0, mx1) in enumerate(marcas):
    arr[strip_top:, mx0:mx1] = WHITE
    print(f"Frame {i+1}: borrado x={mx0}-{mx1}")

Image.fromarray(arr).save(OUT, "PNG")
print("Guardado:", OUT)
