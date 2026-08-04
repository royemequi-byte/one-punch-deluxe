"""
organizar_sprites.py — Organiza todos los sprites del proyecto en carpetas limpias.
Crea Assets/Art/Personajes/Wally/ y Assets/Art/Personajes/Rival/
Copia solo los _transparent.png con nombres limpios y elimina el resto.
"""

import shutil
from pathlib import Path

PROYECTO = Path(r"C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets")

# Carpetas de destino
DEST_WALLY = PROYECTO / "Art" / "Personajes" / "Wally"
DEST_RIVAL = PROYECTO / "Art" / "Personajes" / "Rival"
DEST_UI    = PROYECTO / "Art" / "Personajes" / "UI"

DEST_WALLY.mkdir(parents=True, exist_ok=True)
DEST_RIVAL.mkdir(parents=True, exist_ok=True)
DEST_UI.mkdir(parents=True, exist_ok=True)

# Sprites de Wally: origen → nombre final
ORIGEN_WALLY = PROYECTO / "Animations" / "Wally" / "Characters" / "Wally"
WALLY_SPRITES = {
    "wally_idle_new_transparent.png":      "wally_idle.png",
    "wally_jab_new_transparent.png":       "wally_jab.png",
    "wally_straight_new_transparent.png":  "wally_straight.png",
    "wally_hook_new_transparent.png":      "wally_hook.png",
    "wally_block_new_transparent.png":     "wally_block.png",
    "wally_duck_new_transparent.png":      "wally_duck.png",
    "wally_weaveleft_new_transparent.png": "wally_weaveleft.png",
    "wally_weaveright_new_transparent.png":"wally_weaveright.png",
    "wally_backstep_new_transparent.png":  "wally_backstep.png",
    "wally_slip_new_transparent.png":      "wally_slip.png",
    "wally_hit_new_transparent.png":       "wally_hit.png",
    "wally_knockdown_new_transparent.png": "wally_knockdown.png",
    "wally_victory_new_transparent.png":   "wally_victory.png",
    "wally_defeat_new_transparent.png":    "wally_defeat.png",
}

# Sprites del rival: origen → nombre final
ORIGEN_RIVAL = PROYECTO / "logo"
RIVAL_SPRITES = {
    "rival_idle_transparent.png":       "rival_idle.png",
    "rival_attack_transparent.png":     "rival_attack.png",
    "rival_block_transparent.png":      "rival_block.png",
    "rival_hit_transparent.png":        "rival_hit.png",
    "rival_knockdown_transparent.png":  "rival_knockdown.png",
    "rival_victory_transparent.png":    "rival_victory.png",
    "rival_defeat_transparent.png":     "rival_defeat.png",
    "rival_telegraph_transparent.png":  "rival_telegraph.png",
}

# Assets de UI que se conservan
UI_ASSETS = {
    ORIGEN_RIVAL / "logo_splash.png": DEST_UI / "logo_splash.png",
    ORIGEN_RIVAL / "bg_menu.png":     DEST_UI / "bg_menu.png",
}

def copiar_sprites(sprites, origen, destino):
    for archivo, nombre_final in sprites.items():
        src = origen / archivo
        dst = destino / nombre_final
        if src.exists():
            shutil.copy2(src, dst)
            print(f"  OK: {nombre_final}")
        else:
            print(f"  ⚠️  No encontrado: {archivo}")

def eliminar_carpeta(carpeta):
    for archivo in carpeta.iterdir():
        if archivo.name == ".gitkeep" or archivo.suffix == ".controller":
            continue
        if archivo.is_file():
            archivo.unlink()
            print(f"  Eliminado: {archivo.name}")

print("\nCopiando sprites de Wally...")
copiar_sprites(WALLY_SPRITES, ORIGEN_WALLY, DEST_WALLY)

print("\nCopiando sprites del Rival...")
copiar_sprites(RIVAL_SPRITES, ORIGEN_RIVAL, DEST_RIVAL)

print("\nCopiando assets de UI...")
for src, dst in UI_ASSETS.items():
    if src.exists():
        shutil.copy2(src, dst)
        print(f"  OK: {dst.name}")

print("\nLimpiando carpeta de Wally original...")
eliminar_carpeta(ORIGEN_WALLY)

print("\nLimpiando carpeta logo original...")
eliminar_carpeta(ORIGEN_RIVAL)

print("\nListo. Estructura final:")
print(f"  Art/Personajes/Wally/ — {len(list(DEST_WALLY.iterdir()))} sprites")
print(f"  Art/Personajes/Rival/ — {len(list(DEST_RIVAL.iterdir()))} sprites")
print(f"  Art/Personajes/UI/    — {len(list(DEST_UI.iterdir()))} assets")
