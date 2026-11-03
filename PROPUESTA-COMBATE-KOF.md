# PROPUESTA — Combate estilo KOF 2000

> Documento de trabajo. No se ha modificado ningún script, escena ni sprite para escribirlo.
> Autor de la propuesta: asistente. Revisión: pendiente de aprobación del autor del proyecto.

---

## 0. Resumen en cinco líneas

1. Una sola escena de combate, un solo escenario, todos los rivales ahí.
2. Se usan **exactamente tus sprites**, sin repintar, sin reemplazar, sin generar arte nuevo.
3. El combate pasa de "por turnos con esperas" a **KOF real**: movimiento lateral, dash, salto, golpes de 0.2–0.4 s y cancelaciones.
4. Se elimina todo lo que reconstruye la escena por su cuenta (la causa de que el fondo y los objetos "se borren y vuelvan").
5. Nada avanza sin tu OK; cada fase es un entregable que puedes probar y explicar.

---

## 1. Qué se conserva (no se toca)

- Tus sprites de `Assets/Art/Personajes/Wally_Nuevos/`, `Wally/`, `Kiriku/` y `Rival/`.
- El escenario `ring_bg.png`.
- La historia, el menú, el splash y el guardado.
- El HUD con estética KOF (barras, timer, rondas) — se reutiliza, no se rehace.
- Los nombres de tus animaciones y la estructura de carpetas.

**Regla:** si un sprite no existe, no se inventa: se adapta una pose existente o se deja el hueco documentado para que lo dibujes.

---

## 2. Diagnóstico: por qué hoy se siente "pausado" y "raro"

Esto es lo que encontré midiendo el proyecto, no opinión:

| Problema | Evidencia | Efecto que ves |
|---|---|---|
| Los golpes se reproducen al 20 % de velocidad | `WallyJab`, `WallyStraight`, `WallyHook` tienen `m_Speed: 0.2` en el Animator | El puñetazo tarda 5 veces más que la lógica → "congelado" |
| Idle a 8 fps con el último frame repetido | `Wally_Idle.anim`: `m_SampleRate: 8`, frames cada 0.125 s, loop de 0.75 s | Balanceo lento y tosco |
| Tiempos muertos entre acciones | `telegraphDuration` 0.6 s + `minCooldown` 1.0–2.5 s + `windup` | El rival "piensa" más que pelea |
| Faltan clips de Wally | Solo existen 5: Idle, Straight, Block, Hit, Knockdown | Jab, Hook, Duck, Weave, Backstep, Victory y Defeat no cambian nada al pulsarlos |
| No hay movimiento | `PlayerInputHandler` solo lee golpes y esquives | Los personajes están clavados = sensación de demo, no de pelea |
| Frames de Wally de tamaños distintos | idle 313×390 · straight 421×687 · hit 360×566 · hit2 209×354 · block 363×457 · block2 406×672 | Al cambiar de animación el personaje cambia de tamaño y "salta" |
| Tres dueños del fondo | `BackgroundRing` en escena (sprite inválido) + `VisualJuice` lo recrea + `bg_menu` desactivado | El escenario desaparece y reaparece |
| Fondo duplicado e importado mal | `bg_menu.png` y `ring_bg.png` son **el mismo archivo byte a byte**; `ring_bg` está en `Multiple` | Fondo cortado o invisible |
| Posiciones hardcodeadas en 5 sitios | `FightManager`, `FixKOF`, `MontarPeleaKiriku`, `HUDController`, `VisualJuice` | Lo que ajustas a mano se pierde al dar Play |

Todo esto es arreglable sin tocar tu arte.

---

## 3. Cómo se va a sentir (números concretos, 60 fps)

Referencia: KOF 2000. Todo el combate se cuenta **en frames a 60 fps**, no en `WaitForSeconds`.

### Movimiento

| Acción | Valor | Comentario |
|---|---|---|
| Caminar adelante | 2.4 unidades/s | cruza la pantalla en ~6 s |
| Caminar atrás | 1.9 unidades/s | retroceder es más lento, como en KOF |
| Dash (doble toque) | 7.0 u/s durante 12 frames (1.4 u) | recuperación 6 frames |
| Salto | 42 frames en el aire, altura 2.2 u | control horizontal 70 % |
| Agacharse | instantáneo, hitbox 35 % más baja | reutiliza `duck` |
| Empuje entre cuerpos | 0.9 u de separación mínima | nadie se atraviesa |

### Golpes

