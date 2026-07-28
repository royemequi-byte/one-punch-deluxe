"""
normalize_sprites.py
Quita el fondo de un spritesheet, corta los frames por fila y los exporta
como PNGs individuales con fondo transparente, alineados por la base (pies).

Uso:
    python Tools/normalize_sprites.py \
        --input Tools/input/wally/sheet_attacks.jpeg \
        --cols 6 \
        --names Jab,Straight,Hook \
        --output Assets/Art/Characters/Wally \
        --cell 128

Argumentos:
    --input     ruta al spritesheet de entrada (PNG o JPEG)
    --cols      cuántos frames por fila
    --names     nombres de cada fila separados por coma (uno por fila)
    --output    carpeta de salida
    --cell      tamaño de la celda de salida en píxeles (default: 128)
    --tolerance tolerancia para quitar el fondo (default: 40)
    --skip-align  no alinea por la base (usar en Knockdown)
"""

import argparse
import os
from pathlib import Path
from PIL import Image
import numpy as np


def detectar_color_fondo(imagen: Image.Image, muestra: int = 5) -> tuple:
    """Detecta el color de fondo muestreando las esquinas de la imagen."""
    px = imagen.load()
    ancho, alto = imagen.size
    muestras = [
        px[0, 0], px[ancho - 1, 0],
        px[0, alto - 1], px[ancho - 1, alto - 1],
        px[ancho // 2, 0], px[0, alto // 2],
    ]
    # Toma solo los canales RGB (ignora alfa si existe)
    muestras_rgb = [c[:3] if len(c) > 3 else c for c in muestras]
    # Devuelve el promedio como color de fondo
    r = int(sum(c[0] for c in muestras_rgb) / len(muestras_rgb))
    g = int(sum(c[1] for c in muestras_rgb) / len(muestras_rgb))
    b = int(sum(c[2] for c in muestras_rgb) / len(muestras_rgb))
    return (r, g, b)


def quitar_fondo(imagen: Image.Image, tolerancia: int = 40) -> Image.Image:
    """Reemplaza el color de fondo por transparencia."""
    imagen = imagen.convert("RGBA")
    datos = np.array(imagen)

    fondo = detectar_color_fondo(imagen)
    print(f"  Color de fondo detectado: RGB{fondo}")

    r, g, b, a = datos[:, :, 0], datos[:, :, 1], datos[:, :, 2], datos[:, :, 3]

    # Máscara: píxeles cuyo color está cerca del fondo
    mascara = (
        (np.abs(r.astype(int) - fondo[0]) < tolerancia) &
        (np.abs(g.astype(int) - fondo[1]) < tolerancia) &
        (np.abs(b.astype(int) - fondo[2]) < tolerancia)
    )

    datos[mascara] = [0, 0, 0, 0]  # transparente
    return Image.fromarray(datos)


def alinear_por_base(frames: list[Image.Image]) -> list[Image.Image]:
    """Mueve cada frame para que los pies queden a la misma altura."""
    bases = []
    for frame in frames:
        datos = np.array(frame)
        alfa = datos[:, :, 3]
        filas_con_contenido = np.where(alfa.max(axis=1) > 0)[0]
        # Base = fila más baja con contenido visible
        base = filas_con_contenido[-1] if len(filas_con_contenido) > 0 else frame.height - 1
        bases.append(base)

    base_maxima = max(bases)

    frames_alineados = []
    for frame, base in zip(frames, bases):
        desplazamiento = base_maxima - base
        nuevo = Image.new("RGBA", frame.size, (0, 0, 0, 0))
        nuevo.paste(frame, (0, desplazamiento))
        frames_alineados.append(nuevo)

    return frames_alineados


def centrar_en_celda(frame: Image.Image, celda: int) -> Image.Image:
    """Centra el frame en una celda cuadrada de tamaño celda x celda."""
    resultado = Image.new("RGBA", (celda, celda), (0, 0, 0, 0))
    datos = np.array(frame)
    alfa = datos[:, :, 3]

    cols_con_contenido = np.where(alfa.max(axis=0) > 0)[0]
    filas_con_contenido = np.where(alfa.max(axis=1) > 0)[0]

    if len(cols_con_contenido) == 0 or len(filas_con_contenido) == 0:
        return resultado

    x1, x2 = int(cols_con_contenido[0]), int(cols_con_contenido[-1])
    y1, y2 = int(filas_con_contenido[0]), int(filas_con_contenido[-1])
    recortado = frame.crop((x1, y1, x2 + 1, y2 + 1))

    # Escala si el contenido es más grande que la celda
    if recortado.width > celda or recortado.height > celda:
        recortado.thumbnail((celda, celda), Image.NEAREST)

    # Centra horizontalmente, pega desde abajo
    x_offset = (celda - recortado.width) // 2
    y_offset = celda - recortado.height
    resultado.paste(recortado, (x_offset, y_offset), recortado)
    return resultado


def procesar_sheet(input_path: str, cols: int, nombres: list[str],
                   output_dir: str, celda: int, tolerancia: int,
                   skip_align: bool):
    """Procesa un spritesheet completo y exporta un PNG por animación."""

    print(f"\nProcesando: {input_path}")
    imagen = Image.open(input_path).convert("RGBA")

    # Quitar fondo
    print("  Quitando fondo...")
    imagen = quitar_fondo(imagen, tolerancia)

    ancho, alto = imagen.size
    filas = len(nombres)
    alto_fila = alto // filas
    ancho_frame = ancho // cols

    print(f"  Imagen: {ancho}x{alto}px | {filas} filas x {cols} cols")
    print(f"  Tamaño por frame: {ancho_frame}x{alto_fila}px")

    Path(output_dir).mkdir(parents=True, exist_ok=True)

    for i, nombre in enumerate(nombres):
        print(f"  Exportando animación: {nombre}")
        frames = []

        for j in range(cols):
            x = j * ancho_frame
            y = i * alto_fila
            frame = imagen.crop((x, y, x + ancho_frame, y + alto_fila))
            frames.append(frame)

        # Alinear por la base (pies a la misma altura)
        if not skip_align:
            frames = alinear_por_base(frames)

        # Centrar cada frame en celda de salida
        frames_finales = [centrar_en_celda(f, celda) for f in frames]

        # Exportar spritesheet de esta animación (1 fila)
        sheet_salida = Image.new("RGBA", (celda * cols, celda), (0, 0, 0, 0))
        for j, frame in enumerate(frames_finales):
            sheet_salida.paste(frame, (j * celda, 0), frame)

        nombre_archivo = f"wally_{nombre.lower()}.png"
        ruta_salida = os.path.join(output_dir, nombre_archivo)
        sheet_salida.save(ruta_salida)
        print(f"  OK Guardado: {ruta_salida}")


def main():
    parser = argparse.ArgumentParser(
        description="Normaliza spritesheets: quita fondo, corta frames, alinea por la base."
    )
    parser.add_argument("--input", required=True, help="Ruta al spritesheet de entrada")
    parser.add_argument("--cols", type=int, required=True, help="Frames por fila")
    parser.add_argument("--names", required=True, help="Nombres de animaciones separados por coma")
    parser.add_argument("--output", required=True, help="Carpeta de salida")
    parser.add_argument("--cell", type=int, default=128, help="Tamaño de celda de salida (default: 128)")
    parser.add_argument("--tolerance", type=int, default=40, help="Tolerancia para quitar fondo (default: 40)")
    parser.add_argument("--skip-align", action="store_true", help="No alinear por la base (usar en Knockdown)")
    args = parser.parse_args()

    nombres = [n.strip() for n in args.names.split(",")]
    procesar_sheet(
        input_path=args.input,
        cols=args.cols,
        nombres=nombres,
        output_dir=args.output,
        celda=args.cell,
        tolerancia=args.tolerance,
        skip_align=args.skip_align,
    )
    print("\n¡Listo! Revisa la carpeta de salida antes de importar a Unity.")


if __name__ == "__main__":
    main()
