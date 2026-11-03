# LISTA COMPLETA DE SPRITES — ONE PUNCH DELUXE

> Objetivo: movimiento natural, estilo KOF XV, composición lateral.
> Cada hoja se pide con la misma fórmula:
>
> **ESTILO + PERSONAJE + ACCIÓN + TÉCNICA**

---

## 1. Reglas técnicas (no negociables)

| Dato | Valor |
|---|---|
| Tamaño por frame | **512 × 512 px** |
| Frames por fila | 8, 6 o 4 según la acción |
| Fondo | **transparente** |
| Altura del personaje | **≈ 430 px** en todos los frames |
| Línea de pies | **y = 470** en todos los frames |
| Personaje | centrado, cuerpo completo, sin recortes |
| Estilo | cel shading limpio, tinta, look KOF XV |
| Prohibido | motion blur, estelas, frames translúcidos, texto, marca de agua, sombras, fondo |

**Referencia de estilo:** usa como imagen de referencia la hoja de Wally
que ya generaste (`Eight-Frame Anime Boxer Sprite Sheet.png`). Todas las
hojas nuevas deben parecer del mismo juego.

**Orientación:**
- Wally: **de perfil mirando a la DERECHA**.
- Kirikú: **de perfil mirando a la IZQUIERDA**.

> Si el generador te da a Wally mirando a la izquierda, no pasa nada:
> lo espejo yo. Pero lo ideal es pedirlo mirando a la derecha.

---

## 2. Bloque de ESTILO (cópialo en todos los prompts)

```
2D hand-painted anime fighting game sprite sheet, cel shading, thick ink
outlines, King of Fighters XV render style, dramatic rim light, full body,
transparent background, no motion blur, no ghosting, no semi-transparent
frames, no shadows, no text, no watermark, no background.
```

## 3. Bloque de TÉCNICA (cópialo en todos los prompts)

```
Single horizontal row of EXACTLY [N] frames, each frame 512x512,
character centered, same scale and proportions in every frame,
feet aligned at y = 470, character height around 430 px,
solid opaque frames, no extra characters, no cropping.
```

## 4. WALLY (perfil derecha)

```
Young Japanese boxer in strict side profile facing RIGHT, spiky black hair,
blue sleeveless tank top, black boxing shorts with blue trim, blue boxing
boots with white socks, red boxing gloves, muscular, wide boxing stance.
```

## 5. KIRIKÚ (perfil izquierda)

```
Japanese rival boxer in strict side profile facing LEFT, messy black hair,
bare muscular torso, red boxing gloves, black boxing shorts with white trim,
black boxing boots, intense expression, wide boxing stance.
```

---

## 6. Acciones — FASE 1 (con esto se cambia a lateral)

| # | Archivo | Frames | Acción (frase para el prompt) |
|---|---|---|---|
| 1 | `wally_walk.png` | 8 | `Walk cycle: alternating steps, guard up, loopable, feet back on the same baseline.` |
| 2 | `wally_jab.png` | 6 | `Quick left jab: guard, small wind-up, extension, full extension, retract, guard.` |
| 3 | `wally_straight.png` | 6 | `Powerful right straight: guard, shoulder wind-up, hip rotation, full extension, hold, retract.` |
| 4 | `wally_hook.png` | 6 | `Wide left hook: guard, wind-up, arc, impact, follow-through, guard.` |
| 5 | `wally_block.png` | 4 | `Tight high guard: gloves covering the face, slight crouch, holding the pose.` |
| 6 | `wally_hit.png` | 4 | `Taking a punch: head snapping back, torso recoil, SOLID opaque frames, no ghosting.` |
| 7 | `wally_knockdown.png` | 6 | `Knockdown: impact, stagger, knees buckle, falling, hitting the canvas, lying down.` |
| 8 | `kiriku_idle.png` | 8 | `Idle boxing stance: subtle breathing and bounce, guard up, loopable.` |
| 9 | `kiriku_walk.png` | 8 | `Walk cycle: alternating steps, guard up, loopable.` |
| 10 | `kiriku_jab.png` | 6 | `Quick left jab, front profile.` |
| 11 | `kiriku_straight.png` | 6 | `Powerful right straight.` |
| 12 | `kiriku_hook.png` | 6 | `Wide left hook.` |
| 13 | `kiriku_block.png` | 4 | `Tight high guard, holding.` |
| 14 | `kiriku_hit.png` | 4 | `Taking a punch, head snap, SOLID opaque frames.` |
| 15 | `kiriku_knockdown.png` | 6 | `Knockdown and lying on the canvas.` |