| Golpe | Startup | Activo | Recovery | Total | Hitstun | Blockstun | Hitstop | Daño |
|---|---|---|---|---|---|---|---|---|
| Jab (J) | 3 f | 2 f | 7 f | **0.20 s** | 12 f | 6 f | 3 f | 4 |
| Recto (K) | 5 f | 3 f | 12 f | **0.33 s** | 16 f | 8 f | 4 f | 8 |
| Gancho (L) | 7 f | 3 f | 15 f | **0.42 s** | 18 f | 9 f | 5 f | 12 |

- **Cancelación:** un golpe que impacta puede encadenarse al siguiente durante los primeros 4 frames del recovery. Jab → Recto → Gancho.
- **Input buffer:** 6 frames. Si pulsas 100 ms antes de poder actuar, la orden no se pierde.
- **Hitstop:** 3–5 frames congelados en el impacto. Es lo que da "peso" sin sentirse lento.
- **Bloqueo:** mantener la dirección **contraria al rival** (como KOF). Alternativa accesible: Espacio, configurable.
- **Botones:** `A/D` mover · `W` saltar · `S` agacharse · doble `A/D` dash · `J/K/L` golpes.

### IA

| Perfil | Reacción | Decisión cada | Agresividad |
|---|---|---|---|
| Rival común | 12–16 f (0.20–0.27 s) | 0.25–0.45 s | 45 % |
| Kirikú | 8–11 f (0.13–0.18 s) | 0.15–0.30 s | 75 % |

La IA **se mueve**: se acerca, se aleja, hace poke, bloquea y castiga. No espera quieta 2 segundos.

### Rondas

99 s por round (como KOF), mejor de 3, cartel `ROUND 1 → FIGHT! → K.O.`.

---

## 4. Uso de tus sprites (respetando qué es cada uno)

### Wally — sprites existentes

| Estado del juego | Sprite que se usa | Notas |
|---|---|---|
| Idle | `Wally_Nuevos/wally_idle` (5 f) | a 12 fps = 0.42 s por ciclo |
| Caminar | `wally_idle` + inclinación 3° y rebote de 2 px | **provisional**: no hay frames de caminar |
| Jab | `Wally/wally_jab` (5 f) | arte antiguo; si desentona, se usa `wally_straight` frames 1–2 |
| Recto | `Wally_Nuevos/wally_straight` (5 f) | |
| Gancho | `Wally/wally_hook` (5 f) | |
| Bloqueo alto | `Wally_Nuevos/wally_block` | |
| Bloqueo bajo | `Wally_Nuevos/wally_block2` | |
| Daño leve / fuerte | `wally_hit` / `wally_hit2` | golpe pesado usa el 2 |
| Agachado | `Wally/wally_duck` | |
| Esquive lateral | `wally_weaveleft` / `wally_weaveright` | con 6 frames de invulnerabilidad |
| Dash atrás | `wally_backstep` | |
| Derribo | `wally_knockdown` | |
| Victoria / Derrota | `wally_victory` / `wally_defeat` | |
| **OMITIDO** | `Wally_Nuevos/wally_gameover_screen.png` | se ve cortado en tiras (mosaico). Confirmar si es el que quieres fuera |

### Kirikú — sprites existentes

Todos son homogéneos: **274×353 px, 5 frames**. Eso es una ventaja enorme.

| Estado | Sprite |
|---|---|
| Idle | `kiriku_idle` (o `idle2`/`idle3` como variantes) |
| Jab / Recto | `kiriku_jab` / `kiriku_attack` |
| Gancho | `kiriku_hook` |
| Bloqueo | `kiriku_block` |
| Daño | `kiriku_hit` / `kiriku_hit2` |
| Esquive | `kiriku_weave` |
| Derribo | `kiriku_knockdown` |
| Aviso previo (opcional) | `kiriku_telegraph` |
| Victoria | `rival_victory` |

### Lo que falta (y cómo se resuelve)

| Falta | Solución inmediata | Solución ideal (para ti, opcional) |
|---|---|---|
| Caminar (ambos) | idle + inclinación/rebote | 4–6 frames de perfil al mismo tamaño |
| Salto (ambos) | idle desplazado en arco | 3 frames: subida, aire, bajada |
| Agachado Kirikú | `kiriku_weave` | 1 frame agachado |

**No se deforma nada.** Para que Wally no cambie de tamaño al atacar, se recortan las hojas a un lienzo común y se alinean los pies, **sin reescalar el dibujo**. Los PNG originales quedan intactos; la versión normalizada se guarda aparte en `Assets/Sprites/KOF/`.

---

## 5. Escenario único y cámara

