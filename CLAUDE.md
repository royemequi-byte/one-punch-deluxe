# CLAUDE.md — ONE PUNCH DELUXE

> Archivo de contexto maestro. Léelo completo al inicio de cada sesión.
> Si algo de lo que te piden contradice este archivo, avisa antes de ejecutarlo.

---

## 1. CONTEXTO CRÍTICO DEL PROYECTO

**Qué es:** videojuego de boxeo 2D estilo arcade, trabajo de grado universitario.

| Factor | Realidad |
|---|---|
| Plazo | 7 semanas de trabajo efectivo |
| Nivel del desarrollador | Principiante en programación |
| Experiencia en Unity | Cero. Nunca había abierto el editor |
| Arte | Assets gratis y generados por IA, no propios |
| Equipo | Grupal, pero solo una persona programa |
| Entregable | Build jugable + documentación + sustentación oral |

**La consecuencia más importante:** este proyecto se aprueba o se reprueba en la **sustentación oral**. El desarrollador tiene que poder explicar cada sistema sin ayuda. Un código brillante que él no entiende vale menos que un código simple que sí puede defender.

**Por lo tanto: SIMPLICIDAD > ELEGANCIA. SIEMPRE.**

---

## 2. REGLAS DE ORO (no negociables)

1. **Explica antes de codificar.** Antes de crear cualquier script nuevo, escribe 3–5 líneas: qué hace, con qué clases habla y por qué se necesita.
2. **Un script a la vez.** No generes varios archivos de golpe.
3. **Comentarios en español, código en inglés.** Nombres de clases, métodos y variables en inglés (estándar profesional). Comentarios y logs en español.
4. **Nada de código sin comentar.** Cada clase con resumen arriba. Cada método público explicado. Cada línea no obvia, comentada.
5. **Prohibido el sobre-diseño.** Si una solución necesita 3 clases nuevas y una interfaz para algo que resuelve un `if`, usa el `if`.
6. **Nunca toques archivos `.unity`, `.prefab`, `.asset` o `.meta` a mano.** Editarlos fuera del editor corrompe el proyecto. Si algo requiere el editor, entrega instrucciones paso a paso.
7. **Al terminar cada script, pregunta:** *"¿Quieres que te explique cómo funciona antes de seguir?"*
8. **No avances de hito sin autorización explícita.**

---

## 3. ALCANCE

### ✅ DENTRO DEL ALCANCE

- 1 personaje jugable: **Wally Dempsey**
- 2 rivales: **Rival común** y **Kirikú** (jefe final)
- 3 golpes: **Jab, Recto (Straight), Gancho (Hook)**
- Defensa: **Bloqueo** y **Esquive** (posiciones, no desplazamientos)
- Sistema de vida, energía (stamina), rounds y temporizador
- KO, victoria y derrota
- IA con **máquina de estados finita**, compartida por ambos rivales, diferenciada por datos
- Pantallas: Splash → Menú → Historia → Combate → Resultado
- Historia: paneles de imagen fija + texto entre combates
- Guardado en **JSON** (`JsonUtility`)
- **Sistema de audio completo** (ver sección 9)
- Build final para PC/Windows

### ❌ FUERA DEL ALCANCE — va como "trabajo futuro" en el documento

- Uppercut y movimientos especiales
- Los 4 minijuegos de entrenamiento
- Rivales 3, 4 y 5
- Tienda, inventario, sistema de dinero
- Addressables, DOTween
- Multijugador, versión móvil

**Si te piden algo de la lista roja, recuerda que lo sacamos del alcance y pregunta si de verdad quieren meterlo.**

---

## 4. PERSPECTIVA — DECISIÓN CENTRAL

**El juego es tipo PUNCH-OUT!!, NO lateral.**

La cámara está detrás del jugador: se ve la **espalda de Wally** abajo y el **rival de frente** al fondo. Distancia fija, sin desplazamiento.

### Lo que esto elimina completamente del proyecto

- Movimiento horizontal
- `moveSpeed`, `preferredRange`
- Estados `Approach` y `Retreat` en la IA
- Lógica de distancia entre personajes
- Cámara que sigue al personaje

### Esquives

