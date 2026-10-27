"""
Genera el fondo del ring de boxeo para ONE PUNCH DELUXE.
Estilo manga/anime oscuro, compatible con los sprites de Wally y el Rival.
Resolución: 1920x1080 (16:9).
"""
from PIL import Image, ImageDraw, ImageFilter
import math

W, H = 1920, 1080

img = Image.new("RGBA", (W, H), (0, 0, 0, 255))
draw = ImageDraw.Draw(img)

# --- Fondo: gradiente oscuro de arena ---
for y in range(H):
    t = y / H
    r = int(8 + t * 20)
    g = int(5 + t * 10)
    b = int(18 + t * 25)
    draw.line([(0, y), (W, y)], fill=(r, g, b, 255))

# --- Multitud de fondo (siluetas difusas) ---
import random
random.seed(42)
crowd_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
cdraw = ImageDraw.Draw(crowd_layer)
crowd_top = int(H * 0.05)
crowd_bottom = int(H * 0.38)
for _ in range(350):
    x = random.randint(0, W)
    y = random.randint(crowd_top, crowd_bottom)
    w = random.randint(14, 28)
    h_head = random.randint(18, 35)
    h_body = random.randint(40, 80)
    dark = random.randint(25, 50)
    alpha = random.randint(120, 200)
    color = (dark, dark, dark + 8, alpha)
    cdraw.ellipse([x - w//2, y - h_head, x + w//2, y], fill=color)
    cdraw.rectangle([x - w//2 - 2, y, x + w//2 + 2, y + h_body], fill=color)

crowd_blur = crowd_layer.filter(ImageFilter.GaussianBlur(radius=3))
img = Image.alpha_composite(img.convert("RGBA"), crowd_blur)
draw = ImageDraw.Draw(img)

# --- Luces de arena (halo difuso desde arriba) ---
lights_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
ldraw = ImageDraw.Draw(lights_layer)
light_positions = [int(W * 0.2), int(W * 0.5), int(W * 0.8)]
for lx in light_positions:
    for r in range(180, 0, -10):
        alpha = int(18 * (1 - r / 180))
        ldraw.ellipse([lx - r * 3, -r, lx + r * 3, r * 2], fill=(255, 240, 180, alpha))
lights_blur = lights_layer.filter(ImageFilter.GaussianBlur(radius=12))
img = Image.alpha_composite(img, lights_blur)
draw = ImageDraw.Draw(img)

# --- Piso del ring (perspectiva isométrica) ---
ring_top_y = int(H * 0.48)
ring_bot_y = int(H * 0.92)
ring_left_x = int(W * 0.04)
ring_right_x = int(W * 0.96)
canvas_color = (34, 85, 34, 255)
draw.polygon([
    (int(W * 0.08), ring_top_y),
    (int(W * 0.92), ring_top_y),
    (ring_right_x, ring_bot_y),
    (ring_left_x, ring_bot_y),
], fill=canvas_color)

# Líneas del ring (cuadrícula leve en el canvas)
grid_color = (28, 72, 28, 255)
for i in range(1, 5):
    t = i / 5
    # Líneas horizontales
    lx1 = int(W * 0.08 + (ring_left_x - W * 0.08) * t)
    lx2 = int(W * 0.92 + (ring_right_x - W * 0.92) * t)
    ly = int(ring_top_y + (ring_bot_y - ring_top_y) * t)
    draw.line([(lx1, ly), (lx2, ly)], fill=grid_color, width=1)

# Borde frontal del ring (suelo)
draw.line([(ring_left_x, ring_bot_y), (ring_right_x, ring_bot_y)], fill=(20, 55, 20, 255), width=3)
draw.rectangle([ring_left_x, ring_bot_y, ring_right_x, H], fill=(12, 30, 12, 255))

# --- Esquinas del ring (postes) ---
post_color = (200, 180, 60, 255)
post_shadow = (80, 70, 20, 180)
posts = [
    (int(W * 0.08), ring_top_y),
    (int(W * 0.92), ring_top_y),
    (ring_left_x, ring_bot_y),
    (ring_right_x, ring_bot_y),
]
for px, py in posts:
    draw.ellipse([px - 6, py - 130, px + 6, py + 6], fill=post_color)
    draw.ellipse([px - 8, py - 132, px + 8, py + 8], outline=post_shadow, width=2)

# --- Cuerdas del ring (3 cuerdas) ---
rope_colors = [(200, 50, 50, 255), (200, 50, 50, 255), (190, 190, 190, 255)]
rope_y_offsets = [-100, -55, -15]
for i, (ry_off, rcol) in enumerate(zip(rope_y_offsets, rope_colors)):
    pts_top = [
        (int(W * 0.08), ring_top_y + ry_off),
        (int(W * 0.92), ring_top_y + ry_off),
    ]
    pts_bot = [
        (ring_left_x, ring_bot_y + ry_off),
        (ring_right_x, ring_bot_y + ry_off),
    ]
    for pts in [pts_top, pts_bot]:
        draw.line(pts, fill=rcol, width=5)

# Conectar cuerdas esquina a esquina (laterales)
for ry_off in rope_y_offsets:
    draw.line([
        (int(W * 0.08), ring_top_y + ry_off),
        (ring_left_x, ring_bot_y + ry_off),
    ], fill=(190, 190, 190, 255), width=3)
    draw.line([
        (int(W * 0.92), ring_top_y + ry_off),
        (ring_right_x, ring_bot_y + ry_off),
    ], fill=(190, 190, 190, 255), width=3)

# --- Spotlight central sobre el ring ---
spot_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
sdraw = ImageDraw.Draw(spot_layer)
cx, cy = W // 2, int(H * 0.62)
for r in range(320, 0, -8):
    alpha = int(28 * (1 - r / 320))
    sdraw.ellipse([cx - r * 2, cy - r, cx + r * 2, cy + r], fill=(255, 255, 200, alpha))
spot_blur = spot_layer.filter(ImageFilter.GaussianBlur(radius=20))
img = Image.alpha_composite(img, spot_blur)

# --- Guardar ---
out_path = r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Art\Backgrounds\Ring\ring_bg.png"
img_final = img.convert("RGB")
img_final.save(out_path, "PNG", optimize=False)
print(f"Fondo generado: {out_path}")
print(f"Tamaño: {W}x{H}")
