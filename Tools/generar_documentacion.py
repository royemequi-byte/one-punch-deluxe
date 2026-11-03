"""
Genera la documentación técnica completa de ONE PUNCH DELUXE en HTML.
Abre el archivo generado en el navegador y usa Ctrl+P -> Guardar como PDF.
"""

import base64
import os
from pathlib import Path

# Rutas del proyecto
ROOT = Path(r"C:\Users\Lenovo\Documents\OnePunchDeluxe")
SCRIPTS = ROOT / "Assets" / "Scripts"
ART = ROOT / "Assets" / "Art"
STORY_IMG = ART / "Backgrounds" / "Story"
WALLY_IMG = ART / "Personajes" / "Wally"
RIVAL_IMG = ART / "Personajes" / "Rival"
UI_IMG = ART / "Personajes" / "UI"
SALIDA = Path(r"C:\Users\Lenovo\Desktop\OnePunchDeluxe_Documentacion.html")


def imagen_b64(ruta):
    if not Path(ruta).exists():
        return ""
    with open(ruta, "rb") as f:
        datos = base64.b64encode(f.read()).decode("utf-8")
    ext = Path(ruta).suffix.lower().replace(".", "")
    if ext == "jpg":
        ext = "jpeg"
    return f"data:image/{ext};base64,{datos}"


def leer_script(ruta):
    try:
        with open(ruta, "r", encoding="utf-8") as f:
            return f.read()
    except:
        return "// No se pudo leer el archivo"


# ── Leer todos los scripts ─────────────────────────────────────────────────────
scripts = {
    "Health.cs": leer_script(SCRIPTS / "Combat" / "Health.cs"),
    "Stamina.cs": leer_script(SCRIPTS / "Combat" / "Stamina.cs"),
    "DodgePosition.cs": leer_script(SCRIPTS / "Combat" / "DodgePosition.cs"),
    "CombatController.cs": leer_script(SCRIPTS / "Combat" / "CombatController.cs"),
    "FightManager.cs": leer_script(SCRIPTS / "Combat" / "FightManager.cs"),
    "PlayerInputHandler.cs": leer_script(SCRIPTS / "Player" / "PlayerInputHandler.cs"),
    "AttackData.cs": leer_script(SCRIPTS / "Data" / "AttackData.cs"),
    "AIProfile.cs": leer_script(SCRIPTS / "Data" / "AIProfile.cs"),
    "RivalAI.cs": leer_script(SCRIPTS / "AI" / "RivalAI.cs"),
    "HUDController.cs": leer_script(SCRIPTS / "UI" / "HUDController.cs"),
    "AudioManager.cs": leer_script(SCRIPTS / "Audio" / "AudioManager.cs"),
    "AudioLibrary.cs": leer_script(SCRIPTS / "Audio" / "AudioLibrary.cs"),
    "SaveData.cs": leer_script(SCRIPTS / "Save" / "SaveData.cs"),
    "SaveManager.cs": leer_script(SCRIPTS / "Save" / "SaveManager.cs"),
    "StoryPanel.cs": leer_script(SCRIPTS / "Story" / "StoryPanel.cs"),
    "StoryPlayer.cs": leer_script(SCRIPTS / "Story" / "StoryPlayer.cs"),
    "SplashController.cs": leer_script(SCRIPTS / "UI" / "SplashController.cs"),
    "MainMenuController.cs": leer_script(SCRIPTS / "UI" / "MainMenuController.cs"),
}

