from PIL import Image
from collections import Counter, deque
import os

INPUT_DIR  = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Mejoradas"
OUTPUT_DIR = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Sprites\Transparentes"
TOLERANCE  = 30

os.makedirs(OUTPUT_DIR, exist_ok=True)

def color_dist(c1, c2):
    return sum((a - b) ** 2 for a, b in zip(c1, c2)) ** 0.5

def remove_bg(img_path, out_path, tol):
    img = Image.open(img_path).convert("RGBA")
    data = img.load()
    w, h = img.size

    corners = [data[0,0][:3], data[w-1,0][:3], data[0,h-1][:3], data[w-1,h-1][:3]]
    bg = Counter(corners).most_common(1)[0][0]

    visited = [[False]*h for _ in range(w)]
    queue = deque()

    # sembrar desde todos los bordes
    for x in range(w):
        for y in [0, h-1]:
            if not visited[x][y] and color_dist(data[x,y][:3], bg) < tol:
                queue.append((x, y)); visited[x][y] = True
    for y in range(h):
        for x in [0, w-1]:
            if not visited[x][y] and color_dist(data[x,y][:3], bg) < tol:
                queue.append((x, y)); visited[x][y] = True

    while queue:
        cx, cy = queue.popleft()
        r, g, b, a = data[cx, cy]
        data[cx, cy] = (r, g, b, 0)
        for nx, ny in [(cx-1,cy),(cx+1,cy),(cx,cy-1),(cx,cy+1)]:
            if 0 <= nx < w and 0 <= ny < h and not visited[nx][ny]:
                if color_dist(data[nx,ny][:3], bg) < tol:
                    visited[nx][ny] = True
                    queue.append((nx, ny))

    img.save(out_path, "PNG")

files = [f for f in os.listdir(INPUT_DIR) if f.lower().endswith(".png")]
print(f"Procesando {len(files)} imagen(es)...\n")

for fname in files:
    src = os.path.join(INPUT_DIR, fname)
    dst = os.path.join(OUTPUT_DIR, fname)
    print(f"-> {fname}")
    try:
        remove_bg(src, dst, TOLERANCE)
        print(f"   OK")
    except Exception as e:
        print(f"   ERROR: {e}")

print(f"\nListo. PNGs transparentes en:\n{OUTPUT_DIR}")
