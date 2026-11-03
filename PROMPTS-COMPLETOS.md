# PACK COMPLETO DE PROMPTS — ONE PUNCH DELUXE

> Formato que ya funcionó: **cuadrícula con celdas y líneas finas**.
> Cada imagen = una acción. Se corta por celdas y no se rompe nada.

---

## Fórmula

**ESTILO + PERSONAJE + ACCIÓN + TÉCNICA**

---

## 1. ESTILO (cópialo siempre)

```
2D hand-painted anime fighting game sprite sheet, cel shading, thick ink
outlines, King of Fighters XV render style, dramatic rim light, full body,
transparent background, no motion blur, no ghosting, no semi-transparent
frames, no shadows, no text, no watermark, no background.
```

## 2. PERSONAJE

**Wally** (todos los sheets mirando a la DERECHA):
```
Young Japanese boxer in strict side profile facing RIGHT, spiky black hair,
blue sleeveless tank top, black boxing shorts with blue trim, blue boxing
boots with white socks, red boxing gloves, muscular, wide boxing stance.
```

**Kirikú** (todos los sheets mirando a la DERECHA; yo lo volteo en el juego):
```
Japanese rival boxer in strict side profile facing RIGHT, messy black hair,
bare muscular torso, red boxing gloves, black boxing shorts with white trim,
black boxing boots, intense expression, wide boxing stance.
```

## 3. TÉCNICA (según los frames)

**4 frames (2×2, 1024×1024):**
```
Single image arranged as a 2-column by 2-row grid of 4 frames, each cell
512x512. Character centered in each cell, same scale, feet aligned, solid
opaque frames. Draw thin light-grey lines separating the cells. Leave a 30 px
transparent margin inside each cell. No arm, glove, foot or shadow may touch
or cross the cell lines. Total image 1024 x 1024 px.
```

**6 frames (3×2, 1536×1024):**
```
Single image arranged as a 3-column by 2-row grid of 6 frames, each cell
512x512. Character centered in each cell, same scale, feet aligned, solid
opaque frames. Draw thin light-grey lines separating the cells. Leave a 30 px
transparent margin inside each cell. No arm, glove, foot or shadow may touch
or cross the cell lines. Total image 1536 x 1024 px.
```

**8 frames (4×2, 2048×1024):**
```
Single image arranged as a 4-column by 2-row grid of 8 frames, each cell
512x512. Character centered in each cell, same scale, feet aligned, solid
opaque frames. Draw thin light-grey lines separating the cells. Leave a 30 px
transparent margin inside each cell. No arm, glove, foot or shadow may touch
or cross the cell lines. Total image 2048 x 1024 px.
```

**10 frames (5×2, 2560×1024):**
```
Single image arranged as a 5-column by 2-row grid of 10 frames, each cell
512x512. Character centered in each cell, same scale, feet aligned, solid
opaque frames. Draw thin light-grey lines separating the cells. Leave a 30 px
transparent margin inside each cell. No arm, glove, foot or shadow may touch
or cross the cell lines. Total image 2560 x 1024 px.
```

---

## 4. ACCIONES (frase para pegar)