# Descripciones de cada script
descripciones = {
    "Health.cs": "Administra la vida de un peleador. Reduce vida al recibir daño y dispara eventos cuando cambia o llega a cero. Se usa tanto en Wally como en los rivales.",
    "Stamina.cs": "Administra la energía del peleador. Baja al atacar o bloquear, y se recupera automáticamente con el tiempo. Sin stamina no es posible ejecutar ataques.",
    "DodgePosition.cs": "Enumeración de las posiciones de esquive: Neutral, WeaveLeft, WeaveRight, Duck y Backstep. Define el sistema de evasión sin desplazamiento.",
    "CombatController.cs": "Ejecuta el ciclo completo de un golpe: Windup → Active → Recovery. Durante la fase Active evalúa si el rival está esquivando y aplica o descarta el daño.",
    "FightManager.cs": "Árbitro de la pelea. Inicializa el combate, conecta a los participantes, escucha los KOs y el tiempo agotado, y muestra la pantalla de resultado.",
    "PlayerInputHandler.cs": "Lee el teclado y traduce las pulsaciones en acciones de combate. J=Jab, K=Recto, L=Gancho, WASD=Esquive, Espacio=Bloqueo.",
    "AttackData.cs": "ScriptableObject con los datos de un golpe: daño, costo de stamina, tiempos de cada fase, trigger de animación y sonido. Se edita desde Unity sin tocar código.",
    "AIProfile.cs": "ScriptableObject con la personalidad del rival: tiempo de telegrafía, cooldown entre ataques y lista de ataques disponibles. Kirikú y el Rival Común usan el mismo código pero distintos perfiles.",
    "RivalAI.cs": "Loop de IA con máquina de estados: espera → telegrafía (muestra '!') → golpea → repite. FightManager lo activa y desactiva.",
    "HUDController.cs": "Controla el HUD de combate: barras de vida, barra de stamina, temporizador y pantalla de resultado con botones Reintentar y Menú.",
    "AudioManager.cs": "Singleton de audio con pool de 8 AudioSources para SFX. Maneja fade entre pistas de música. Persiste entre escenas con DontDestroyOnLoad.",
    "AudioLibrary.cs": "ScriptableObject con referencias a todos los clips de audio. Evita rutas mágicas en el código y centraliza la asignación de sonidos.",
    "SaveData.cs": "Clase serializable con los datos de la partida: nivel actual, victorias y derrotas. JsonUtility la convierte a/desde JSON.",
    "SaveManager.cs": "Singleton que guarda y carga la partida en un archivo JSON usando Application.persistentDataPath. Persiste entre escenas.",
    "StoryPanel.cs": "ScriptableObject con los datos de un panel de historia: imagen de fondo y texto narrativo.",
    "StoryPlayer.cs": "Muestra los paneles de historia con efecto de máquina de escribir y paneo Ken Burns. Al terminar todos los paneles carga la escena de combate.",
    "SplashController.cs": "Muestra el logo del juego con fade in/out y luego carga el menú principal automáticamente.",
    "MainMenuController.cs": "Controla el menú principal con botones de Nueva Partida, Continuar y Salir. Continuar solo está activo si hay progreso guardado.",
}

# Agrupación por módulo
modulos = {
    "Combate": ["Health.cs", "Stamina.cs", "DodgePosition.cs", "CombatController.cs", "FightManager.cs"],
    "Jugador": ["PlayerInputHandler.cs"],
    "Datos (ScriptableObjects)": ["AttackData.cs", "AIProfile.cs"],
    "Inteligencia Artificial": ["RivalAI.cs"],
    "Interfaz de Usuario": ["HUDController.cs", "SplashController.cs", "MainMenuController.cs"],
    "Audio": ["AudioManager.cs", "AudioLibrary.cs"],
    "Guardado": ["SaveData.cs", "SaveManager.cs"],
    "Historia": ["StoryPanel.cs", "StoryPlayer.cs"],
}

# ── Cargar imágenes ────────────────────────────────────────────────────────────
logo = imagen_b64(UI_IMG / "logo_splash_new.png")
panel1 = imagen_b64(STORY_IMG / "story_panel_01_gimnasio.png")
panel2 = imagen_b64(STORY_IMG / "story_panel_02_torneo.png")
panel3 = imagen_b64(STORY_IMG / "story_panel_03_arena.png")
wally_idle = imagen_b64(WALLY_IMG / "wally_idle.png")
wally_jab = imagen_b64(WALLY_IMG / "wally_jab.png")
wally_hook = imagen_b64(WALLY_IMG / "wally_hook.png")
wally_block = imagen_b64(WALLY_IMG / "wally_block.png")
wally_victory = imagen_b64(WALLY_IMG / "wally_victory.png")
rival_idle = imagen_b64(RIVAL_IMG / "rival_idle.png")
rival_attack = imagen_b64(RIVAL_IMG / "rival_attack.png")

# ── Construir HTML ─────────────────────────────────────────────────────────────
def bloque_script(nombre, codigo, descripcion):
    codigo_escaped = codigo.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
    return f"""
    <div class="script-block">
      <div class="script-header">
        <span class="script-name">{nombre}</span>
        <span class="script-badge">C#</span>
      </div>
      <p class="script-desc">{descripcion}</p>
      <pre><code>{codigo_escaped}</code></pre>
    </div>
    """

def seccion_modulo(nombre_modulo, lista_scripts):
    contenido = ""
    for s in lista_scripts:
        contenido += bloque_script(s, scripts.get(s, ""), descripciones.get(s, ""))
    return f"""
    <div class="modulo">
      <h3 class="modulo-titulo">{nombre_modulo}</h3>
      {contenido}
    </div>
    """

sprites_wally = ""
for nombre, b64 in [("Idle", wally_idle), ("Jab", wally_jab), ("Hook", wally_hook), ("Block", wally_block), ("Victory", wally_victory)]:
    if b64:
        sprites_wally += f'<div class="sprite-item"><img src="{b64}" alt="{nombre}"><p>{nombre}</p></div>'

