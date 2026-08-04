# ONE PUNCH DELUXE — Documentación Técnica

> Trabajo de grado · Videojuego 2D · Unity 6 LTS

---

## 1. Descripción del Juego

**ONE PUNCH DELUXE** es un videojuego de boxeo arcade 2D inspirado en la saga *Punch-Out!!* de Nintendo. El jugador controla a **Wally Dempsey**, un boxeador joven que sueña con convertirse en campeón. A través de una historia narrada en paneles de cómic, Wally enfrenta rivales de dificultad creciente hasta llegar al jefe final: **Kirikú**.

### Objetivo del jugador

Derrotar al rival por KO (reducir su vida a cero) o acumular más vida que el rival cuando se acabe el tiempo del round.

### Controles

| Tecla | Acción |
|-------|--------|
| J | Jab (golpe rápido, poco daño) |
| K | Recto / Straight (golpe medio) |
| L | Gancho / Hook (golpe lento, mayor daño) |
| A | Esquive izquierda (WeaveLeft) |
| D | Esquive derecha (WeaveRight) |
| S | Agacharse (Duck) |
| W | Paso atrás (Backstep) |
| Espacio | Bloquear (reduce daño al 25%) |

---

## 2. Flujo de Juego

```
[Splash]
    ↓  fade in/out automático (4.5 segundos)
[Menú Principal]
    ↓  "Nueva Partida" → borra progreso anterior
[Historia]
    ↓  3 paneles con Ken Burns + texto (cualquier tecla para avanzar)
[Combate — Día 1]
    ↓  KO o tiempo agotado
[Resultado — Victoria / Derrota]
    ↓  "Reintentar" o "Volver al Menú"
```

---

## 3. Arquitectura del Proyecto

```
OnePunchDeluxe/
├── Assets/
│   ├── Art/
│   │   ├── Backgrounds/Story/     # Paneles de historia (PNG)
│   │   └── Personajes/
│   │       ├── Wally/             # 14 sprites espalda
│   │       ├── Rival/             # 8 sprites frente
│   │       └── UI/                # Logo, botones, fondo
│   ├── Audio/
│   │   ├── Music/                 # Música (.ogg, streaming)
│   │   ├── SFX/                   # Efectos (.wav/.ogg, decompress on load)
│   │   └── CREDITOS.md            # Licencias de todos los assets de audio
│   ├── ScriptableObjects/
│   │   ├── Attacks/               # JabData, StraightData, HookData
│   │   ├── AIProfiles/            # RivalComunProfile (Kirikú pendiente)
│   │   └── (paneles de historia)  # Panel01, Panel02, Panel03
│   ├── Scripts/
│   │   ├── Combat/                # CombatController, FightManager, Health, Stamina
│   │   ├── Player/                # PlayerInputHandler
│   │   ├── AI/                    # RivalAI
│   │   ├── Data/                  # AttackData, AIProfile (ScriptableObjects)
│   │   ├── Save/                  # SaveManager, SaveData
│   │   ├── Audio/                 # AudioManager, AudioLibrary
│   │   ├── UI/                    # HUDController, MainMenuController, SplashController
│   │   └── Story/                 # StoryPlayer, StoryPanel
│   └── (escenas .unity)
└── Tools/
    ├── normalize_sprites.py       # Procesamiento de sprites con Pillow
    └── input/                     # Sprites originales sin procesar
```

---

## 4. Stack Tecnológico

| Herramienta | Versión / Detalle |
|-------------|-------------------|
| Motor | Unity 6 LTS — plantilla Universal 2D (URP) |
| Lenguaje | C# |
| Input | Unity Input System (paquete nuevo) |
| Interfaz | Canvas + TextMeshPro |
| Datos | ScriptableObjects |
| Persistencia | JSON con `JsonUtility` |
| Audio | AudioSource + pool de SFX |
| Control de versiones | Git + GitHub + Git LFS (para assets binarios) |
| Arte (sprites) | IA generativa (ChatGPT/DALL-E) + Python (Pillow) para procesamiento |
| Audio (SFX) | Kenney.nl (CC0) |

