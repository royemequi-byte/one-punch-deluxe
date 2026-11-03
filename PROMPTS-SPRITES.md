# PROMPTS PARA GENERAR SPRITES — ONE PUNCH DELUXE

> Objetivo: conseguir animaciones de 8 frames por acción, en el mismo estilo
> detallado de Kirikú, para que el movimiento deje de verse "cuadriculado".
> Los originales actuales no se tocan: lo nuevo va a `Assets/Art/Personajes/.../Nuevo/`.

---

## 1. Reglas técnicas (importantísimas)

Pídele siempre al generador **una sola fila horizontal** con los frames:

| Dato | Valor |
|---|---|
| Formato | PNG con transparencia |
| Tamaño por frame | 512 × 512 px |
| Frames por acción | 6 u 8, en una sola fila |
| Personaje | centrado horizontalmente |
| Altura del personaje | ~430 px en todos los frames |
| Pies | misma línea de base en todos los frames (y ≈ 470) |
| Fondo | transparente, sin sombra, sin texto, sin marca de agua |
| Estilo | pintura anime 2D, tinta gruesa, cel shading, luz dramática |
| **Prohibido** | motion blur, estelas, frames semitransparentes, duplicados fantasma |

> Los "fantasmas" translúcidos son los que hacían ver azul al rival sobre el ring.
> Si el generador los añade, hay que repetir el prompt.

Cada acción se entrega como **un PNG aparte** con este nombre:

```
wally_idle_8.png
wally_walk_8.png
wally_dash_6.png
wally_jump_6.png
wally_jab_6.png
wally_straight_6.png
wally_hook_6.png
wally_block_4.png
wally_hit_4.png
wally_hit_heavy_4.png
wally_knockdown_6.png
wally_victory_6.png
wally_defeat_6.png
```

Y lo mismo con `kiriku_` en lugar de `wally_`.

---

## 2. Prompt maestro de estilo (pégalo siempre al inicio)

```
2D hand-painted anime boxing game sprite sheet, style of Ashita no Joe and
King of Fighters 2000, thick black ink outlines, cel shading with painterly
highlights, dramatic arena lighting, highly detailed anatomy, transparent
background, single horizontal row of EXACTLY 8 frames, each frame 512x512,
character centered, full body, feet on the same baseline, identical character
design, identical colors and proportions in every frame, no motion blur,
no ghosting, no semi-transparent frames, no background, no shadows, no text,
no watermark, no extra characters, no borders.
```

---

## 3. Wally (vista de espaldas, como está el juego)

Descripción fija del personaje (no cambiarla entre acciones):

```
Young Japanese boxer seen from behind, spiky black hair, muscular back,
blue sleeveless tank top, black boxing shorts with small blue trim,
blue boxing boots with white socks, red boxing gloves, wide boxing stance.
```

### 3.1 Idle (8 frames)

```
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: idle boxing stance from behind, subtle breathing and small bounce,
shoulders relaxed, gloves up, feet planted, loopable animation,
8 frames evenly spaced.
```

### 3.2 Caminar (8 frames)

```
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: walking forward in a boxing stance seen from behind, alternating steps,
knees bending naturally, gloves guarding the face, torso stays at the same
height, feet return to the same baseline, loopable walk cycle, 8 frames.
```

### 3.3 Dash (6 frames)

```
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: explosive forward dash from behind, body leaning forward, one foot
pushing off, gloves tight to the chest, no motion blur, solid frames, 6 frames.
```

### 3.4 Salto (6 frames)

```
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: jump sequence from behind: 1 crouch, 2 takeoff, 3 rising, 4 apex,
5 falling, 6 landing crouch. Same character size, no blur, 6 frames.
```

### 3.5 Golpes (6 frames cada uno)

```
Jab:
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: quick left jab from behind, 6 frames: guard, small wind-up, extension,
full extension, retract, guard.

Straight:
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: powerful right straight punch from behind, 6 frames: guard, shoulder
wind-up, hip rotation, full extension, hold, retract.

Hook:
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: wide left hook from behind, 6 frames: guard, wind-up, arc, impact,
follow-through, guard.
```

### 3.6 Bloqueo, daño y KO

```
Block (4 frames):
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: tight high guard from behind, gloves covering the head, slight crouch,
solid frames, 4 frames.

Hit (4 frames):
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: taking a punch from behind, head snapping back, torso recoiling,
SOLID OPAQUE frames, NO transparency, NO ghosting, 4 frames.

Heavy hit (4 frames):
same but stronger recoil and one knee bending, solid frames.

Knockdown (6 frames):
[PROMPT MAESTRO] + [DESCRIPCIÓN WALLY] +
Action: falling to the canvas, 6 frames: impact, stagger, knees buckle,
falling, hitting the floor, lying down. Solid frames.

Victory (6 frames):
Action: raising both gloves in victory, chest out, 6 frames, solid.

Defeat (6 frames):
Action: on one knee, head down, exhausted, 6 frames, solid.
```