sprites_rival = ""
for nombre, b64 in [("Idle", rival_idle), ("Attack", rival_attack)]:
    if b64:
        sprites_rival += f'<div class="sprite-item"><img src="{b64}" alt="{nombre}"><p>{nombre}</p></div>'

paneles_html = ""
for titulo, texto, b64 in [
    ("Panel 1 — El Descubrimiento", "La madre de Wally estaba enferma. El dinero se acababa. Entró a pelear por necesidad, no por gloria. Pero entre la multitud de aquella pelea clandestina, alguien lo vio diferente. Tange Danpei había encontrado lo que buscaba toda su vida.", panel1),
    ("Panel 2 — El Ascenso", "Bajo la guía de Danpei, Wally ascendió como pocos. Torneo tras torneo, rival tras rival. El cinturón nacional cayó en sus manos. Pero el verdadero enemigo aún no había mostrado su rostro.", panel2),
    ("Panel 3 — La Gloria y la Maldición", "Kirikú. Campeón del mundo. El hombre que mantenía a toda una ciudad sumida en la miseria. Esta noche Wally pelea por su madre, por su ciudad, por todo. Lo que nadie sabe es el precio que pagará.", panel3),
]:
    img_tag = f'<img src="{b64}" alt="{titulo}" class="panel-img">' if b64 else '<div class="panel-img-placeholder">Imagen no disponible</div>'
    paneles_html += f"""
    <div class="panel-historia">
      {img_tag}
      <div class="panel-texto">
        <h4>{titulo}</h4>
        <p>{texto}</p>
      </div>
    </div>
    """

logo_tag = f'<img src="{logo}" alt="Logo ONE PUNCH DELUXE" class="logo-portada">' if logo else '<h1 style="font-size:3em;color:#e8b400;">ONE PUNCH DELUXE</h1>'

secciones_codigo = ""
for modulo, lista in modulos.items():
    secciones_codigo += seccion_modulo(modulo, lista)