---

## 5. Sistemas Implementados

### 5.1 Sistema de Combate

Implementado en `CombatController.cs`. Cada golpe sigue el ciclo:

```
Windup → Active → Recovery
  ↓         ↓        ↓
preparación  impacto  vulnerable
```

**Cómo funciona:**
- El jugador presiona J/K/L → `PlayerInputHandler` llama `CombatController.TryAttack(attackData)`.
- `TryAttack` verifica stamina disponible y que no haya un ataque en curso.
- Durante la fase **Active**, se evalúa si el rival esquivó usando `EvaluarImpacto()`.
- Si el rival bloquea, el daño se reduce al 25% (`blockDamageMultiplier = 0.25`).
- **Detección de impacto sin colliders**: se compara la posición de esquive del rival contra las posiciones que ese golpe específico puede esquivar (configurado en `AttackData.dodgesThatAvoid`).

### 5.2 Sistema de Vida y Stamina

`Health.cs` y `Stamina.cs` son componentes independientes que notifican cambios mediante **eventos de C#** (`event System.Action`). El HUD se suscribe a estos eventos — nunca consulta directamente el estado.

```
Health.OnHealthChanged → HUDController actualiza barra
Health.OnDamage        → FightManager dispara animación Hit
Health.OnDeath         → FightManager termina la pelea
```

La **stamina** se recupera automáticamente con el tiempo (`regenPerSecond`). Sin stamina no se puede atacar ni bloquear.

### 5.3 Esquives (Posiciones, no Desplazamientos)

Implementado en `DodgePosition.cs` (enum) y `PlayerInputHandler.cs`.

Los esquives son **estados de posición**, no movimientos en el espacio — coherente con la vista tipo Punch-Out!! donde Wally no se mueve lateralmente. Las posiciones disponibles son:

| Enum | Tecla | Descripción |
|------|-------|-------------|
| `Neutral` | (ninguna) | Posición normal |
| `WeaveLeft` | A | Esquive izquierda |
| `WeaveRight` | D | Esquive derecha |
| `Duck` | S | Agacharse |
| `Backstep` | W | Paso atrás |

Cada `AttackData` define qué posiciones lo evitan. Ejemplo: el Jab no puede alcanzar a alguien en `WeaveLeft` o `WeaveRight`.

### 5.4 Inteligencia Artificial del Rival

Implementado en `RivalAI.cs`. Ciclo de la IA:

```
Esperar (cooldown aleatorio)
    ↓
Telegrafiar (mostrar "!" por telegraphDuration segundos)
    ↓
Atacar (llamar TryAttack con golpe aleatorio)
    ↓
(repetir)
```

El **telegraph** es el elemento de jugabilidad central: da tiempo al jugador para reaccionar. La dificultad se controla exclusivamente cambiando `AIProfile.telegraphDuration`:

- Rival Común: `0.6s` — tiempo suficiente para reaccionar
- Kirikú (jefe): `0.25s` — muy poco tiempo, requiere anticipación

Ambos rivales comparten **exactamente el mismo código** (`RivalAI.cs`). Solo cambia el `AIProfile` (ScriptableObject) asignado en el Inspector.

### 5.5 Sistema de Audio

Implementado en `AudioManager.cs` y `AudioLibrary.cs`.

**Arquitectura:**
- `AudioManager` es un **Singleton** con `DontDestroyOnLoad` — persiste entre escenas.
- Un `AudioSource` dedicado para música con **fade in/fade out** al cambiar de pista.
- **Pool de 8 AudioSources** para efectos de sonido — evita que dos golpes simultáneos se corten.
- `AudioLibrary` (ScriptableObject) centraliza todas las referencias a clips. Nada de rutas mágicas en el código.

**Por qué pool de SFX:**
Sin pool, si el jugador lanza dos golpes rápidos, el segundo interrumpe el sonido del primero. Con un pool circular de 8 fuentes, hasta 8 SFX pueden sonar simultáneamente.

