# ARQUITECTURA PARA EL EQUIPO — ONE PUNCH DELUXE

> Referencia rápida de decisiones técnicas y estándares del proyecto.

---

## Estándar Visual del Proyecto

### 1. Estilo visual base

Ilustración digital estilo manga/cómic de acción. Paleta rojo y dorado sobre fondos oscuros, alto contraste, estética arcade de boxeo retro en alta resolución.

### 2. Prompts de imagen para IA

Cuando el juego necesite una imagen nueva, usar este formato:

**[qué] + estilo base fijo + detalle técnico**

El estilo base fijo va en TODOS los prompts:

> `digital manga comic illustration style, red and gold color palette, dark moody background, high contrast, retro arcade boxing game aesthetic, high resolution, cinematic lighting`

Siempre termina con `no text`.

| Tipo de imagen | Añadir al final del prompt |
|---|---|
| Personajes / objetos | `isolated on plain white background, centered, full body` |
| Fondos | `wide shot, 16:9 aspect ratio, no characters` |
| Íconos | `simple clean icon, centered, plain background` |

**Ejemplo completo para un fondo de ring:**
> `boxing ring interior, digital manga comic illustration style, red and gold color palette, dark moody background, high contrast, retro arcade boxing game aesthetic, high resolution, cinematic lighting, wide shot, 16:9 aspect ratio, no characters, no text`

### 3. Importación en Unity (obligatorio)

| Campo | Valor |
|---|---|
| Compression | None o High Quality |
| Max Size | 2048 o 4096 |
| Filter Mode | Point para pixel art / Bilinear para ilustraciones |
| Alpha Is Transparency | ✅ siempre marcado en PNG con alfa |

Nada borroso ni comprimido a baja calidad.

### 4. Textos

- Siempre **TextMeshPro**. Nunca Text legacy.
- Nunca dejar textos por defecto (`New Text`, `Text`, `Label`).
- Fuentes propias importadas en `Assets/Fonts/`. No usar Arial.

### 5. Nomenclatura

- Objetos en Hierarchy: nombres descriptivos (`LogoSplash`, `FondoNegro`, `TextoAvanzar`). Nunca `Image`, `Text`, `GameObject`.
- Archivos: `snake_case` para assets (`logo_splash.png`, `bg_menu.png`).
- Carpetas ordenadas bajo `Assets/Art/UI/`, `Assets/Art/Backgrounds/`, etc.
