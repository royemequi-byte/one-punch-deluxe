using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el menú principal: Nueva Partida, Continuar y Salir.
/// Continuar solo está activo si existe una partida guardada con progreso.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button botonNuevaPartida;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonSalir;

    [Header("Escenas")]
    [SerializeField] private string escenaHistoria = "historia";
    [SerializeField] private string escenaPelea = "dia 1";

    // Navegación con mando y teclado (la rueda del menú no depende del ratón)
    private Button[] botones;
    private int      seleccion;
    private float    navCooldown;
    private bool     listo;

    // Etiqueta de abajo: dice si el mando está detectado o si hay que usar teclado
    private TextMeshProUGUI etiquetaMando;
    private string textoMandoActual = "";
    private float  etiquetaVisibleHasta;

    private void Start()
    {
        // Continuar solo disponible si hay progreso guardado
        bool hayProgreso = SaveManager.Instance != null && SaveManager.Instance.Data.nivelActual > 0;
        if (botonContinuar != null)
            botonContinuar.interactable = hayProgreso;

        // Conectar botones con sonido de clic
        botonNuevaPartida?.onClick.AddListener(PlayClick);
        botonNuevaPartida?.onClick.AddListener(NuevaPartida);

        botonContinuar?.onClick.AddListener(PlayClick);
        botonContinuar?.onClick.AddListener(Continuar);

        botonSalir?.onClick.AddListener(PlayClick);
        botonSalir?.onClick.AddListener(Salir);

        // Reproducir música de menú
        AudioManager.Instance?.PlayMusic(AudioManager.Instance?.Library?.musicaMenu);

        botones = new[] { botonNuevaPartida, botonContinuar, botonSalir };
        seleccion = 0;
        listo = true;
        MarcarSeleccion();

        CrearEtiquetaMando();
    }

    /// <summary>Línea de abajo que confirma si el mando está vivo.</summary>
    private void CrearEtiquetaMando()
    {
        GameObject canvasGO = new GameObject("MenuMandoCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 520;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject go = new GameObject("TextoMando", typeof(RectTransform));
        go.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1700f, 70f);
        rt.anchoredPosition = new Vector2(0f, 40f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        tmp.fontSize = 30f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        etiquetaMando = tmp;
        ActualizarEtiquetaMando(true);
        etiquetaVisibleHasta = Time.unscaledTime + 6f;
    }

    private void ActualizarEtiquetaMando(bool forzar = false)
    {
        if (etiquetaMando == null) return;

        string texto;
        if (PadInput.HayMando)
            texto = "<color=#7CFF7C>MANDO DETECTADO:</color> " + PadInput.NombreDispositivo.ToUpper() +
                    "      D-PAD / STICK ELEGIR      \u2715 (CRUZ) ACEPTAR";
        else
            texto = "<color=#FFCC55>SIN MANDO</color>      TECLADO: \u2191 \u2193 ELEGIR      ENTER ACEPTAR";

        if (!forzar && texto == textoMandoActual) return;

        textoMandoActual = texto;
        etiquetaMando.text = texto;

        // Se muestra unos segundos y luego se desvanece: no estorba al menú
        etiquetaMando.alpha = 1f;
        etiquetaVisibleHasta = Time.unscaledTime + 6f;
    }

    private void Update()
    {
        if (!listo) return;

        navCooldown -= Time.unscaledDeltaTime;

        bool arriba = false, abajo = false, confirmar = false;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (navCooldown <= 0f)
            {
                arriba = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
                abajo  = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
            }
            confirmar = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
        }

        if (PadInput.HayMando)
        {
            PadInput.RegistrarDispositivos();
            if (navCooldown <= 0f)
            {
                arriba |= PadInput.NavArriba;
                abajo  |= PadInput.NavAbajo;
            }
            confirmar |= PadInput.ConfirmarPulsado || PadInput.InicioPulsado;
        }

        if (arriba) Mover(-1);
        if (abajo)  Mover(1);

        if (confirmar) Aceptar();

        ActualizarEtiquetaMando();

        // Desvanecer la etiqueta del mando cuando ya pasó el tiempo
        if (etiquetaMando != null && Time.unscaledTime > etiquetaVisibleHasta && etiquetaMando.alpha > 0f)
            etiquetaMando.alpha = Mathf.MoveTowards(etiquetaMando.alpha, 0f, Time.unscaledDeltaTime * 1.5f);
    }

    private void Mover(int delta)
    {
        if (botones == null) return;

        for (int intento = 0; intento < botones.Length; intento++)
        {
            seleccion = (seleccion + delta + botones.Length) % botones.Length;
            if (botones[seleccion] != null && botones[seleccion].interactable) break;
        }

        navCooldown = 0.2f;
        AudioManager.Instance?.PlaySFX(AudioManager.Instance?.Library?.sfxBotonHover, 0.7f, 1.1f);
        MarcarSeleccion();
    }

    private void Aceptar()
    {
        if (botones == null) return;
        Button b = botones[Mathf.Clamp(seleccion, 0, botones.Length - 1)];
        if (b == null || !b.interactable) return;
        b.onClick.Invoke();
    }

    private void MarcarSeleccion()
    {
        if (botones == null) return;
        Button b = botones[Mathf.Clamp(seleccion, 0, botones.Length - 1)];
        if (b == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(b.gameObject);
    }

    private void PlayClick()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance?.Library?.sfxBotonClick);
    }

    private void NuevaPartida()
    {
        SaveManager.Instance?.ReiniciarPartida();
        SceneManager.LoadScene(escenaHistoria);
    }

    private void Continuar()
    {
        // Ir directo a la pelea del nivel guardado
        SceneManager.LoadScene(escenaPelea);
    }

    private void Salir()
    {
        Application.Quit();
    }
}