html = f"""<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>ONE PUNCH DELUXE — Documentación Técnica</title>
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #0d0d0d; color: #e0e0e0; line-height: 1.6; }}

  /* ── PORTADA ── */
  .portada {{
    background: linear-gradient(135deg, #0d0d0d 0%, #1a0a00 50%, #0d0d0d 100%);
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: 40px;
    border-bottom: 3px solid #e8b400;
    page-break-after: always;
  }}
  .logo-portada {{ max-width: 400px; margin-bottom: 30px; }}
  .portada h1 {{ font-size: 2.8em; color: #e8b400; text-transform: uppercase; letter-spacing: 4px; margin-bottom: 10px; }}
  .portada h2 {{ font-size: 1.3em; color: #ff6b35; margin-bottom: 40px; font-weight: 400; }}
  .portada-info {{ background: rgba(255,255,255,0.05); border: 1px solid #333; border-radius: 8px; padding: 30px 50px; margin-top: 20px; }}
  .portada-info p {{ margin: 8px 0; color: #bbb; font-size: 1em; }}
  .portada-info strong {{ color: #e8b400; }}

  /* ── LAYOUT GENERAL ── */
  .contenido {{ max-width: 1000px; margin: 0 auto; padding: 40px 30px; }}
  h1.seccion {{ font-size: 2em; color: #e8b400; border-bottom: 2px solid #e8b400; padding-bottom: 10px; margin: 50px 0 25px; page-break-before: always; }}
  h2.subseccion {{ font-size: 1.4em; color: #ff6b35; margin: 35px 0 15px; }}
  h3.modulo-titulo {{ font-size: 1.2em; color: #e8b400; background: rgba(232,180,0,0.1); border-left: 4px solid #e8b400; padding: 10px 16px; margin: 30px 0 15px; border-radius: 0 4px 4px 0; }}
  p {{ margin-bottom: 14px; color: #ccc; }}

  /* ── TABLA ── */
  table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
  th {{ background: #e8b400; color: #000; padding: 10px 14px; text-align: left; font-size: 0.9em; }}
  td {{ padding: 9px 14px; border-bottom: 1px solid #222; color: #ccc; font-size: 0.9em; }}
  tr:nth-child(even) td {{ background: rgba(255,255,255,0.03); }}

  /* ── CÓDIGO ── */
  .script-block {{ margin: 20px 0; border: 1px solid #2a2a2a; border-radius: 8px; overflow: hidden; page-break-inside: avoid; }}
  .script-header {{ background: #1a1a2e; padding: 10px 16px; display: flex; align-items: center; gap: 10px; border-bottom: 1px solid #2a2a2a; }}
  .script-name {{ color: #7ec8e3; font-family: 'Courier New', monospace; font-size: 0.95em; font-weight: bold; }}
  .script-badge {{ background: #562c8e; color: #fff; font-size: 0.7em; padding: 2px 8px; border-radius: 10px; font-weight: bold; }}
  .script-desc {{ padding: 10px 16px; background: rgba(255,255,255,0.02); color: #aaa; font-size: 0.88em; border-bottom: 1px solid #1a1a1a; margin: 0; }}
  pre {{ background: #0d1117; padding: 20px; overflow-x: auto; margin: 0; }}
  code {{ font-family: 'Courier New', Consolas, monospace; font-size: 0.82em; color: #e6edf3; line-height: 1.6; }}

  /* ── HISTORIA ── */
  .panel-historia {{ display: flex; gap: 20px; margin: 25px 0; background: rgba(255,255,255,0.03); border: 1px solid #2a2a2a; border-radius: 8px; overflow: hidden; page-break-inside: avoid; }}
  .panel-img {{ width: 320px; min-width: 320px; object-fit: cover; }}
  .panel-img-placeholder {{ width: 320px; min-width: 320px; background: #1a1a1a; display: flex; align-items: center; justify-content: center; color: #555; }}
  .panel-texto {{ padding: 20px; }}
  .panel-texto h4 {{ color: #e8b400; margin-bottom: 12px; font-size: 1em; }}
  .panel-texto p {{ color: #bbb; font-size: 0.92em; font-style: italic; }}

  /* ── SPRITES ── */
  .sprites-grid {{ display: flex; flex-wrap: wrap; gap: 15px; margin: 20px 0; }}
  .sprite-item {{ text-align: center; background: rgba(255,255,255,0.03); border: 1px solid #2a2a2a; border-radius: 8px; padding: 10px; }}
  .sprite-item img {{ max-height: 140px; max-width: 120px; object-fit: contain; display: block; margin: 0 auto 8px; }}
  .sprite-item p {{ font-size: 0.8em; color: #888; margin: 0; }}

  /* ── SEMANAS ── */
  .semana-card {{ background: rgba(255,255,255,0.03); border: 1px solid #2a2a2a; border-radius: 8px; padding: 20px; margin: 15px 0; border-left: 4px solid #e8b400; page-break-inside: avoid; }}
  .semana-card h4 {{ color: #e8b400; margin-bottom: 8px; }}
  .semana-card ul {{ padding-left: 20px; color: #bbb; font-size: 0.92em; }}
  .semana-card li {{ margin: 4px 0; }}
  .badge-ok {{ color: #4caf50; font-weight: bold; }}

  /* ── CONTROLES ── */
  .controles-grid {{ display: flex; flex-wrap: wrap; gap: 10px; margin: 15px 0; }}
  .key-item {{ background: #1a1a2e; border: 1px solid #444; border-radius: 6px; padding: 10px 16px; text-align: center; min-width: 100px; }}
  .key-item .key {{ font-family: monospace; font-size: 1.3em; color: #e8b400; font-weight: bold; display: block; }}
  .key-item .accion {{ font-size: 0.78em; color: #aaa; margin-top: 4px; display: block; }}

  /* ── PATRONES ── */
  .patron-card {{ background: rgba(255,255,255,0.03); border: 1px solid #2a2a2a; border-radius: 8px; padding: 18px; margin: 10px 0; }}
  .patron-card h4 {{ color: #ff6b35; margin-bottom: 6px; }}
  .patron-card p {{ font-size: 0.9em; color: #aaa; margin: 0; }}

  /* ── PRINT ── */
  @media print {{
    body {{ background: white; color: #111; }}
    .portada {{ background: white; border-bottom: 3px solid #c8960a; }}
    .portada h1 {{ color: #c8960a; }}
    .portada h2 {{ color: #e05a00; }}
    .portada-info {{ border-color: #ddd; }}
    h1.seccion {{ color: #c8960a; border-color: #c8960a; }}
    h2.subseccion {{ color: #e05a00; }}
    h3.modulo-titulo {{ color: #c8960a; border-color: #c8960a; background: #fff8e0; }}
    pre {{ background: #f5f5f5; border: 1px solid #ddd; }}
    code {{ color: #222; }}
    .script-header {{ background: #1a1a2e; }}
    .semana-card {{ border-color: #ddd; border-left-color: #c8960a; }}
    .key-item {{ background: #f5f5f5; border-color: #ccc; }}
    .key-item .key {{ color: #c8960a; }}
    .patron-card {{ border-color: #ddd; }}
    td {{ color: #333; }}
    p {{ color: #333; }}
    .panel-texto p {{ color: #444; }}
    .sprite-item {{ border-color: #ddd; }}
  }}
</style>
</head>
<body>

<!-- ═══════════════════════════════ PORTADA ════════════════════════════════ -->
<div class="portada">
  {logo_tag}
  <h1>ONE PUNCH DELUXE</h1>
  <h2>Videojuego de Boxeo 2D — Documentación Técnica</h2>
  <div class="portada-info">
    <p><strong>Proyecto:</strong> Trabajo de Grado — Ingeniería de Sistemas</p>
    <p><strong>Motor:</strong> Unity 6 LTS — Universal Render Pipeline 2D</p>
    <p><strong>Lenguaje:</strong> C# (lógica de juego) · Python (herramientas de producción)</p>
    <p><strong>Control de versiones:</strong> Git + GitHub + Git LFS</p>
    <p><strong>Duración del desarrollo:</strong> 7 semanas (julio – agosto 2026)</p>
    <p><strong>Fecha de entrega:</strong> Agosto 2026</p>
  </div>
</div>

<!-- ═══════════════════════════════ CONTENIDO ═════════════════════════════ -->
<div class="contenido">

<!-- ─── 1. INTRODUCCIÓN ─────────────────────────────────────────────────── -->
<h1 class="seccion">1. Introducción</h1>
<p>
  <strong>ONE PUNCH DELUXE</strong> es un videojuego de boxeo 2D estilo arcade desarrollado
  en Unity 6 LTS como proyecto de trabajo de grado. El juego adopta la perspectiva clásica
  del título <em>Punch-Out!!</em> de Nintendo: la cámara se posiciona detrás del jugador,
  permitiendo ver la espalda de Wally Dempsey en la parte inferior y al rival de frente al fondo.
  Esta decisión de diseño elimina la necesidad de programar movimiento horizontal, cámara
  dinámica y cálculos de distancia, permitiendo enfocarse en la profundidad del sistema de combate.
</p>
<p>
  El sistema de combate se basa en <strong>ventanas temporales</strong> y <strong>posiciones de esquive</strong>,
  exactamente como en el Punch-Out!! original. No se usan colisiones físicas: un golpe impacta
  si el rival no está en la posición de esquive correcta durante la fase Active del ataque.
</p>

<h2 class="subseccion">Objetivos del proyecto</h2>
<table>
  <tr><th>Objetivo</th><th>Estado</th></tr>
  <tr><td>Juego 2D completo y jugable en PC Windows</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>Personaje jugable con 3 tipos de golpe y 5 esquives</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>Rival con IA basada en máquina de estados finita</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>HUD con barras de vida, stamina, timer y rounds</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>Sistema de audio completo con pool de SFX</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>Historia con paneles narrativos y efecto Ken Burns</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>Guardado de progreso en JSON</td><td class="badge-ok">✅ Completado</td></tr>
  <tr><td>Build ejecutable para Windows x86_64</td><td class="badge-ok">✅ Completado</td></tr>
</table>

<!-- ─── 2. TECNOLOGÍAS ──────────────────────────────────────────────────── -->
<h1 class="seccion">2. Tecnologías Utilizadas</h1>
<table>
  <tr><th>Herramienta</th><th>Versión</th><th>Uso</th></tr>
  <tr><td>Unity</td><td>6 LTS (6000.5.5f1)</td><td>Motor del juego, escenas, física, render</td></tr>
  <tr><td>Universal Render Pipeline 2D</td><td>Incluida en Unity 6</td><td>Renderizado optimizado para juegos 2D</td></tr>
  <tr><td>C#</td><td>.NET Standard 2.1</td><td>Toda la lógica del juego (18 scripts)</td></tr>
  <tr><td>Input System</td><td>com.unity.inputsystem</td><td>Lectura de teclado con la API moderna de Unity</td></tr>
  <tr><td>TextMeshPro</td><td>Incluido en Unity</td><td>Renderizado de texto de alta calidad en UI</td></tr>
  <tr><td>JsonUtility</td><td>Nativo de Unity</td><td>Serialización del guardado de partida</td></tr>
  <tr><td>Git + Git LFS</td><td>Última versión</td><td>Control de versiones y almacenamiento de assets grandes</td></tr>
  <tr><td>GitHub</td><td>—</td><td>Repositorio remoto y respaldo en la nube</td></tr>
  <tr><td>Python</td><td>3.x</td><td>Herramientas de producción: normalización de sprites y audio</td></tr>
</table>

<!-- ─── 3. ARQUITECTURA ─────────────────────────────────────────────────── -->
<h1 class="seccion">3. Arquitectura del Sistema</h1>
<p>
  El proyecto aplica tres patrones de diseño clásicos, seleccionados porque el problema
  los exigía, no por completitud técnica:
</p>

<div class="patron-card">
  <h4>ScriptableObject como datos</h4>
  <p>Las estadísticas de los peleadores (AttackData, AIProfile) viven en archivos separados del código.
  Esto permite balancear la dificultad del juego sin modificar ningún script. Kirikú y el Rival Común
  usan exactamente el mismo código de IA, diferenciado únicamente por su AIProfile.</p>
</div>

<div class="patron-card">
  <h4>Máquina de estados finita (FSM)</h4>
  <p>La IA del rival recorre estados definidos: Idle → Telegraph → Attack → Recovery → Idle.
  El estado Telegraph muestra un indicador visual "!" que avisa al jugador. Su duración configurable
  (0.6s para el rival común, 0.25s para Kirikú) define toda la dificultad del juego.</p>
</div>

<div class="patron-card">
  <h4>Observer (eventos de C#)</h4>
  <p>Health y Stamina publican eventos (OnHealthChanged, OnStaminaChanged, OnDeath) a los que
  el HUD se suscribe. Esto desacopla completamente la lógica de combate de la interfaz visual.
  Agregar un efecto de sonido al recibir daño no requiere modificar Health.cs.</p>
</div>

<h2 class="subseccion">Módulos del sistema</h2>
<table>
  <tr><th>Módulo</th><th>Scripts</th><th>Responsabilidad</th></tr>
  <tr><td>Combate</td><td>Health, Stamina, DodgePosition, CombatController, FightManager</td><td>Núcleo del sistema de pelea</td></tr>
  <tr><td>Jugador</td><td>PlayerInputHandler</td><td>Traducir teclado en acciones de combate</td></tr>
  <tr><td>Datos</td><td>AttackData, AIProfile</td><td>ScriptableObjects editables desde Unity</td></tr>
  <tr><td>IA</td><td>RivalAI</td><td>Comportamiento autónomo del rival</td></tr>
  <tr><td>Audio</td><td>AudioManager, AudioLibrary</td><td>Música y efectos de sonido</td></tr>
  <tr><td>Guardado</td><td>SaveData, SaveManager</td><td>Persistencia en JSON</td></tr>
  <tr><td>Historia</td><td>StoryPanel, StoryPlayer</td><td>Paneles narrativos entre combates</td></tr>
  <tr><td>UI</td><td>HUDController, SplashController, MainMenuController</td><td>Todas las interfaces del juego</td></tr>
</table>

<h2 class="subseccion">Flujo de un golpe</h2>
<table>
  <tr><th>Paso</th><th>Qué ocurre</th><th>Script responsable</th></tr>
  <tr><td>1</td><td>Jugador presiona J, K o L</td><td>PlayerInputHandler</td></tr>
  <tr><td>2</td><td>Se consulta si hay stamina disponible</td><td>Stamina</td></tr>
  <tr><td>3</td><td>Se resta la stamina y arranca el ciclo</td><td>CombatController</td></tr>
  <tr><td>4</td><td>Se dispara el trigger de animación</td><td>Animator (Unity)</td></tr>
  <tr><td>5</td><td>Fase Windup: preparación (no daña)</td><td>CombatController</td></tr>
  <tr><td>6</td><td>Fase Active: ¿el rival esquivó correctamente?</td><td>CombatController</td></tr>
  <tr><td>7a</td><td>No esquivó → se aplica daño a Health del rival</td><td>Health</td></tr>
  <tr><td>7b</td><td>Esquivó → sonido de whiff, no hay daño</td><td>AudioManager</td></tr>
  <tr><td>8</td><td>Health dispara OnHealthChanged</td><td>Health</td></tr>
  <tr><td>9</td><td>HUD actualiza la barra de vida automáticamente</td><td>HUDController</td></tr>
  <tr><td>10</td><td>Fase Recovery: el peleador queda vulnerable</td><td>CombatController</td></tr>
</table>

<!-- ─── 4. CONTROLES ────────────────────────────────────────────────────── -->
<h1 class="seccion">4. Controles del Juego</h1>
<div class="controles-grid">
  <div class="key-item"><span class="key">J</span><span class="accion">Jab</span></div>
  <div class="key-item"><span class="key">K</span><span class="accion">Recto</span></div>
  <div class="key-item"><span class="key">L</span><span class="accion">Gancho</span></div>
  <div class="key-item"><span class="key">A</span><span class="accion">Esquive izq.</span></div>
  <div class="key-item"><span class="key">D</span><span class="accion">Esquive der.</span></div>
  <div class="key-item"><span class="key">S</span><span class="accion">Agacharse</span></div>
  <div class="key-item"><span class="key">W</span><span class="accion">Paso atrás</span></div>
  <div class="key-item"><span class="key">ESP</span><span class="accion">Bloquear</span></div>
</div>

<!-- ─── 5. HISTORIA ─────────────────────────────────────────────────────── -->
<h1 class="seccion">5. Historia — El Camino del Campeón</h1>
<p>La narrativa del juego se presenta en tres paneles antes del combate, con efecto de
máquina de escribir y paneo Ken Burns. Cada panel combina una imagen cinematográfica con
texto narrativo.</p>
{paneles_html}

<!-- ─── 6. ARTE Y PERSONAJES ─────────────────────────────────────────────── -->
<h1 class="seccion">6. Arte y Personajes</h1>
<h2 class="subseccion">Wally Dempsey — Personaje Jugable</h2>
<p>Visto desde la espalda, perspectiva tipo Punch-Out!!. Sprites en pixel art estilo Ashita no Joe,
formato 128×128 px, PNG con canal alfa, pivot en Bottom.</p>
<div class="sprites-grid">{sprites_wally}</div>

<h2 class="subseccion">Rival Común</h2>
<p>Visto de frente. Mismo set de animaciones pero desde la perspectiva opuesta.
Comparte el código de IA con Kirikú, diferenciado únicamente por el AIProfile.</p>
<div class="sprites-grid">{sprites_rival}</div>

<h2 class="subseccion">Especificaciones técnicas de sprites</h2>
<table>
  <tr><th>Parámetro</th><th>Valor</th></tr>
  <tr><td>Dimensión de celda</td><td>128 × 128 px</td></tr>
  <tr><td>Formato</td><td>PNG con canal alfa (fondo transparente)</td></tr>
  <tr><td>Pivot</td><td>Bottom (pies en la base)</td></tr>
  <tr><td>Filter Mode</td><td>Point (evita blur en pixel art)</td></tr>
  <tr><td>Compression</td><td>None</td></tr>
  <tr><td>Pixels Per Unit</td><td>128</td></tr>
  <tr><td>Sprite Mode</td><td>Multiple (spritesheet)</td></tr>
</table>

<!-- ─── 7. CRONOGRAMA ───────────────────────────────────────────────────── -->
<h1 class="seccion">7. Cronograma de Desarrollo</h1>

<div class="semana-card">
  <h4>Semana 1 — Setup y configuración <span class="badge-ok">✅</span></h4>
  <ul>
    <li>Instalación de Unity 6 LTS con plantilla Universal 2D (URP)</li>
    <li>Configuración del repositorio Git con Git LFS para assets binarios</li>
    <li>Estructura de carpetas del proyecto</li>
    <li>Selección de assets de arte e instalación de paquetes (Input System, TextMeshPro)</li>
  </ul>
</div>

<div class="semana-card">
  <h4>Semana 2 — Combate gris jugable <span class="badge-ok">✅</span></h4>
  <ul>
    <li>Scripts base: Health, Stamina, DodgePosition</li>
    <li>CombatController con ciclo Windup → Active → Recovery</li>
    <li>PlayerInputHandler con teclado (J/K/L/WASD/Espacio)</li>
    <li>Wally golpea a una cápsula, le quita vida, hay KO</li>
  </ul>
</div>

<div class="semana-card">
  <h4>Semana 3 — Sprites y Animator <span class="badge-ok">✅</span></h4>
  <ul>
    <li>Importación y configuración de sprites de Wally y el Rival</li>
    <li>Configuración del Animator Controller con todos los estados y triggers</li>
    <li>AttackData como ScriptableObject con triggers de animación</li>
    <li>El juego se ve como un videojuego de boxeo real</li>
  </ul>
</div>

<div class="semana-card">
  <h4>Semana 4 — IA con máquina de estados finita <span class="badge-ok">✅</span></h4>
  <ul>
    <li>RivalAI con ciclo Telegraph → Attack → Recovery</li>
    <li>AIProfile como ScriptableObject: telegraphDuration, cooldowns, lista de ataques</li>
    <li>Indicador visual "!" durante el estado Telegraph</li>
    <li>El rival pelea solo con aviso visible antes de cada golpe</li>
  </ul>
</div>

<div class="semana-card">
  <h4>Semana 5 — HUD, rounds y menús <span class="badge-ok">✅</span></h4>
  <ul>
    <li>HUDController con barras de vida (Wally y rival), stamina y temporizador</li>
    <li>FightManager como árbitro: detecta KO y tiempo agotado</li>
    <li>Pantalla de resultado con botones Reintentar y Menú</li>
    <li>Ciclo completo: Splash → Menú → Combate → Resultado</li>
  </ul>
</div>

<div class="semana-card">
  <h4>Semana 6 — Audio, historia y guardado <span class="badge-ok">✅</span></h4>
  <ul>
    <li>AudioManager con pool de 8 AudioSources para SFX simultáneos</li>
    <li>AudioLibrary ScriptableObject con 11 clips de SFX asignados</li>
    <li>Sistema de historia: StoryPanel + StoryPlayer con efecto Ken Burns y máquina de escribir</li>
    <li>SaveManager con JsonUtility: guardar victorias, derrotas y nivel actual</li>
    <li>Escenas historia.unity, splash.unity y menu.unity completas</li>
  </ul>
</div>

<div class="semana-card">
  <h4>Semana 7 — Polish y build final <span class="badge-ok">✅</span></h4>
  <ul>
    <li>Animaciones Hit, Victory y Defeat del Rival Común creadas en el editor</li>
    <li>Corrección de bugs: referencias del inspector, transiciones del Animator</li>
    <li>Actualización de la narrativa (El Camino del Campeón) en los 3 paneles</li>
    <li>Build final Windows x86_64</li>
    <li>Documentación técnica completa</li>
  </ul>
</div>

<!-- ─── 8. SISTEMA DE AUDIO ─────────────────────────────────────────────── -->
<h1 class="seccion">8. Sistema de Audio</h1>
<table>
  <tr><th>Clip</th><th>Archivo</th><th>Tipo</th><th>Estado</th></tr>
  <tr><td>Jab</td><td>sfx_jab.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Recto</td><td>sfx_straight.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Gancho</td><td>sfx_hook.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Impacto directo</td><td>sfx_impacto.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Impacto bloqueado</td><td>sfx_bloqueo.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Golpe al aire</td><td>sfx_whiff.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Daño recibido</td><td>sfx_danio.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Caída/Knockdown</td><td>sfx_caida.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Campana de round</td><td>sfx_campana.ogg</td><td>SFX Combate</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Clic de botón</td><td>sfx_click.ogg</td><td>SFX UI</td><td class="badge-ok">✅ Asignado</td></tr>
  <tr><td>Hover de botón</td><td>sfx_hover.ogg</td><td>SFX UI</td><td class="badge-ok">✅ Asignado</td></tr>
</table>

<!-- ─── 9. CÓDIGO FUENTE COMPLETO ────────────────────────────────────────── -->
<h1 class="seccion">9. Código Fuente Completo</h1>
<p>El proyecto contiene <strong>18 scripts en C#</strong> organizados por módulo funcional.
Cada script tiene una única responsabilidad y máximo 150 líneas.</p>

{secciones_codigo}

<!-- ─── 10. CONCLUSIONES ─────────────────────────────────────────────────── -->
<h1 class="seccion">10. Conclusiones</h1>
<p>
  El desarrollo de <strong>ONE PUNCH DELUXE</strong> en 7 semanas demostró que es posible
  construir un videojuego completo y jugable con un equipo pequeño aplicando principios de
  ingeniería de software desde el primer día:
</p>
<ul style="color:#ccc; padding-left:25px; margin:15px 0;">
  <li style="margin:8px 0">La separación entre datos (ScriptableObjects) y lógica (scripts) permitió balancear la dificultad sin modificar código.</li>
  <li style="margin:8px 0">El patrón Observer desacoplò el HUD del sistema de combate, facilitando cambios visuales sin afectar la lógica.</li>
  <li style="margin:8px 0">La decisión de perspectiva tipo Punch-Out!! eliminó semanas de trabajo al descartar movimiento horizontal y cámara dinámica.</li>
  <li style="margin:8px 0">El pool de AudioSources garantiza que dos golpes simultáneos no se interrumpan entre sí.</li>
  <li style="margin:8px 0">Git LFS permitió versionar assets binarios grandes (sprites, audio) sin degradar el rendimiento del repositorio.</li>
</ul>
<p>
  La decisión más importante del proyecto fue priorizar la simplicidad sobre la elegancia:
  tres patrones de diseño bien aplicados valen más que ocho patrones mal justificados.
  Cada línea de código puede ser explicada y defendida en la sustentación oral.
</p>

<br><br>
<p style="text-align:center; color:#555; font-size:0.85em;">
  ONE PUNCH DELUXE — Documentación Técnica — Agosto 2026<br>
  Generado automáticamente desde el código fuente del proyecto.
</p>

</div><!-- /contenido -->
</body>
</html>"""

# ── Guardar ────────────────────────────────────────────────────────────────────
with open(SALIDA, "w", encoding="utf-8") as f:
    f.write(html)

print(f"\nDocumento generado exitosamente:")
print(f"   {SALIDA}")
print(f"\nInstrucciones para convertir a PDF:")
print(f"   1. Abre el archivo en Google Chrome o Microsoft Edge")
print(f"   2. Presiona Ctrl+P")
print(f"   3. Destino: 'Guardar como PDF'")
print(f"   4. Diseno: Horizontal (landscape)")
print(f"   5. Clic en Guardar")
