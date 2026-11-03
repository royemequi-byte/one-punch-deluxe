from PIL import Image, ImageEnhance
import numpy as np

SRC = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Ilustraciones_Nuevas\Kiriku_Idle_Sheet.jpeg"
OUT = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas\Kiriku_Idle_Sheet.png"

img = Image.open(SRC).convert("RGB")
w, h = img.size
frame_w = w // 5

result = img.copy()

for fi in [1, 2, 3]:  # frames 2, 3, 4 (índice 1, 2, 3)
    x0 = fi * frame_w
    x1 = x0 + frame_w
    frame = img.crop((x0, 0, x1, h))

    arr = np.array(frame, dtype=np.float32)

    # máscara: pixels que NO son fondo blanco
    bg = (arr[...,0] > 220) & (arr[...,1] > 220) & (arr[...,2] > 220)
    char_mask = ~bg

    # calcular la media de R, G, B del personaje en este frame
    char_pixels = arr[char_mask]
    if len(char_pixels) == 0:
        continue

    # referencia: frame 1
    ref_frame = img.crop((0, 0, frame_w, h))
    ref_arr = np.array(ref_frame, dtype=np.float32)
    ref_bg = (ref_arr[...,0] > 220) & (ref_arr[...,1] > 220) & (ref_arr[...,2] > 220)
    ref_char = ref_arr[~ref_bg]

    # escalar para que la media de color del ghost iguale la del frame de referencia
    mean_ghost = char_pixels.mean(axis=0)
    mean_ref = ref_char.mean(axis=0)

    # factor de escala por canal (evitar división por cero)
    scale = np.where(mean_ghost > 1, mean_ref / mean_ghost, 1.0)
    scale = np.clip(scale, 0.5, 3.0)

    corrected = arr.copy()
    corrected[char_mask] = np.clip(arr[char_mask] * scale, 0, 255)

    # pegar frame corregido en el resultado
    patch = Image.fromarray(corrected.astype(np.uint8))
    result.paste(patch, (x0, 0))

result.save(OUT, "PNG")
print("Listo:", OUT)