### 5.6 Sistema de Guardado

Implementado en `SaveManager.cs` y `SaveData.cs`.

- `SaveData` es una clase plana `[Serializable]` — compatible con `JsonUtility`.
- El archivo se guarda en `Application.persistentDataPath` (carpeta de datos del juego en Windows).
- Datos guardados: `nivelActual` (qué rival corresponde), `victorias`, `derrotas`.
- El menú principal consulta `nivelActual > 0` para habilitar "Continuar".

```csharp
// Ruta real en Windows:
// C:\Users\<usuario>\AppData\LocalLow\<empresa>\OnePunchDeluxe\save.json
```

### 5.7 Sistema de Historia

Implementado en `StoryPlayer.cs` y `StoryPanel.cs`.

Cada panel de historia es un **ScriptableObject** (`StoryPanel`) con imagen y texto. `StoryPlayer` los recorre en orden con:

- **Efecto máquina de escribir**: `maxVisibleCharacters` de TextMeshPro se incrementa a 0.02s/letra.
- **Texto instantáneo**: si el jugador presiona una tecla mientras escribe, muestra todo de golpe.
- **Avanzar de panel**: segunda tecla después de que termina el texto.
- **Efecto Ken Burns**: la imagen hace paneo suave de lado a lado con `SmoothStep` para simular movimiento de cámara.

---

## 6. Patrones de Diseño

Se aplicaron exactamente los patrones que el problema exigía. No se introdujeron patrones adicionales para evitar complejidad innecesaria.

### ScriptableObject como Datos

`AttackData`, `AIProfile` y `StoryPanel` son ScriptableObjects. Esto permite:
- Balancear el juego cambiando valores en el Inspector **sin tocar código**.
- Reutilizar el mismo código (`RivalAI`) con distintos perfiles de datos.
- Los datos viven en el proyecto como assets, no hardcodeados.

### Máquina de Estados (FSM)

La IA del rival implementa una FSM implícita: `Idle → Telegraph → Attack → Recovery → Idle`. El jugador también tiene estados implícitos (Neutral, Attacking, Blocking, Dodging).

**Por qué FSM en juegos de pelea:** los estados de combate son discretos, mutuamente excluyentes y tienen transiciones claras. Una FSM es la representación más natural y fácil de razonar.

### Observer (Eventos de C#)

`Health` y `Stamina` exponen eventos (`event System.Action`). El HUD se suscribe para actualizar la UI. `FightManager` se suscribe para detectar KO.

**Por qué Observer:** desacopla completamente la lógica de juego de la interfaz. `Health` no sabe que existe un HUD — solo avisa que su valor cambió. Si el HUD cambia de diseño, no hay que tocar `Health`.

### Singleton

`AudioManager`, `SaveManager`. Se usa `DontDestroyOnLoad` para que persistan entre escenas. Máximo 2 singletons en el proyecto — ninguno más.

---

## 7. Pipeline de Arte

### Generación de Sprites

Los sprites se generaron con IA (ChatGPT con DALL-E) usando prompts específicos:

- **Estilo:** manga/anime cinematográfico, paleta limitada, líneas limpias
- **Wally:** vista de ESPALDA, uniforme rojo-negro, pelo oscuro
- **Rival:** vista de FRENTE, expresión agresiva
- **Fondo transparente obligatorio** para composición en Unity

### Procesamiento con Python (`Tools/normalize_sprites.py`)

Los sprites generados por IA llegan con fondos irregulares, tamaños distintos y personajes desalineados. El script de Python (con Pillow) hace:

1. **Quitar fondo**: detecta el color de fondo y lo elimina (alpha = 0)
2. **Auto-crop**: recorta los bordes transparentes
3. **Alineación por base**: centra el personaje y alinea los pies a la misma altura en todos los frames
4. **Celda 128×128**: rellena con transparencia hasta la celda uniforme
5. **Exportar spritesheet**: genera un PNG con todos los frames en fila

**Por qué la alineación es crítica:** si un frame queda 3px más arriba que otro, el personaje "salta" visualmente al cambiar de animación, aunque la animación sea técnicamente correcta.