- **Una escena** (`dia 1` renombrada a `03_Fight` si te parece), reutilizada para Rival y Kirikú. El rival se elige por datos (`FighterData` + `AIProfile`), no duplicando escenas.
- **Un solo fondo**: `ring_bg.png`, importado como **Single**, PPU 100, sin compresión. Se eliminan `bg_menu` (duplicado) y la creación por código.
- `ring_bg` mide **1254×1254** (cuadrado). En 16:9 hay que recortar arriba/abajo. Se ancla la línea del suelo a la altura de los pies. Si exportas una versión 1920×1080, queda perfecto.
- **Cámara fija**, ortográfica, tamaño 5 → 17.78 × 10 unidades. Ambos peleadores siempre visibles.
- Límites del ring: `x ∈ [-7.5, +7.5]`. Nadie sale del cuadrilátero.
- Los peleadores dejan de ser hijos de la cámara. Van sueltos en la escena, con posiciones que salen de un único `ArenaConfig`.

---

## 6. Arquitectura (6 scripts, todos explicables)

| Script | Responsabilidad | Líneas aprox. |
|---|---|---|
| `FighterMotor` | caminar, dash, salto, gravedad, límites, orientación | 120 |
| `FighterStateMachine` | estados y tiempos en frames (idle, walk, attack, block, hitstun, KO) | 140 |
| `CombatResolver` | un único sitio donde se decide si un golpe impacta, bloquea o falla | 90 |
| `RivalAI` | FSM de aproximación, poke, bloqueo y castigo | 120 |
| `PlayerInputHandler` | buffer, doble toque, dirección de bloqueo | 90 |
| `ArenaConfig` (ScriptableObject) | suelo, límites, puntos de spawn, escala de cada peleador | 30 |

Sin interfaces, sin Event Bus, sin Factory. Se mantienen los 3 patrones del documento original.

**Detección de impacto por distancia y estado**, no por colliders:
`|x_atacante − x_objetivo| ≤ alcance` **y** el objetivo no está invulnerable **y** está en el lado correcto.
Es la misma idea que ya tienes documentada, solo que ahora con alcance real en unidades.

---

## 7. Fases, entregables y criterio de aceptación

| Fase | Trabajo | Entregable que puedes probar | Días |
|---|---|---|---|
| **0. Orden** | desactivar scripts que reconstruyen la escena; un solo fondo; referencias válidas | El escenario se queda quieto al dar Play | 0.5 |
| **1. Sprites y velocidad** | lienzo común sin deformar; clips a 12 fps; `m_Speed` a 1 | Los golpes duran lo que dice la tabla, Wally no cambia de tamaño | 0.5 |
| **2. Movimiento KOF** | caminar, dash, salto, agacharse, empuje, límites | Te mueves por el ring y ambos quedan en pantalla | 2 |
| **3. Combate real** | startup/active/recovery, hitstun, blockstun, hitstop, cancelaciones | Puedes hacer jab → recto → gancho y se siente contundente | 2.5 |
| **4. IA KOF** | acercarse, alejarse, poke, bloquear, castigar; 2 perfiles | El rival te presiona y retrocede, no se queda plantado | 2 |
| **5. Escenario, HUD y rondas** | fondo único, timer 99, gemas, anunciador | `ROUND 1 → FIGHT → K.O.` completo | 1 |
| **6. Prueba de feel** | checklist de aceptación y ajuste fino | Pasa la checklist de abajo | 1 |
| **7. Documentación** | actualizar los `.md` y guion de sustentación | Puedes explicar cada sistema | 1 |

Total: **~10 días de trabajo**, dentro de las 7 semanas.

---

## 8. Qué se desactiva (no se borra sin tu permiso)

Scripts que editan la escena por su cuenta y son la causa de los cambios "fantasma":

`FixKOF`, `MontarPeleaKiriku`, `CrearHUDKOF`, `RepararEscena`, `DiagEscena`, `AbrirEscenas`, `ConfigurarSprites`, `LimpiarEscena`, `ConectarEscena`, `SetupBuildYJugar` y los `Fix*.cs` de `Scripts/Utils`.

Propuesta: moverlos a `Tools/legacy/` (fuera de `Assets/`, Unity deja de compilarlos). Si algún día los necesitas, siguen ahí.

También se revierte la regla 6 del `CLAUDE.md` a *"no editar `.unity`, `.prefab`, `.meta` a mano"* y se añade: *"prohibido crear scripts que reconstruyan la escena; usar prefabs"*.

---

## 9. Fuera de esta propuesta

- Modo versus, selección de personaje, más rivales.
- Uppercut, especiales, supers, barras de poder.
- Cámara con seguimiento, escenarios múltiples, parallax.
- Multijugador.

---

## 10. Checklist de aceptación ("se siente KOF")