---

## 4. Kirikú (de frente, como está el juego)

Descripción fija:

```
Japanese rival boxer seen from the front, messy black hair, intense stare,
bare muscular torso, red boxing gloves, black boxing shorts with white trim,
black boxing boots, wide stance.
```

Usa el mismo prompt maestro y cambia la acción:

```
idle (8): front-facing boxing stance, subtle bounce and breathing, loopable.
walk (8): walking forward in guard, front view, alternating steps, loopable.
dash (6): explosive forward dash, front view, body leaning, solid frames.
jump (6): crouch, takeoff, rise, apex, fall, landing.
jab (6): quick left jab, front view.
straight (6): powerful right straight, front view.
hook (6): wide left hook, front view.
block (4): high guard, front view.
hit (4): head snapping back, SOLID OPAQUE frames, no ghosting.
hit_heavy (4): stronger recoil, solid.
knockdown (6): falling and lying on the canvas, solid.
victory (6): raising gloves, front view.
defeat (6): on one knee, front view.
```

---

## 5. Variante lateral del MISMO ring (solo para el combate)

El `ring_bg.png` original se conserva intacto para la historia.
Para el combate necesitamos **el mismo lugar visto desde el costado**:

```
Use the attached ring photo as a strict visual reference.
Same underground boxing arena, same crowd, same hanging spotlights,
same red and blue ring ropes, same worn canvas floor, same palette,
same mood and lighting. Change ONLY the camera angle: a ringside
side view for a 2D fighting game, so two fighters can stand on the
canvas facing each other in profile. 16:9 composition,
3840 x 2160 px (or 1920 x 1080), NO characters, NO text, NO watermark,
no new objects, no different arena.
```

> 3840 × 1080 es 32:9, no 16:9. Usa 16:9 para cámara fija.
> Solo pide 3840 × 1080 (32:9) si quieres que la cámara recorra el ring.

Es el mismo ring de la historia, solo que la cámara se pone al costado
durante la pelea. Con esa imagen puedo:

1. Poner la composición lateral (Wally izquierda, Kirikú derecha).
2. Mantener la cámara fija dentro del ring.
3. Si la quieres más ancha, hacer que la cámara se desplace sin salirse.

---

## 6. Cómo me los entregas

1. Un PNG por acción (fila horizontal, 512×512 por frame).
2. Sin fondo, sin sombra, sin texto.
3. Todos los frames del mismo tamaño y con los pies a la misma altura.
4. Dime el nombre del archivo y listo: yo lo recorto, ajusto el PPU, el pivote
   y lo conecto al Animator sin deformar nada.

---

## 7. Versión de perfil (look KOF XV, como la referencia)

El juego actual está compuesto con Wally de espaldas y Kirikú de frente.
Para que se vea como la referencia de KOF XV, **los dos tienen que estar de perfil**,
mirándose, sobre un escenario ancho.

### Prompt maestro de perfil

```
2D hand-painted anime fighting game sprite sheet, strict side view profile,
character facing RIGHT, King of Fighters XV character-select render style,
thick ink outlines, cel shading, dramatic rim light, transparent background,
single horizontal row of EXACTLY 8 frames, each frame 512x512, full body,
character centered, feet on the same baseline, identical design and colors
in every frame, no motion blur, no ghosting, no semi-transparent frames,
no background, no shadows, no text, no watermark.
```

Para Kirikú cambia `facing RIGHT` por `facing LEFT`.

### Wally de perfil

```
Young Japanese boxer in profile facing right, spiky black hair, blue sleeveless
tank top, black boxing shorts with blue trim, blue boxing boots, red gloves,
muscular, wide boxing stance, side view.
```

### Kirikú de perfil

```
Japanese rival boxer in profile facing left, messy black hair, bare muscular
torso, red boxing gloves, black boxing shorts with white trim, black boots,
intense expression, side view.
```

### Acciones que necesitamos primero (en este orden)

1. `idle` (8 frames)
2. `walk` (8 frames)
3. `jab` (6 frames)
4. `straight` (6 frames)
5. `hook` (6 frames)
6. `block` (4 frames)

Con esas seis ya se puede cambiar la composición a lateral y se nota el salto
de calidad. Después: dash, salto, daño, KO, victoria y derrota.

### Qué haré yo cuando las tengas

- Voltear la composición: Wally a la izquierda mirando a la derecha,
  Kirikú a la derecha mirando a la izquierda.
- Ajustar el pivote y el PPU sin deformar.
- Rehacer el Animator con las acciones nuevas.
- Añadir retratos al HUD con las caras de perfil.
- Cámara con desplazamiento sobre el escenario ancho.

---

## 8. Atajo si el generador no respeta los 8 frames

Genera **un frame por imagen** con el mismo prompt y la indicación
`single frame, frame N of 8`. Sale más caro en tiempo, pero la consistencia
es mucho mayor y el resultado se ve más profesional.
