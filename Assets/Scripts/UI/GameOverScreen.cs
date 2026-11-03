using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pantalla final estilo arcade (KOF): se oscurece el ring, entra una banda roja,
/// el título revienta en pantalla, suena la voz del locutor y aparecen dos botones.
/// Se construye 100% por código para no tocar las escenas a mano.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    private const string EscenaMenu = "menu";
    private const float AnchoBoton = 520f;
    private const float AltoBoton  = 104f;

    private static GameOverScreen activa;

    // ── Estado ───────────────────────────────────────────────────
    private bool  wallyGana;
    private int   roundsWally;
    private int   roundsRival;
    private string escenaReintento;

    private CanvasGroup grupo;
    private Image overlay;
    private Image vineta;
    private RectTransform banda;
    private RectTransform filo;
    private RectTransform cajaTitulo;
    private RectTransform cajaBotones;

    private TextMeshProUGUI titulo;
    private TextMeshProUGUI subtitulo;
    private TextMeshProUGUI marcador;
    private TextMeshProUGUI pista;

    private readonly RectTransform[] botonesRect = new RectTransform[2];
    private readonly Image[]         botonesFondo = new Image[2];
    private readonly TextMeshProUGUI[] botonesTexto = new TextMeshProUGUI[2];
    private readonly Outline[]       botonesBrillo = new Outline[2];

    private int   seleccion;
    private float navCooldown;
    private bool  listo;
    private bool  saliendo;
    private float tiempo;

    private static Sprite spriteRedondeado;
    private static Sprite spriteVineta;

    // ── API ──────────────────────────────────────────────────────

    /// <summary>Muestra la pantalla final. Llamar una sola vez al terminar la pelea.</summary>
    public static void Show(bool gana, int wallyWins, int rivalWins, string escena = null)
    {
        if (activa != null) return;

        GameObject go = new GameObject("GameOverScreen");
        activa = go.AddComponent<GameOverScreen>();
        activa.Iniciar(gana, wallyWins, rivalWins, escena);
    }

    private void Iniciar(bool gana, int wallyWins, int rivalWins, string escena)
    {
        wallyGana       = gana;
        roundsWally     = wallyWins;
        roundsRival     = rivalWins;
        escenaReintento = string.IsNullOrEmpty(escena) ? SceneManager.GetActiveScene().name : escena;

        Construir();
        OcultarHudDePelea();
        StartCoroutine(SecuenciaEntrada());
    }

    // ── CONSTRUCCIÓN ─────────────────────────────────────────────

    private void Construir()
    {
        GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 600;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        grupo = canvasGO.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = true;

        // 1. Fondo oscuro
        RectTransform fondoRT = Rect("Fondo", canvasGO.transform);
        Estirar(fondoRT, 0f, 0f, 0f, 0f);
        overlay = fondoRT.gameObject.AddComponent<Image>();
        overlay.color = new Color(0.015f, 0.015f, 0.02f, 0.93f);

        // 2. Viñeta para que el centro respire
        RectTransform vinetaRT = Rect("Vineta", canvasGO.transform);
        Estirar(vinetaRT, 0f, 0f, 0f, 0f);
        vineta = vinetaRT.gameObject.AddComponent<Image>();
        vineta.sprite = SpriteVineta();
        vineta.color  = new Color(1f, 1f, 1f, 1f);
        vineta.raycastTarget = false;

        // 3. Banda de energía roja (entra deslizándose)
        Color rojo  = new Color(0.72f, 0.04f, 0.04f, 0.85f);
        Color dorado = new Color(1f, 0.78f, 0.15f, 0.95f);

        banda = Rect("Banda", canvasGO.transform);
        banda.anchorMin = banda.anchorMax = new Vector2(0.5f, 0.5f);
        banda.sizeDelta = new Vector2(2400f, 196f);
        banda.anchoredPosition = new Vector2(0f, 104f);
        banda.localRotation = Quaternion.Euler(0f, 0f, -4.5f);
        Image bandaImg = banda.gameObject.AddComponent<Image>();
        bandaImg.color = rojo;
        bandaImg.raycastTarget = false;

        filo = Rect("Filo", banda);
        filo.anchorMin = new Vector2(0f, 0f);
        filo.anchorMax = new Vector2(1f, 0f);
        filo.sizeDelta = new Vector2(0f, 7f);
        filo.anchoredPosition = Vector2.zero;
        Image filoImg = filo.gameObject.AddComponent<Image>();
        filoImg.color = dorado;
        filoImg.raycastTarget = false;

        // 4. Título
        cajaTitulo = Rect("Titulo", canvasGO.transform);
        cajaTitulo.anchorMin = cajaTitulo.anchorMax = new Vector2(0.5f, 0.5f);
        cajaTitulo.sizeDelta = new Vector2(1700f, 260f);
        cajaTitulo.anchoredPosition = new Vector2(0f, 96f);

        titulo = Texto(cajaTitulo, wallyGana ? "YOU WIN!" : "GAME OVER", 168f,
                       wallyGana ? new Color(1f, 0.83f, 0.16f) : new Color(1f, 0.96f, 0.93f));
        titulo.characterSpacing = 10f;
        titulo.outlineWidth = 0.30f;
        titulo.outlineColor = wallyGana ? new Color(0f, 0f, 0f, 1f) : new Color(0.38f, 0f, 0f, 1f);

        // 5. Subtítulo + marcador
        subtitulo = Texto(canvasGO.transform, "", 40f, new Color(0.96f, 0.96f, 0.96f));
        Posicionar(subtitulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1400f, 70f), new Vector2(0f, -62f));
        subtitulo.text = wallyGana
            ? "KIRIKÚ HA CAÍDO — EL RING ES DE WALLY"
            : "WALLY HA CAÍDO — KIRIKÚ SE QUEDA CON EL RING";

        marcador = Texto(canvasGO.transform, "", 86f, new Color(0.88f, 0.88f, 0.88f));
        Posicionar(marcador.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(900f, 160f), new Vector2(0f, -172f));
        marcador.text = roundsWally + "  —  " + roundsRival
                      + "\n<size=30><color=#9A9A9A>ROUNDS GANADOS</color></size>";

        // 6. Botones
        cajaBotones = Rect("Botones", canvasGO.transform);
        cajaBotones.anchorMin = cajaBotones.anchorMax = new Vector2(0.5f, 0f);
        cajaBotones.sizeDelta = new Vector2(1200f, AltoBoton);
        cajaBotones.anchoredPosition = new Vector2(0f, 218f);

        Color colorA = wallyGana ? new Color(0.12f, 0.42f, 0.16f) : new Color(0.55f, 0.06f, 0.06f);
        Color colorB = new Color(0.13f, 0.13f, 0.16f);

        CrearBoton(0, wallyGana ? "REVANCHA" : "REINTENTAR", colorA, -320f);
        CrearBoton(1, "MENÚ PRINCIPAL", colorB, 320f);

        // 7. Pista de controles
        pista = Texto(canvasGO.transform, "", 28f, new Color(0.78f, 0.78f, 0.78f));
        Posicionar(pista.rectTransform, new Vector2(0.5f, 0f), new Vector2(1400f, 44f), new Vector2(0f, 122f));
        pista.text = "◄ ►  ELEGIR        ENTER / CRUZ  ACEPTAR        ESC / CÍRCULO  MENÚ";

        seleccion = 0;
        ActualizarSeleccion();
    }

    private void CrearBoton(int indice, string texto, Color color, float x)
    {
        RectTransform rt = Rect("Boton" + indice, cajaBotones);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(AnchoBoton, AltoBoton);
        rt.anchoredPosition = new Vector2(x, 0f);

        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = SpriteRedondeado();
        img.type   = Image.Type.Sliced;
        img.color  = color;

        Outline brillo = rt.gameObject.AddComponent<Outline>();
        brillo.effectColor = new Color(1f, 0.85f, 0.2f, 0f);
        brillo.effectDistance = new Vector2(5f, -5f);

        TextMeshProUGUI label = Texto(rt, texto, 44f, Color.white);
        label.characterSpacing = 3f;

        botonesRect[indice]   = rt;
        botonesFondo[indice]  = img;
        botonesTexto[indice]  = label;
        botonesBrillo[indice] = brillo;
    }

    /// <summary>Apaga el HUD del combate para que no se transparente detrás.</summary>
    private void OcultarHudDePelea()
    {
        Canvas[] lienzos = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in lienzos)
        {
            if (c == null || c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            if (c.sortingOrder >= 600) continue;          // es esta misma pantalla
            if (c.transform.IsChildOf(transform)) continue;

            CanvasGroup g = c.GetComponent<CanvasGroup>();
            if (g == null) g = c.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            g.blocksRaycasts = false;
            g.interactable = false;
        }
    }

    // ── ANIMACIÓN DE ENTRADA ─────────────────────────────────────

    private IEnumerator SecuenciaEntrada()
    {
        // Voz del locutor: K.O. ya sonó; aquí va el veredicto
        yield return new WaitForSecondsRealtime(0.12f);
        AnnouncerVoice.Hablar(wallyGana ? AnnouncerVoice.YouWin : AnnouncerVoice.GameOver, 1f);

        // 1. Oscurecer
        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.SmoothStep(0f, 1f, t / 0.45f);
            yield return null;
        }
        grupo.alpha = 1f;

        // 2. Banda deslizante
        float xFinal = banda.anchoredPosition.x;
        banda.anchoredPosition = new Vector2(-2000f, banda.anchoredPosition.y);
        banda.localScale = new Vector3(0.2f, 1f, 1f);

        t = 0f;
        while (t < 0.38f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.38f);
            banda.anchoredPosition = new Vector2(Mathf.Lerp(-2000f, xFinal, p), banda.anchoredPosition.y);
            banda.localScale = new Vector3(Mathf.Lerp(0.2f, 1f, p), 1f, 1f);
            yield return null;
        }
        banda.anchoredPosition = new Vector2(xFinal, banda.anchoredPosition.y);
        banda.localScale = Vector3.one;

        // 3. Título: golpe de escala con rebote + temblor
        Vector2 centro = cajaTitulo.anchoredPosition;
        t = 0f;
        while (t < 0.55f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / 0.55f);
            float escala = 1f + 1.6f * (1f - Mathf.SmoothStep(0f, 1f, p));
            cajaTitulo.localScale = Vector3.one * escala;

            float sacudida = (1f - p) * 26f;
            cajaTitulo.anchoredPosition = centro + new Vector2(
                Random.Range(-sacudida, sacudida), Random.Range(-sacudida, sacudida));

            yield return null;
        }
        cajaTitulo.localScale = Vector3.one;
        cajaTitulo.anchoredPosition = centro;

        // 4. Textos secundarios
        subtitulo.alpha = 0f;
        marcador.alpha  = 0f;
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / 0.3f);
            subtitulo.alpha = p;
            marcador.alpha  = p;
            yield return null;
        }
        subtitulo.alpha = 1f;
        marcador.alpha  = 1f;

        // 5. Botones suben desde abajo
        Vector2 cajaBase = cajaBotones.anchoredPosition;
        pista.alpha = 0f;
        t = 0f;
        while (t < 0.35f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.35f);
            cajaBotones.anchoredPosition = new Vector2(cajaBase.x, Mathf.Lerp(cajaBase.y - 120f, cajaBase.y, p));
            pista.alpha = p;
            yield return null;
        }
        cajaBotones.anchoredPosition = cajaBase;
        pista.alpha = 1f;

        grupo.interactable = true;
        listo = true;
    }

    // ── BUCLE ────────────────────────────────────────────────────

    private void Update()
    {
        if (!listo || saliendo) return;

        tiempo += Time.unscaledDeltaTime;
        navCooldown -= Time.unscaledDeltaTime;

        // Latido suave del título para que la pantalla no se sienta muerta
        if (cajaTitulo != null)
        {
            float latido = 1f + Mathf.Sin(tiempo * 2.2f) * 0.012f;
            cajaTitulo.localScale = Vector3.Lerp(cajaTitulo.localScale, Vector3.one * latido, 8f * Time.unscaledDeltaTime);
        }

        for (int i = 0; i < botonesRect.Length; i++)
        {
            if (botonesRect[i] == null) continue;
            float objetivo = (i == seleccion) ? 1.07f : 1f;
            botonesRect[i].localScale = Vector3.Lerp(botonesRect[i].localScale,
                Vector3.one * objetivo, 10f * Time.unscaledDeltaTime);

            float brilloObjetivo = (i == seleccion) ? 0.85f : 0f;
            Color c = botonesBrillo[i].effectColor;
            c.a = Mathf.Lerp(c.a, brilloObjetivo, 10f * Time.unscaledDeltaTime);
            botonesBrillo[i].effectColor = c;

            Color txt = botonesTexto[i].color;
            float objetivoTxt = (i == seleccion) ? 1f : 0.72f;
            txt.a = Mathf.Lerp(txt.a, objetivoTxt, 10f * Time.unscaledDeltaTime);
            botonesTexto[i].color = txt;
        }

        LeerEntrada();
    }

    private void LeerEntrada()
    {
        Keyboard kb = Keyboard.current;
        Mouse raton = Mouse.current;

        // ── Teclado ──
        if (kb != null)
        {
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                Confirmar(seleccion);

            if (kb.rKey.wasPressedThisFrame) Confirmar(0);
            if (kb.escapeKey.wasPressedThisFrame) Confirmar(1);

            if (navCooldown <= 0f)
            {
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) { Mover(-1); navCooldown = 0.18f; }
                if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) { Mover(1); navCooldown = 0.18f; }
            }
        }

        // ── Mando (PlayStation, Xbox o genérico) ──
        if (PadInput.HayMando)
        {
            PadInput.RegistrarDispositivos();

            if (PadInput.ConfirmarPulsado || PadInput.InicioPulsado) Confirmar(seleccion);
            if (PadInput.CancelarPulsado) Confirmar(1);

            if (navCooldown <= 0f)
            {
                if (PadInput.NavIzquierda) { Mover(-1); navCooldown = 0.22f; }
                if (PadInput.NavDerecha)   { Mover(1);  navCooldown = 0.22f; }
            }
        }

        // ── Ratón ──
        if (raton != null)
        {
            Vector2 pos = raton.position.ReadValue();
            for (int i = 0; i < botonesRect.Length; i++)
            {
                if (botonesRect[i] == null) continue;
                bool dentro = RectTransformUtility.RectangleContainsScreenPoint(botonesRect[i], pos, null);

                if (dentro && seleccion != i && navCooldown <= 0f)
                {
                    seleccion = i;
                    navCooldown = 0.12f;
                }

                if (dentro && raton.leftButton.wasPressedThisFrame)
                {
                    Confirmar(i);
                    return;
                }
            }
        }
    }

    private void Mover(int delta)
    {
        seleccion = (seleccion + delta + botonesRect.Length) % botonesRect.Length;
        AudioManager.Instance?.PlaySFX(AudioManager.Instance?.Library?.sfxBotonHover, 0.7f, 1.1f);
    }

    private void Confirmar(int indice)
    {
        if (saliendo) return;
        saliendo = true;
        AudioManager.Instance?.PlaySFX(AudioManager.Instance?.Library?.sfxBotonClick, 0.9f);
        StartCoroutine(Salir(indice == 0 ? escenaReintento : EscenaMenu));
    }

    private IEnumerator Salir(string escena)
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }

        activa = null;
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(escena)) SceneManager.LoadScene(escena);
    }

    private void OnDestroy()
    {
        if (activa == this) activa = null;
        Time.timeScale = 1f;
    }

    // ── AYUDAS DE UI ─────────────────────────────────────────────

    private void ActualizarSeleccion()
    {
        for (int i = 0; i < botonesBrillo.Length; i++)
        {
            if (botonesBrillo[i] == null) continue;
            Color c = botonesBrillo[i].effectColor;
            c.a = (i == seleccion) ? 0.85f : 0f;
            botonesBrillo[i].effectColor = c;
        }
    }

    private static RectTransform Rect(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    private static void Estirar(RectTransform rt, float izq, float der, float abajo, float arriba)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(izq, abajo);
        rt.offsetMax = new Vector2(-der, -arriba);
    }

    private static void Posicionar(RectTransform rt, Vector2 ancla, Vector2 tamano, Vector2 posicion)
    {
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.sizeDelta = tamano;
        rt.anchoredPosition = posicion;
    }

    private static TextMeshProUGUI Texto(Transform padre, string contenido, float tamano, Color color)
    {
        RectTransform rt = Rect("Texto", padre);
        Estirar(rt, 0f, 0f, 0f, 0f);

        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        tmp.text = contenido;
        tmp.fontSize = tamano;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        tmp.color = color;

        if (tamano >= 80f)
        {
            tmp.outlineWidth = 0.22f;
            tmp.outlineColor = new Color(0f, 0f, 0f, 1f);
        }

        return tmp;
    }

    // ── TEXTURAS PROCEDURALES ────────────────────────────────────

    private static Sprite SpriteRedondeado()
    {
        if (spriteRedondeado != null) return spriteRedondeado;

        const int size = 64;
        const int radio = 22;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radio - x - 0.5f, x + 0.5f - (size - radio), 0f);
                float dy = Mathf.Max(radio - y - 0.5f, y + 0.5f - (size - radio), 0f);
                float d  = Mathf.Sqrt(dx * dx + dy * dy);
                float a  = Mathf.Clamp01(radio + 0.5f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        spriteRedondeado = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f),
                                         100f, 0, SpriteMeshType.FullRect,
                                         new Vector4(radio, radio, radio, radio));
        return spriteRedondeado;
    }

    private static Sprite SpriteVineta()
    {
        if (spriteVineta != null) return spriteVineta;

        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x / (size - 1f)) * 2f - 1f;
                float ny = (y / (size - 1f)) * 2f - 1f;
                float d  = Mathf.Sqrt(nx * nx + ny * ny) / 1.4142f;
                float a  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.30f) / 0.70f)) * 0.92f;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
            }
        }
        tex.Apply();

        spriteVineta = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        return spriteVineta;
    }
}