- [ ] 60 fps estables en el combate.
- [ ] `A/D` responde en el mismo frame en que pulsas.
- [ ] Doble toque hace dash de 1.4 u en 0.18 s.
- [ ] Jab completo en 0.20 s; recto en 0.33 s; gancho en 0.42 s.
- [ ] Se puede encadenar jab → recto → gancho si el primero impacta.
- [ ] El bloqueo reduce daño y empuja hacia atrás.
- [ ] Hitstop de 3–5 frames perceptible.
- [ ] La IA ataca cada 0.4–1.0 s, no cada 2.5 s.
- [ ] El rival camina, se aleja y bloquea.
- [ ] Wally no cambia de tamaño entre animaciones.
- [ ] El fondo nunca desaparece ni se duplica.
- [ ] Round completo: `ROUND 1 → FIGHT → K.O. → resultado`.

---

## 11. Riesgos y cómo se mitigan

| Riesgo | Mitigación |
|---|---|
| Tus sprites no tienen frames de caminar y el movimiento se ve "deslizado" | inclinación + rebote + polvo; y lista exacta de frames a dibujar |
| Wally está dibujado de espaldas y Kirikú de frente | se mantiene la composición actual (Wally abajo-izquierda, Kirikú al fondo-derecha); es un híbrido arcade coherente con tu arte |
| Cambios de escena mientras Unity está abierto | todo cambio de escena lo haces tú en el editor; yo entrego instrucciones y scripts, no toco `.unity` |
| Falta de tiempo | fases 0–3 ya dan una pelea jugable; 4–7 son pulido |

---

## 12. Lo que necesito de ti para arrancar

1. **OK a la propuesta** (o cambios concretos).
2. **Confirmar el sprite omitido**: creo que es `wally_gameover_screen.png` (se ve en tiras). Si es otro, dímelo.
3. **Decidir el bloqueo**: dirección contraria (KOF real) o Espacio (más fácil).
4. **Permiso para la Fase 0** (desactivar scripts y dejar un solo fondo). No se borra nada.

---

## Anexo A — Fase 0 en el editor (lo haces tú, 3 minutos)

Ya está hecho en código: `VisualJuice` no crea ni mueve el fondo, `HUDController` no reescribe el layout y `FightManager` no mueve a los peleadores. Falta dejar el escenario bien en Unity.

**Paso 1 — Importar el fondo como una sola imagen**
1. En la ventana *Project*, ve a `Assets/Art/Backgrounds/Ring` y haz clic en `ring_bg`.
2. En el *Inspector*:
   - **Texture Type:** `Sprite (2D and UI)`
   - **Sprite Mode:** `Single`
   - **Pixels Per Unit:** `100`
   - **Mesh Type:** `Full Rect`
   - **Filter Mode:** `Bilinear`
   - **Compression:** `None`
3. Pulsa **Apply**.

**Paso 2 — Limpiar el fondo duplicado**
1. Abre la escena `Assets/dia 1.unity`.
2. En la *Hierarchy*, selecciona `bg_menu` y **desactívalo** (checkbox del Inspector). Es la misma imagen que `ring_bg`, duplicada.

**Paso 3 — Colocar el escenario**
1. Selecciona `BackgroundRing`.
2. En el *Sprite Renderer*: arrastra `ring_bg` al campo **Sprite**.
3. En *Transform*:
   - **Position:** `X 0` · `Y 0.5` · `Z 10`
   - **Scale:** `X 1.418` · `Y 1.418` · `Z 1`
4. En *Sprite Renderer* → **Order in Layer:** `-10`.

> 1.418 es la escala que hace que la imagen cuadrada (1254×1254) cubra el ancho de la cámara 16:9. Recorta las luces del techo, no el suelo.

**Paso 4 — Comprobar peleadores**
1. `Wally`: Position `(-2, -3, 10)`, Scale `(3, 3, 1)`, Order in Layer `2`.
2. `Rival`: Position `(2, -3, 10)`, Scale `(-3, 3, 1)`, Order in Layer `1`.

**Paso 5 — Guardar y probar**
1. `Ctrl + S`.
2. Play.
3. **Checkpoint:** el fondo cubre toda la pantalla, no parpadea, no se duplica, y los peleadores están uno frente al otro con los pies sobre la lona.
4. Sal de Play y avísame.

---

## Anexo B — Decisiones cerradas

- **Bloqueo:** mantener la dirección contraria al rival (KOF real). Espacio queda como alternativa configurable.
- **Sprite omitido:** `Wally_Nuevos/wally_gameover_screen.png`.
- **Arte:** no se repinta, no se reescala el dibujo, no se reemplaza. Solo se re-recorta y se alinea.
