from PIL import Image
from collections import Counter, deque
import os

INPUT_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(INPUT_DIR, "transparent")
TOLERANCE = 35

os.makedirs(OUTPUT_DIR, exist_ok=True)

extensions = (".png", ".jpg", ".jpeg")

def color_distance(c1, c2):
    return sum((a - b) ** 2 for a, b in zip(c1[:3], c2[:3])) ** 0.5

def remove_background(img_path, out_path, tolerance):
    img = Image.open(img_path).convert("RGBA")
    data = img.load()
    w, h = img.size

    # detectar color de fondo por las 4 esquinas
    corners = [data[0,0][:3], data[w-1,0][:3], data[0,h-1][:3], data[w-1,h-1][:3]]
    bg_color = Counter(corners).most_common(1)[0][0]

    # flood fill desde todos los bordes — solo elimina fondo conectado al exterior
    visited = [[False]*h for _ in range(w)]
    queue = deque()

    for x in range(w):
        for y in [0, h-1]:
            if not visited[x][y] and color_distance(data[x,y][:3], bg_color) < tolerance:
                queue.append((x, y))
                visited[x][y] = True
    for y in range(h):
        for x in [0, w-1]:
            if not visited[x][y] and color_distance(data[x,y][:3], bg_color) < tolerance:
                queue.append((x, y))
                visited[x][y] = True

    while queue:
        cx, cy = queue.popleft()
        r, g, b, a = data[cx, cy]
        data[cx, cy] = (r, g, b, 0)
        for nx, ny in [(cx-1,cy),(cx+1,cy),(cx,cy-1),(cx,cy+1)]:
            if 0 <= nx < w and 0 <= ny < h and not visited[nx][ny]:
                if color_distance(data[nx,ny][:3], bg_color) < tolerance:
                    visited[nx][ny] = True
                    queue.append((nx, ny))

    img.save(out_path, "PNG")
    print(f"  OK: {os.path.basename(out_path)}")

files = [f for f in os.listdir(INPUT_DIR) if f.lower().endswith(extensions) and f != "remove_bg.py"]
print(f"Procesando {len(files)} imagen(es)...\n")

for fname in files:
    in_path = os.path.join(INPUT_DIR, fname)
    out_name = os.path.splitext(fname)[0] + ".png"
    out_path = os.path.join(OUTPUT_DIR, out_name)
    print(f"-> {fname}")
    try:
        remove_background(in_path, out_path, TOLERANCE)
    except Exception as e:
        print(f"  ERROR: {e}")

print(f"\nListo. PNGs transparentes en:\n{OUTPUT_DIR}")