Son **posiciones**, no desplazamientos:
- `Neutro` — posición normal
- `WeaveLeft` — inclinado a la izquierda
- `WeaveRight` — inclinado a la derecha
- `Duck` — agachado
- `Backstep` — paso atrás

### Detección de impacto

**No usa colliders.** Se calcula por **ventana temporal + posición de esquive**:
- Golpe activo → ¿está el objetivo en posición `Neutro` o en la posición que ese golpe alcanza? → impacto confirmado.
- Si el objetivo está en posición correcta de esquive → falla (whiff).

### FSM de la IA (7 estados)

```
Idle → Telegraph → Attack → Recovery → Idle
              ↓                ↑
           Block / Dodge ──────┘
              ↓
           Stunned → KO
```

- **Telegraph:** gesto visible antes de golpear. Su duración define la dificultad.
  - Rival común: `telegraphDuration = 0.6s`
  - Kirikú: `telegraphDuration = 0.25s`

---

## 5. STACK TÉCNICO

| Herramienta | Versión / Nota |
|---|---|
| Unity | 6 LTS, plantilla **Universal 2D (URP)** |
| Lenguaje | C# |
| Editor de código | VS Code + C# Dev Kit + extensión Unity (Microsoft) |
| Input | **Input System** de Unity (paquete nuevo, no el legacy) |
| UI | Canvas + TextMeshPro |
| Datos | ScriptableObjects |
| Persistencia | JSON con `JsonUtility` |
| Audio | AudioSource + **AudioMixer** nativos de Unity |
| Control de versiones | Git + GitHub |

### Paquetes que hay que instalar en Unity (Window → Package Manager)

1. **Input System** (`com.unity.inputsystem`) — al instalarlo Unity pide reiniciar. Acepta.
2. **TextMeshPro** — ya viene incluido. La primera vez que se use, Unity pide importar *TMP Essentials*. Aceptar.
3. Los paquetes 2D vienen con la plantilla Universal 2D. No hace falta instalar nada más.

**No instales ningún paquete adicional sin preguntar antes.**

---

## 6. PATRONES DE DISEÑO

### Se usan (3, y es suficiente)

