# Instrucciones del Editor — ONE PUNCH DELUXE

> Todo lo que debes hacer en Unity para que los cambios del código funcionen.
> Sigue las instrucciones en orden. Cada sección tiene un checkpoint al final.

---

## TAREA 1 — Conectar las nuevas referencias de FightManager

**Escena:** `dia 1.unity`

El `FightManager` tiene 2 campos nuevos que hay que conectar:

1. Abre la escena `dia 1`.
2. En la **Jerarquía** busca el objeto `FightManager`.
3. En el **Inspector** verás los nuevos campos:
   - **Rival Animator** → arrastra el objeto del Rival al campo
   - **Player Input** → arrastra el objeto de Wally (el que tiene `PlayerInputHandler`) al campo
4. Haz clic en **Save** (Ctrl+S).

✅ **Checkpoint:** En el Inspector de FightManager los dos campos nuevos tienen objetos asignados (no dicen "None").

---

## TAREA 2 — Agregar botones al panel de resultado

**Escena:** `dia 1.unity`

El `HUDController` ahora tiene dos botones: Reintentar y Volver al Menú. Hay que crearlos en el Canvas.

1. En la Jerarquía, expande el Canvas → busca el objeto `ResultPanel` (el panel que aparece cuando termina la pelea).
2. Dentro de `ResultPanel`, crea dos botones:
   - Clic derecho sobre `ResultPanel` → **UI → Button - TextMeshPro**
   - Nómbralo `BotonReintentar`
   - En el texto del botón escribe: `REINTENTAR`
3. Repite para crear otro botón:
   - Nómbralo `BotonMenu`
   - Texto: `MENÚ`
4. Posiciona los botones debajo del texto de resultado (uno al lado del otro).
5. Selecciona el objeto `HUD` (el que tiene `HUDController`) en la Jerarquía.
6. En el **Inspector** de `HUDController`, arrastra:
   - `BotonReintentar` al campo **Boton Reintentar**
   - `BotonMenu` al campo **Boton Menu**
7. Guarda la escena (Ctrl+S).

✅ **Checkpoint:** En el Inspector del HUD los campos "Boton Reintentar" y "Boton Menu" tienen objetos asignados.

---

## TAREA 3 — Animaciones del Rival (Hit, Victory, Defeat)

**Qué falta:** el Rival solo tiene animaciones Idle y Attack. Necesita Hit, Victory y Defeat.

Los sprites ya existen en `Assets/Art/Personajes/Rival/`.

### 3.1 Crear la animación RivalHit

1. En el **Project** (ventana de assets), navega a `Assets/es/RivalComun/`.
2. Haz clic derecho → **Create → Animation** → nómbrala `RivalHit`.
3. Abre el **Animation** window (Window → Animation → Animation).
4. Selecciona el objeto **Rival** en la Jerarquía.
5. En el panel Animation, selecciona la animación `RivalHit` del menú desplegable.
6. Haz clic en **Add Property → Sprite Renderer → Sprite**.
7. Arrastra el sprite `rival_hit.png` al frame 0.
8. En el Inspector de la animación, asegúrate de que **Loop Time** está **desactivado** (es un golpe recibido, no loops).

### 3.2 Repetir para RivalVictory y RivalDefeat

- Repite el proceso para `RivalVictory` usando `rival_victory.png`.
- Repite para `RivalDefeat` usando `rival_defeat.png`.

### 3.3 Agregar los estados al Animator del Rival

1. Abre el **Animator** window (Window → Animation → Animator).
2. Selecciona el objeto **Rival** en la Jerarquía — verás el `RivalAnimator`.
3. En el panel izquierdo **Parameters**, agrega 3 triggers nuevos (botón `+`):
   - `Hit`
   - `Victory`
   - `Defeat`
4. Arrastra las 3 animaciones nuevas al Animator (desde el Project).
5. Por cada animación nueva, crea una transición desde **Any State**:
   - Clic derecho en **Any State** → **Make Transition** → clic en `RivalHit`
   - En la transición, agrega condición: trigger `Hit`
   - Desactiva "Has Exit Time"
   - Repite para Victory y Defeat
6. Desde `RivalHit` → `RivalIdle`: crear transición con **Has Exit Time = true**, Exit Time = 1.0 (vuelve a idle al terminar)
7. Guarda (Ctrl+S).

✅ **Checkpoint:** El Animator del Rival tiene 5 estados (Idle, Attack, Hit, Victory, Defeat) y 5 triggers (Jab, Straight, Hook, Hit, Victory, Defeat).

---

## TAREA 4 — Agregar música de historia al AudioLibrary

1. En el **Project**, navega a `Assets/Audio/SFX/` y busca el asset `AudioLibrary`.
2. Selecciónalo y en el **Inspector** verás el nuevo campo **Musica Historia**.
3. Si tienes un archivo de música para la historia en `Assets/Audio/Music/`, arrástralo ahí.
4. Si no tienes música de historia aún, puedes dejarlo vacío — la historia funciona en silencio (no hay error).

✅ **Checkpoint:** El campo "Musica Historia" del AudioLibrary tiene un clip asignado (o está vacío intencionalmente).

---

## TAREA 5 — Verificar Build Settings

1. Ve a **File → Build Settings** (o Ctrl+Shift+B).
2. Verifica que las escenas estén en este orden exacto:
   ```
   0: Assets/Scenes/Scenes/splash.unity   (o donde esté tu splash)
   1: Assets/menu.unity
   2: Assets/historia.unity
   3: Assets/dia 1.unity
   ```
3. Si faltan escenas, arrástralas desde el Project a la lista.
4. Si el orden es incorrecto, arrastra para reordenar.

✅ **Checkpoint:** Hay exactamente 4 escenas en Build Settings, en el orden correcto, y la escena 0 es splash.

---

## TAREA 6 — Prueba final del flujo completo

Antes de hacer el build final, prueba el juego completo:

1. **Play en escena Splash:** debe aparecer el logo, hacer fade, y cargar el menú automáticamente.
2. **Menú Principal:** el botón "Continuar" debe estar gris (primera vez). "Nueva Partida" debe cargar la historia.
3. **Historia:** los 3 paneles deben aparecer con Ken Burns y texto. Tecla para avanzar.
4. **Combate:** la campana suena, el rival telegrafía con "!", el HUD muestra las barras.
5. **KO:** aparece el panel de resultado con dos botones. "Reintentar" recarga la pelea. "Menú" va al menú.
6. **Segunda vez en menú:** "Continuar" debe estar activo (hay progreso guardado).

✅ **Checkpoint final:** El flujo completo funciona de principio a fin sin errores en la consola.

---

## TAREA 7 — Build para PC/Windows

1. **File → Build Settings**
2. Plataforma: **PC, Mac & Linux Standalone**
3. Target Platform: **Windows**
4. Architecture: **x86_64**
5. Haz clic en **Build** (no "Build and Run").
6. Elige una carpeta de salida (por ejemplo, `Builds/Windows/`).
7. Espera a que compile. Si hay errores, aparecen en la consola.

✅ **Checkpoint:** Existe un archivo `.exe` en la carpeta de salida y el juego corre haciendo doble clic.