### Configuración en Unity

| Parámetro | Valor | Razón |
|-----------|-------|-------|
| Sprite Mode | Multiple | Spritesheet con varios frames |
| Pixels Per Unit | 128 | Celda 128px = 1 unidad Unity |
| Filter Mode | Point | Pixel art nítido (sin blur) |
| Compression | None | Sin artefactos de compresión |
| Pivot | Bottom | Personajes se posan sobre el suelo |

---

## 8. Capturas de Pantalla

*(Insertar capturas al finalizar el proyecto)*

| Escena | Descripción |
|--------|-------------|
| Splash | Logo con fade in/out sobre fondo negro |
| Menú Principal | 3 botones sobre fondo del ring, logo arriba |
| Historia | Panel de cómic con texto y efecto Ken Burns |
| Combate | Wally (espalda) vs Rival (frente), HUD con barras |
| Resultado | Panel de KO con opción de Reintentar o ir al Menú |

---

## 9. Cronograma de Desarrollo

| Semana | Fechas | Hito | Estado |
|--------|--------|------|--------|
| 1 | 28 jul – 3 ago | Setup: Unity, Git, estructura de carpetas, sprites elegidos | ✅ Completo |
| 2 | 4 ago – 10 ago | Combate gris jugable: golpes, vida, KO sin arte | ✅ Completo |
| 3 | 11 ago – 17 ago | Sprites + Animator: arte manga en pantalla | ✅ Completo |
| 4 | 18 ago – 24 ago | IA con FSM: rival con telegraph y ataques propios | ✅ Completo |
| 5 | 25 ago – 31 ago | HUD, rounds: ciclo completo splash→combate→resultado | ✅ Completo |
| 6 | 1 sep – 7 sep | Audio, historia, guardado: juego completo con sonido | ✅ Completo |
| 7 | 8 sep – 14 sep | Polish, build final, documentación | En curso |

---

## 10. Créditos de Assets

### Audio

Ver archivo completo en `Assets/Audio/CREDITOS.md`.

| Asset | Fuente | Licencia |
|-------|--------|----------|
| SFX de combate (11 archivos) | Kenney.nl — Impact Sounds | CC0 (dominio público) |
| SFX de UI | Kenney.nl — UI Audio | CC0 (dominio público) |

### Arte

| Asset | Fuente | Licencia |
|-------|--------|----------|
| Sprites de Wally (14) | Generados con IA, procesados con Python | Uso educativo |
| Sprites del Rival (8) | Generados con IA, procesados con Python | Uso educativo |
| Paneles de historia (3) | Generados con IA | Uso educativo |
| Logo del juego | Generado con IA | Uso educativo |
| Fondo del ring | Generado con IA | Uso educativo |

### Fuentes

| Fuente | Uso | Licencia |
|--------|-----|----------|
| Electronic Highway Sign | Títulos y botones | Gratuita |
| LiberationSans (incluida en TMP) | Texto de historia | Open Font License |

---

## 11. Glosario Técnico

| Término | Significado en el proyecto |
|---------|---------------------------|
| **FSM** | Finite State Machine — máquina de estados finita usada en la IA |
| **Telegraph** | Gesto visible del rival antes de golpear — aviso al jugador |
| **Windup / Active / Recovery** | Las 3 fases de un golpe (preparación / impacto / vulnerable) |
| **KO** | Knock-Out — cuando la vida llega a cero |
| **Singleton** | Patrón de diseño: una sola instancia del objeto en todo el juego |
| **ScriptableObject** | Asset de Unity para guardar datos sin depender de GameObjects |
| **DontDestroyOnLoad** | Hace que un objeto sobreviva al cambio de escena |
| **Pool de SFX** | Conjunto fijo de AudioSources reutilizables para evitar cortes de audio |
| **Ken Burns** | Efecto de paneo y zoom lento sobre una imagen fija |
| **URP** | Universal Render Pipeline — pipeline de renderizado de Unity 6 |