| Archivo | Frames | Cuadrícula | Acción |
|---|---|---|---|
| `wally_walk.png` | 8 | 4×2 | `Walk cycle: contact, down, passing, up, contact opposite, down, passing, up. Feet clearly stepping, guard up, loopable.` |
| `kiriku_walk.png` | 8 | 4×2 | same as walk |
| `wally_block.png` | 4 | 2×2 | `Tight high guard: gloves covering the face, slight crouch, holding the pose.` |
| `kiriku_block.png` | 4 | 2×2 | same as block |
| `wally_hit.png` | 4 | 2×2 | `Taking a punch: impact, head snapping back, torso recoil, recover. Solid opaque frames, no ghosting.` |
| `kiriku_hit.png` | 4 | 2×2 | same as hit |
| `wally_hit_heavy.png` | 4 | 2×2 | `Heavy hit: strong recoil, one knee bends, big stagger, recover. Solid opaque frames.` |
| `kiriku_hit_heavy.png` | 4 | 2×2 | same as heavy hit |
| `wally_knockdown.png` | 6 | 3×2 | `Knockdown: impact, stagger, knees buckle, falling, hitting the canvas, lying down.` |
| `kiriku_knockdown.png` | 6 | 3×2 | same as knockdown |
| `wally_getup.png` | 6 | 3×2 | `Getting up from the canvas: hands push, knee, stand, guard, ready.` |
| `kiriku_getup.png` | 6 | 3×2 | same as getup |
| `wally_jump.png` | 6 | 3×2 | `Jump: crouch, takeoff, rising, apex, falling, landing crouch.` |
| `kiriku_jump.png` | 6 | 3×2 | same as jump |
| `wally_dash.png` | 6 | 3×2 | `Forward dash: crouch, push off, burst, glide, brake, recover.` |
| `kiriku_dash.png` | 6 | 3×2 | same as dash |
| `wally_crouch.png` | 4 | 2×2 | `Crouch down, hold low guard, stand back up.` |
| `kiriku_crouch.png` | 4 | 2×2 | same as crouch |
| `wally_victory.png` | 6 | 3×2 | `Victory: raising both gloves, chest out, confident.` |
| `kiriku_victory.png` | 6 | 3×2 | `Victory: raising gloves, intense stare.` |
| `wally_defeat.png` | 6 | 3×2 | `Defeat: on one knee, head down, exhausted.` |
| `kiriku_defeat.png` | 6 | 3×2 | `Defeat: on one knee, head down.` |
| `wally_intro.png` | 6 | 3×2 | `Intro: shadow boxing, touching gloves, ready stance.` |
| `kiriku_intro.png` | 6 | 3×2 | `Intro: cracking knuckles, pointing at the rival, ready stance.` |
| `wally_special.png` | 10 | 5×2 | `Super punch: charge, step in, explosive straight, impact, follow-through, recovery.` |
| `kiriku_special.png` | 10 | 5×2 | `Super punch: charge, lunge, explosive hook, impact, follow-through, recovery.` |
| `wally_portrait.png` | 1 | — | `Bust portrait facing the camera, intense expression, transparent background, 1024x1024.` |
| `kiriku_portrait.png` | 1 | — | `Bust portrait facing the camera, intense expression, transparent background, 1024x1024.` |

---

## 5. Ejemplo completo (walk de Wally)

```
2D hand-painted anime fighting game sprite sheet, cel shading, thick ink
outlines, King of Fighters XV render style, dramatic rim light, full body,
transparent background, no motion blur, no ghosting, no semi-transparent
frames, no shadows, no text, no watermark, no background.

Young Japanese boxer in strict side profile facing RIGHT, spiky black hair,
blue sleeveless tank top, black boxing shorts with blue trim, blue boxing
boots with white socks, red boxing gloves, muscular, wide boxing stance.

Walk cycle: contact, down, passing, up, contact opposite, down, passing, up.
Feet clearly stepping, guard up, loopable.

Single image arranged as a 4-column by 2-row grid of 8 frames, each cell
512x512. Character centered in each cell, same scale, feet aligned, solid
opaque frames. Draw thin light-grey lines separating the cells. Leave a 30 px
transparent margin inside each cell. No arm, glove, foot or shadow may touch
or cross the cell lines. Total image 2048 x 1024 px.
```

---

## 6. Negative prompt

```
multiple characters in one cell, text, watermark, background, floor, shadow,
motion blur, ghosting, transparent body, cropped limbs, different sizes,
different outfits, extra arms, extra legs, duplicate frames, limbs crossing
cell borders
```

---

## 7. Orden de generación (por lotes)

**Lote 1 — caminar**
1. `wally_walk` (4×2)
2. `kiriku_walk` (4×2)

**Lote 2 — defensa y KO**
3. `wally_block` (2×2)
4. `wally_hit` (2×2)
5. `wally_knockdown` (3×2)
6. `kiriku_block` (2×2)
7. `kiriku_hit` (2×2)
8. `kiriku_knockdown` (3×2)

**Lote 3 — movilidad**
9. `wally_jump` (3×2)
10. `wally_dash` (3×2)
11. `wally_crouch` (2×2)
12. `wally_getup` (3×2)
13. lo mismo para Kirikú

**Lote 4 — presentación**
14. victoria, derrota, intro de ambos
15. `wally_special` y `kiriku_special` (5×2)
16. retratos

---

## 8. Regla de oro

- **Una acción por imagen.**
- **Celdas con líneas finas** y margen de 30 px.
- **Ningún miembro puede cruzar la línea.**
- Si un frame sale cortado o con un trozo del vecino, se repite esa imagen.
- Entrégalos en `Art_Incoming` o arrástralos al chat.
