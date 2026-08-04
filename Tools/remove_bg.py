"""
remove_bg.py — Elimina el fondo de imágenes de personajes usando IA.
Uso: python remove_bg.py <archivo.png>
Resultado: guarda <archivo>_transparent.png en la misma carpeta.
"""

import sys
from pathlib import Path
from PIL import Image
from rembg import remove


def remover_fondo(ruta_entrada: str) -> None:
    entrada = Path(ruta_entrada)

    if not entrada.exists():
        print(f"Error: no se encontró el archivo '{entrada}'")
        sys.exit(1)

    if entrada.suffix.lower() not in (".png", ".jpg", ".jpeg", ".webp"):
        print("Error: formatos aceptados: PNG, JPG, JPEG, WEBP")
        sys.exit(1)

    print(f"Procesando: {entrada.name} ...")

    with open(entrada, "rb") as f:
        datos = f.read()

    resultado = remove(datos)

    salida = entrada.parent / (entrada.stem + "_transparent.png")
    with open(salida, "wb") as f:
        f.write(resultado)

    print(f"Guardado: {salida}")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Uso: python remove_bg.py <ruta/al/archivo.png>")
        sys.exit(1)

    remover_fondo(sys.argv[1])