> **Wally idle ya está hecho.** No hace falta pedirlo.

---

## 7. Acciones — FASE 2 (movimiento natural completo)

| # | Archivo | Frames | Acción |
|---|---|---|---|
| 16 | `wally_jump.png` | 6 | `Jump: crouch, takeoff, rising, apex, falling, landing crouch.` |
| 17 | `wally_dash.png` | 6 | `Forward dash: crouch, push off, burst, glide, brake, recover.` |
| 18 | `wally_crouch.png` | 4 | `Crouch down, hold low guard, stand back up.` |
| 19 | `wally_getup.png` | 6 | `Getting up from the canvas: hands push, knee, stand, guard.` |
| 20 | `wally_victory.png` | 6 | `Victory: raising both gloves, chest out, confident.` |
| 21 | `wally_defeat.png` | 6 | `Defeat: on one knee, head down, exhausted.` |
| 22 | `kiriku_jump.png` | 6 | same as jump |
| 23 | `kiriku_dash.png` | 6 | same as dash |
| 24 | `kiriku_crouch.png` | 4 | same as crouch |
| 25 | `kiriku_getup.png` | 6 | same as getup |
| 26 | `kiriku_victory.png` | 6 | `Victory: raising gloves, intense stare.` |
| 27 | `kiriku_defeat.png` | 6 | `Defeat: on one knee, head down.` |

---

## 8. Acciones — FASE 3 (nivel "súper juego")

| # | Archivo | Frames | Acción |
|---|---|---|---|
| 28 | `wally_intro.png` | 6 | `Intro/taunt: touching gloves, shadow boxing, ready stance.` |
| 29 | `wally_hit_heavy.png` | 4 | `Heavy hit: strong recoil, one knee bends, solid frames.` |
| 30 | `wally_special.png` | 10 | `Special punch: charge, step in, explosive straight, impact, recovery.` |
| 31 | `wally_portrait.png` | 1 | `Bust portrait, facing camera, intense expression, transparent background.` |
| 32 | `kiriku_intro.png` | 6 | `Intro/taunt: cracking knuckles, pointing at the rival.` |
| 33 | `kiriku_hit_heavy.png` | 4 | `Heavy hit, strong recoil, solid.` |
| 34 | `kiriku_special.png` | 10 | `Special punch: charge, lunge, explosive hook, impact, recovery.` |
| 35 | `kiriku_portrait.png` | 1 | `Bust portrait, facing camera, intense expression, transparent.` |

---

## 9. Ejemplo de prompt completo (Wally walk)

```
2D hand-painted anime fighting game sprite sheet, cel shading, thick ink
outlines, King of Fighters XV render style, dramatic rim light, full body,
transparent background, no motion blur, no ghosting, no semi-transparent
frames, no shadows, no text, no watermark, no background.

Young Japanese boxer in strict side profile facing RIGHT, spiky black hair,
blue sleeveless tank top, black boxing shorts with blue trim, blue boxing
boots with white socks, red boxing gloves, muscular, wide boxing stance.

Walk cycle: alternating steps, guard up, loopable, feet back on the
same baseline.

Single horizontal row of EXACTLY 8 frames, each frame 512x512,
character centered, same scale and proportions in every frame,
feet aligned at y = 470, character height around 430 px,
solid opaque frames, no extra characters, no cropping.
```

## 10. Negative prompt (si el generador lo acepta)

```
multiple characters, text, watermark, background, floor, shadow,
motion blur, ghosting, transparent body, cropped limbs, different sizes,
different outfits, extra arms, extra legs, duplicate frames
```

---

## 11. Cómo entregarlos

- Un PNG por acción, fila horizontal.
- Nombres exactos de la tabla (si salen distintos, los renombro).
- Déjalos en `C:\Users\Lenovo\Documents\OnePunchDeluxe\Art_Incoming`
  o arrástralos al chat.
- **Por lotes de 3–4**, no los 35 de golpe: así los integro y ajusto
  antes de que sigas gastando generaciones.

### Orden recomendado de lotes

1. `kiriku_idle`, `wally_walk`, `kiriku_walk` → **cambio lateral**
2. `wally_jab`, `wally_straight`, `wally_hook`, `kiriku_jab` → combate
3. `wally_block`, `wally_hit`, `wally_knockdown` → defensa y KO
4. resto de Fase 1 de Kirikú
5. Fase 2
6. Fase 3