| Patrón | Dónde | Por qué |
|---|---|---|
| **ScriptableObject como datos** | `FighterData`, `AttackData`, `AIProfile` | Permite balancear el juego sin tocar código |
| **Máquina de estados (FSM)** | IA de rivales, estados del jugador | Es LA forma correcta de manejar comportamiento en juegos de pelea |
| **Observer (eventos de C#)** | El HUD escucha al combate | Desacopla la interfaz de la lógica |

Además, **Singleton simple** en exactamente 3 managers: `GameManager`, `AudioManager`, `SaveManager`. No más.

### PROHIBIDOS

Factory, Command, Event Bus, MVC, Strategy, Repository, interfaces con una sola implementación, inyección de dependencias, abstracciones "por si acaso".

También prohibido: `GameObject.Find()`, `SendMessage()`, variables `public` para exponer en el inspector (usa `[SerializeField] private`).

**Justificación para la sustentación:** *"Se aplicaron los patrones que el problema exigía. Introducir patrones adicionales habría aumentado la complejidad sin resolver un problema real."*

---

## 7. DATOS (ScriptableObjects)

**`FighterData`** — estadísticas base de un peleador
```
fighterName, portrait, maxHealth, maxStamina, attackPower, defense, animatorController
```

**`AttackData`** — define un golpe
```
attackName, damage, staminaCost, windupTime, activeTime, recoveryTime, animationTrigger, hitSound
```

**`AIProfile`** — personalidad de un rival. Esto es lo que hace a un rival más difícil sin escribir una IA nueva.
```
aggressiveness (0-1)       // qué tan seguido ataca
telegraphDuration (seg)    // tiempo de aviso antes de golpear — define la dificultad
blockChance (0-1)          // probabilidad de bloquear
dodgeChance (0-1)          // probabilidad de esquivar
counterChance (0-1)        // probabilidad de contragolpear
comboLength                // golpes seguidos que encadena
```

- **Rival común:** `telegraphDuration 0.6s`, agresividad 0.4, blockChance 0.2
- **Kirikú:** `telegraphDuration 0.25s`, agresividad 0.8, blockChance 0.6, counterChance 0.5

**Ciclo de un golpe:** `Windup` (preparación) → `Active` (ventana de impacto) → `Recovery` (vulnerable).

**Una sola escena de combate** (`03_Fight`). El rival que aparece se decide en runtime cargando el `AIProfile` y `FighterData` correspondiente. Duplicar la escena por cada pelea multiplica el trabajo de mantenimiento.

---

## 8. ARTE

### Perspectiva y estilo

- Vista tipo **Punch-Out!!**: espalda de Wally abajo, rival de frente al fondo.
- Pixel art, inspirado en *Ashita no Joe*.
- **OBLIGATORIO:** Wally debe diferenciarse del diseño original: cambiar color de pelo, paleta del uniforme y agregar un rasgo visual propio. Es criterio de evaluación académica.

### Animaciones de Wally (espalda)

| Animación | Estado |
|---|---|
| `Idle` | ✅ existe |
| `Jab` | ✅ existe |
| `Hook` | ✅ existe |
| `Block` | ✅ existe |
| `Duck` | ✅ existe |
| `WeaveLeft` | ✅ existe |
| `WeaveRight` | ✅ existe |
| `Backstep` | ✅ existe |
| `Slip` | ✅ existe |
| `Hit` | ❌ **falta — crítica** |
| `Knockdown` | ❌ **falta — crítica** |
| `Victory` | ⚠️ falta — media prioridad |
| `Defeat` | ⚠️ falta — media prioridad |

Los rivales necesitan el **mismo set pero visto de frente** (set completamente distinto).

### Requisitos técnicos de sprites

- Celda uniforme **128×128 px**
- Personaje centrado, pies siempre a la misma altura entre frames
- PNG con alfa (fondo transparente)
- Pivot en **Bottom**
- Import en Unity: `Sprite (2D and UI)` / `Multiple` / `Pixels Per Unit: 128` / `Filter Mode: POINT` / `Compression: NONE`

⚠️ Si Filter Mode queda en `Bilinear`, el pixel art se ve borroso. Verificar siempre.

---

## 9. AUDIO

> El audio es requisito obligatorio del proyecto. No es un extra al final.

### Lo que hay que saber antes de tocar audio en Unity

1. Unity **no necesita ningún paquete extra**. AudioSource, AudioListener y AudioMixer son nativos.
2. El AudioMixer trabaja en **decibelios**, no en 0–1. La conversión obligatoria es:
   ```csharp
   mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
   ```
   Un slider lineal se siente completamente roto en la mitad inferior. Es el error #1 de audio en Unity.
3. Los parámetros del mixer deben estar **expuestos manualmente** en el editor (clic derecho sobre el volumen del grupo → *Expose to script*) con los nombres exactos `MusicVolume`, `SFXVolume`, `UIVolume`. Si no se exponen, `SetFloat` falla en silencio sin lanzar ningún error.

### Arquitectura

**AudioMixer** (`Assets/Audio/MainMixer.mixer`) con jerarquía:
```
Master
├── Music
├── SFX
└── UI
```

- `AudioManager` — singleton con `DontDestroyOnLoad`. Métodos: `PlayMusic(clip, fadeTime)`, `PlaySFX(clip)`, `SetMusicVolume(float)`, `SetSFXVolume(float)`.
- `AudioLibrary` — ScriptableObject con todos los clips referenciados por nombre. Evita rutas mágicas.
- **Pool de 5–8 AudioSources** para SFX. Sin pool, dos golpes simultáneos se cortan entre sí.
- Volúmenes se guardan y se restauran al iniciar el juego.

### Formatos

| Tipo | Formato | Load Type | Compresión |
|---|---|---|---|
| Música (loops) | `.ogg` | Streaming | Vorbis 70% |
| SFX cortos | `.wav` | Decompress on Load | PCM / ADPCM |

SFX: marcar **Force To Mono** — ocupa la mitad y en 2D no se nota la diferencia.

### Lista mínima de sonidos (19 archivos)

- **Música (5):** menú, historia, combate normal, combate Kirikú, victoria
- **SFX combate (10):** jab, recto, gancho, impacto en bloqueo, whiff, gruñido de daño, caída/knockdown, campana de round, cuenta del árbitro, multitud de fondo
- **SFX UI (4):** hover de botón, clic, atrás, notificación

### Fuentes de audio gratuitas y legales

⚠️ Verificar SIEMPRE la licencia y registrarla en `Assets/Audio/CREDITOS.md`.

- **freesound.org** — efectos. Filtrar por CC0.
- **opengameart.org** — música y efectos para juegos.
- **kenney.nl/assets** — packs de UI y efectos, todos CC0.
- **pixabay.com/music** — música libre de regalías.
- **incompetech.com** — Kevin MacLeod (CC-BY, exige dar crédito).

---

## 10. PYTHON (solo herramientas de producción)

**Python NUNCA ejecuta lógica de juego.** Regla: *"C# ejecuta el juego, Python prepara lo que C# consume."*

### Dónde vive

En `Tools/`, **nunca dentro de `Assets/`** (Unity generaría archivos `.meta` basura).

### Herramientas autorizadas (solo 2)

**`Tools/normalize_sprites.py`** (usa Pillow)
- Quita el fondo, detecta frames, auto-crop, centra
- **Alinea por la base** (pies a la misma altura en todos los frames — lo más crítico: si un frame queda 3px arriba, el personaje "brinca")
- Rellena a celda 128×128, exporta spritesheet uniforme
- Flag `--skip-align` para Knockdown (el personaje cae, la alineación no aplica)

**`Tools/process_audio.py`** (usa ffmpeg)
- Convierte formatos
- Fuerza mono en SFX
- Normaliza volumen
- Recorta silencios iniciales/finales
- Renombra por convención del proyecto

### Reglas de los originales

- Archivos originales en `Tools/input/` — son sagrados, nunca se sobrescriben
- Salida procesada en `Assets/`
- Nunca al revés

### PROHIBIDO en Python

Lógica de juego, machine learning, backend, base de datos, paquete "Python for Unity".

---

## 11. ESTRUCTURA DE CARPETAS

```
OnePunchDeluxe/
├── .github/
│   └── workflows/         # CI/CD si se configura
├── Tools/                 # herramientas Python — NUNCA dentro de Assets/
│   ├── input/             # originales sagrados (sprites y audio sin procesar)
│   ├── normalize_sprites.py
│   ├── process_audio.py
│   └── venv/              # excluido por .gitignore
├── Assets/
│   ├── Art/
│   │   ├── Characters/
│   │   │   ├── Wally/
│   │   │   ├── RivalComun/
│   │   │   └── Kiriku/
│   │   ├── Backgrounds/
│   │   │   ├── Ring/
│   │   │   └── Story/
│   │   └── UI/
│   │       ├── Buttons/
│   │       ├── Bars/
│   │       └── Icons/
│   ├── Animations/
│   │   ├── Wally/
│   │   ├── RivalComun/
│   │   └── Kiriku/
│   ├── Audio/
│   │   ├── Music/         # .ogg
│   │   ├── SFX/           # .wav
│   │   └── CREDITOS.md    # registro obligatorio de licencias
│   ├── Fonts/
│   ├── Prefabs/
│   │   └── UI/
│   ├── ScriptableObjects/
│   │   ├── Fighters/
│   │   ├── Attacks/
│   │   └── AIProfiles/
│   ├── Scenes/
│   │   ├── 00_Splash.unity
│   │   ├── 01_MainMenu.unity
│   │   ├── 02_Story.unity
│   │   ├── 03_Fight.unity      # UNA escena de combate, reutilizable
│   │   └── 04_Result.unity
│   └── Scripts/
│       ├── Core/           # GameManager, SceneLoader, GameState
│       ├── Combat/         # Fighter, CombatController, Health, Stamina, FightManager
│       ├── Player/         # PlayerController, PlayerInputHandler
│       ├── AI/             # AIController, estados FSM
│       │   └── States/
│       ├── Data/           # ScriptableObjects (FighterData, AttackData, AIProfile)
│       ├── Save/           # SaveManager, SaveData
│       ├── Audio/          # AudioManager, AudioLibrary
│       ├── UI/             # HUDController, menús
│       ├── Story/          # StoryPlayer, StoryPanel
│       └── Utils/          # Singleton base, helpers
```

---

## 12. CRONOGRAMA — 7 SEMANAS

| Semana | Hito | Entregable verificable |
|---|---|---|
| 1 | Setup + arte elegido | Proyecto en Git, sprites decididos, estructura de carpetas lista |
| 2 | **Combate gris jugable** | Wally golpea a una cápsula, le quita vida, hay KO |
| 3 | Sprites + Animator | Se ve como un juego de boxeo real |
| 4 | IA con FSM | El rival pelea solo con Telegraph visible |
| 5 | HUD, rounds, menús | Ciclo completo Splash→Combate→Resultado |
| 6 | Audio, historia, guardado | Juego completo y jugable con sonido |
| 7 | Pulido + **build** + documentación | Entregable final |

**Reglas del cronograma:**
- Al final de la **semana 2** tiene que haber algo jugable.
- La **semana 7 completa** es para pulir y compilar. Nunca dejar el build para el último día.

---

## 13. GIT

**Commits en español, formato imperativo:**

```
feat: agregar sistema de vida del peleador
fix: corregir que el bloqueo no reducía el daño
docs: documentar la clase CombatController
refactor: separar Stamina de la clase Fighter
audio: agregar AudioManager con control de volumen
tools: agregar normalize_sprites.py
```

- Commit **después de cada script que funcione**. Nunca acumular varios archivos.
- Rama única `main`.
- `.gitignore` obligatorio (ver archivo en la raíz).

---

## 14. CONVENCIONES DE CÓDIGO

```csharp
using UnityEngine;

/// <summary>
/// Administra la vida de un peleador y avisa cuando cambia o llega a cero.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public event System.Action<int, int> OnHealthChanged;  // (actual, máxima)
    public event System.Action OnDeath;
}
```

- Clases y métodos públicos: `PascalCase`
- Variables y parámetros: `camelCase`
- Privadas en inspector: `[SerializeField] private` — **nunca** `public`
- Constantes: `MAYUSCULAS_CON_GUION`
- Eventos: siempre empiezan con `On`
- Un archivo = una clase = mismo nombre
- **Máximo 150 líneas por script**
- **Máximo 25 líneas por método**
- Usa `[Header("...")]` para agrupar campos en el inspector

---

## 15. DEFINICIÓN DE "TERMINADO"

Un script está listo solo si cumple **todo**:

- [ ] Tiene resumen `<summary>` explicando qué hace
- [ ] Todos los métodos públicos comentados
- [ ] Menos de 150 líneas
- [ ] Una sola responsabilidad
- [ ] Compila sin errores ni advertencias en Unity
- [ ] Se probó en el editor y funciona
- [ ] El desarrollador puede explicar qué hace con sus palabras
- [ ] Commiteado

---

## 16. REPARTO DE TRABAJO

**Claude Code hace:**
Escribir y editar los `.cs`, herramientas Python en `Tools/`, `.gitignore`, commits, documentación, revisión de código, instrucciones del editor.

**El humano hace (obligatoriamente):**
Todo lo del editor de Unity: importar sprites, configurar el Animator, armar prefabs, colocar objetos, conectar referencias en el inspector, crear el AudioMixer, darle Play y probar.

**Por eso:** cada vez que un script requiera configuración en el editor, Claude entrega instrucciones **paso a paso numeradas** para alguien que nunca ha abierto Unity. Nombra siempre la ventana y su ubicación. Incluye un checkpoint de "cómo sabes que salió bien".

---

## 17. AGENTES DISPONIBLES

En `.claude/agents/`:

| Agente | Cuándo usarlo |
|---|---|
| `unity-scripter` | Escribir cualquier script C# nuevo o modificar uno existente |
| `code-reviewer` | Revisar un script antes de commitear |
| `editor-guide` | Traducir un script a instrucciones del editor de Unity |
| `tutor` | Explicar código y preparar la sustentación oral |
| `audio-engineer` | Todo lo relacionado con sonido y AudioMixer |
| `asset-hunter` | Buscar sprites y audio gratis con licencia verificada |
| `tools-engineer` | Escribir o modificar las herramientas Python de `Tools/` |
