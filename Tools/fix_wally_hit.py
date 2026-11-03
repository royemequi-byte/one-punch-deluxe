from PIL import Image
import numpy as np

SRC = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Ilustraciones_Nuevas\Wally_Hit_Sheet.jpeg"
OUT = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas\Wally_Hit_Sheet.png"

img = Image.open(SRC).convert("RGB")
arr = np.array(img)
h, w = arr.shape[:2]
frame_w = w // 5
result = np.full_like(arr, 255)  # lienzo blanco

def find_foot_baseline(frame_arr):
    """Fila más baja que no sea fondo blanco (bounding box inferior del personaje)."""
    gray = frame_arr.mean(axis=2)
    # fondo = casi blanco (>220). Personaje = todo lo demás
    char_rows = np.where((gray < 220).any(axis=1))[0]
    return int(char_rows[-1]) if len(char_rows) > 0 else frame_arr.shape[0] - 1

# Obtener la baseline de cada frame
baselines = []
frames = []
for fi in range(5):
    x0 = fi * frame_w
    frame = arr[:, x0:x0+frame_w]
    frames.append(frame)
    baselines.append(find_foot_baseline(frame))

print("Baselines por frame:", baselines)
target = baselines[0]  # frame 1 es la referencia

for fi, (frame, baseline) in enumerate(zip(frames, baselines)):
    offset = target - baseline  # cuánto hay que bajar (positivo = bajar)
    x0 = fi * frame_w

    if offset == 0:
        result[:, x0:x0+frame_w] = frame
        print(f"Frame {fi+1}: sin ajuste")
        continue

    if offset > 0:
        # bajar el personaje: pegar desde offset hacia abajo
        src_rows = frame[:h - offset, :]
        result[offset:, x0:x0+frame_w] = src_rows
    else:
        # subir el personaje (raro en este caso)
        src_rows = frame[-offset:, :]
        result[:h + offset, x0:x0+frame_w] = src_rows

    print(f"Frame {fi+1}: ajustado {offset:+d}px")

Image.fromarray(result).save(OUT, "PNG")
print("Guardado:", OUT)
