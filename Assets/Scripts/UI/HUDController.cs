using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// HUD estilo KOF: barras de vida con ghost amarillo, gemas de ronda, timer central
/// y anunciador de carteles. FightManager llama IniciarRound(), IniciarTimer() y MostrarResultado().
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Combate")]
    [SerializeField] private Health  wallyHealth;
    [SerializeField] private Health  rivalHealth;
    [SerializeField] private Stamina wallyStamina;
    [SerializeField] private Stamina rivalStamina;

    [Header("Barras Wally — izquierda (fillOrigin=Left)")]
    [SerializeField] private Image wallyBar;
    [SerializeField] private Image wallyBarGhost;

    [Header("Barras Rival — derecha (fillOrigin=Right)")]
    [SerializeField] private Image rivalBar;
    [SerializeField] private Image rivalBarGhost;

    [Header("Stamina")]
    [SerializeField] private Image wallyStaminaBar;

    [Header("Gemas de ronda (2 por jugador)")]
    [SerializeField] private Image[] wallyGemas = new Image[2];
    [SerializeField] private Image[] rivalGemas = new Image[2];

    [Header("Timer")]
    [SerializeField] private TMP_Text timerText;

    [Header("Anunciador")]
    [SerializeField] private AnunciadorRound anunciador;

    [Header("Panel resultado final")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text   resultText;
    [SerializeField] private Button     botonReintentar;
    [SerializeField] private Button     botonMenu;

    [Header("Config")]
    [SerializeField] private float roundDuration    = 99f;
    [SerializeField] private float ghostSpeed       = 0.35f;
    [SerializeField] private string escenaMenu      = "menu";

    // FightManager se suscribe para saber cuando el tiempo se acaba
    public event System.Action OnTiempoAgotado;

    /// <summary>FightManager espera este tiempo antes de activar la IA después del cartel.</summary>
    public float DuracionAnuncio => anunciador != null ? anunciador.DuracionTotal : 2.5f;

    private float tiempoRestante;
    private bool  timerActivo       = false;
    private bool  tiempoYaDisparado = false;
    private float wallyGhostFill    = 1f;
    private float rivalGhostFill    = 1f;

    // ── Colores KOF ──────────────────────────────────────────────
    private static readonly Color cVidaAlta   = new Color(0.08f, 0.90f, 0.12f);
    private static readonly Color cVidaMedia  = new Color(0.95f, 0.75f, 0.05f);
    private static readonly Color cVidaBaja   = new Color(0.92f, 0.10f, 0.10f);
    private static readonly Color cGhost      = new Color(0.95f, 0.85f, 0.10f);
    private static readonly Color cGemaLlena  = new Color(1.00f, 0.80f, 0.10f);
    private static readonly Color cGemaVacia  = new Color(0.18f, 0.18f, 0.18f);

    // ── Unity ────────────────────────────────────────────────────

    private void Awake()
    {
        // ── Buscar contenedores ───────────────────────────────────
        // BuscarGO tolera el espacio al inicio que deja el Inspector a veces
        GameObject wallyBarGO   = BuscarGO("WallyHealthBar");
        GameObject rivalBarGO   = BuscarGO("RivalHealthBar");
        GameObject staminaBarGO = BuscarGO("WallyStaminaBar");
        GameObject timerGO      = BuscarGO("TimerText");

        // ── Conectar referencias de UI ────────────────────────────
        if (wallyBar      == null && wallyBarGO   != null)
            wallyBar      = wallyBarGO.GetComponent<Image>()   ?? wallyBarGO.GetComponentInChildren<Image>();
        if (wallyBarGhost == null) wallyBarGhost  = ImgHijo(BuscarGO("WallyHealthBarGhost"));
        if (rivalBar      == null && rivalBarGO   != null)
            rivalBar      = rivalBarGO.GetComponent<Image>()   ?? rivalBarGO.GetComponentInChildren<Image>();
        if (rivalBarGhost == null) rivalBarGhost  = ImgHijo(BuscarGO("RivalHealthBarGhost"));
        if (wallyStaminaBar == null && staminaBarGO != null)
            wallyStaminaBar = staminaBarGO.GetComponent<Image>() ?? staminaBarGO.GetComponentInChildren<Image>();
        if (timerText     == null && timerGO      != null)
            timerText     = timerGO.GetComponent<TMP_Text>();

        // ── Conectar Health / Stamina de los peleadores ───────────
        if (wallyHealth == null)
        {
            var pip = Object.FindFirstObjectByType<PlayerInputHandler>();
            if (pip != null) wallyHealth = pip.GetComponent<Health>();
        }
        if (rivalHealth == null)
        {
            var rai = Object.FindFirstObjectByType<RivalAI>();
            if (rai != null) rivalHealth = rai.GetComponent<Health>();
        }
        if (wallyStamina == null)
        {
            var pip = Object.FindFirstObjectByType<PlayerInputHandler>();
            if (pip != null) wallyStamina = pip.GetComponent<Stamina>();
        }
        if (rivalStamina == null)
        {
            var rai = Object.FindFirstObjectByType<RivalAI>();
            if (rai != null) rivalStamina = rai.GetComponent<Stamina>();
        }

        // Si la escena no trae anunciador ni gemas, se crean al vuelo
        if (anunciador == null) anunciador = gameObject.AddComponent<AnunciadorRound>();
        if (GetComponent<ComboCounter>() == null) gameObject.AddComponent<ComboCounter>();
        if (GetComponent<ControlsHint>() == null) gameObject.AddComponent<ControlsHint>();
        CrearGemasSiFaltan();

        // El cartel viejo "Round 1" se quedaba encima de la pelea; ahora lo
        // muestra el anunciador nuevo, así que se oculta.
        GameObject roundViejo = GameObject.Find("RoundText");
        if (roundViejo != null) roundViejo.SetActive(false);

        // Look KOF XV: barras finas, 99 al centro, GUARD y MAXIMUM abajo
        KofHudSkin skin = GetComponent<KofHudSkin>();
        if (skin == null) skin = gameObject.AddComponent<KofHudSkin>();
        skin.Init(wallyBar, rivalBar, wallyStaminaBar, timerText, wallyStamina, rivalStamina, wallyGemas, rivalGemas);

        // El layout se define en la escena (Canvas). No se reescribe en runtime:
        // así los ajustes hechos en el Inspector se respetan al dar Play.
    }

    // Encuentra un GO tolerando espacio al inicio en el nombre (bug del Inspector)
    static GameObject BuscarGO(string n)
    {
        var go = GameObject.Find(n);
        if (go != null) return go;
        go = GameObject.Find(" " + n);
        if (go != null) go.name = n; // corrige el nombre en runtime
        return go;
    }

    static Image    ImgHijo(GameObject go) => go == null ? null : (go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>());
    static TMP_Text Tmp(string n)          => BuscarGO(n)?.GetComponent<TMP_Text>();

    // Busca Image por nombre, tolera espacio al inicio (bug del Inspector)
    // y verifica también hijos (la Image puede estar en un child del contenedor)
    static Image Img(string n)
    {
        var go = BuscarGO(n);
        if (go == null)
        {
            foreach (var c in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                if (c.gameObject.name.Trim() == n) return c;
            return null;
        }
        return go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>();
    }

    private void Start()
    {
        if (wallyHealth  != null) wallyHealth.OnHealthChanged   += ActualizarVidaWally;
        if (rivalHealth  != null) rivalHealth.OnHealthChanged   += ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged += ActualizarStamina;

        botonReintentar?.onClick.AddListener(Reintentar);
        botonMenu?.onClick.AddListener(IrAlMenu);

        if (resultPanel     != null) resultPanel.SetActive(false);
        if (wallyBarGhost   != null) wallyBarGhost.color = cGhost;
        if (rivalBarGhost   != null) rivalBarGhost.color = cGhost;

        ActualizarGemas(0, 0);
    }

    private void OnDestroy()
    {
        if (wallyHealth  != null) wallyHealth.OnHealthChanged   -= ActualizarVidaWally;
        if (rivalHealth  != null) rivalHealth.OnHealthChanged   -= ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged -= ActualizarStamina;
    }

    private void Update()
    {
        // Ghost bars siguen a las barras reales con retraso
        if (wallyBarGhost != null && wallyBar != null)
        {
            wallyGhostFill = Mathf.MoveTowards(wallyGhostFill, wallyBar.fillAmount,
                                                ghostSpeed * Time.deltaTime);
            wallyBarGhost.fillAmount = wallyGhostFill;
        }
        if (rivalBarGhost != null && rivalBar != null)
        {
            rivalGhostFill = Mathf.MoveTowards(rivalGhostFill, rivalBar.fillAmount,
                                                ghostSpeed * Time.deltaTime);
            rivalBarGhost.fillAmount = rivalGhostFill;
        }

        if (!timerActivo) return;

        tiempoRestante = Mathf.Max(0f, tiempoRestante - Time.deltaTime);
        if (timerText != null)
        {
            int seg = Mathf.CeilToInt(tiempoRestante);
            timerText.text  = seg.ToString();
            timerText.color = seg <= 10 ? cVidaBaja : Color.white;
        }

        if (tiempoRestante <= 0f && !tiempoYaDisparado)
        {
            tiempoYaDisparado = true;
            timerActivo       = false;
            anunciador?.MostrarTime();
            OnTiempoAgotado?.Invoke();
        }
    }

    // ── API PÚBLICA (usada por FightManager) ──────────────────────

    /// <summary>Prepara el timer y dispara el cartel "ROUND X → FIGHT!". El timer NO arranca aún.</summary>
    public void IniciarRound(int round, int wallyWins, int rivalWins)
    {
        // KOF cuenta 99: aunque la escena tenga otro valor guardado, se respeta el 99.
        tiempoRestante    = Mathf.Min(roundDuration, 99f);
        timerActivo       = false;
        tiempoYaDisparado = false;

        // Ghost bars saltan al valor actual de las barras
        wallyGhostFill = wallyBar != null ? wallyBar.fillAmount : 1f;
        rivalGhostFill = rivalBar != null ? rivalBar.fillAmount : 1f;

        ActualizarGemas(wallyWins, rivalWins);
        anunciador?.MostrarRoundYFight(round);
    }

    /// <summary>Cartel grande de K.O. cuando un peleador cae.</summary>
    public void MostrarKO() => anunciador?.MostrarKO();

    /// <summary>Cartel del súper MAXIMUM.</summary>
    public void MostrarMaximum() => anunciador?.MostrarMaximum();

    /// <summary>Arranca el contador. FightManager llama esto justo cuando activa la IA.</summary>
    public void IniciarTimer()
    {
        timerActivo       = true;
        tiempoYaDisparado = false;
    }

    /// <summary>Pausa el timer y actualiza gemas al terminar un round.</summary>
    public void MostrarEntreRounds(int roundTerminado, int wallyWins, int rivalWins)
    {
        timerActivo = false;
        if (timerText != null) timerText.text = "--";
        ActualizarGemas(wallyWins, rivalWins);
    }

    public void OcultarEntreRounds() { /* gemas ya actualizadas, nada más que ocultar */ }

    /// <summary>Pantalla final de victoria/derrota con botones.</summary>
    public void MostrarResultado(bool wallyGana, int wallyWins, int rivalWins)
    {
        timerActivo = false;
        if (timerText != null) timerText.text = "--";
        ActualizarGemas(wallyWins, rivalWins);

        // La pantalla final se construye por código: así SIEMPRE aparece,
        // aunque la escena no traiga panel asignado en el Inspector.
        anunciador?.Ocultar();
        GameOverScreen.Show(wallyGana, wallyWins, rivalWins);
    }

    // ── PRIVADOS ──────────────────────────────────────────────────

    // Crea las dos gemas de round de cada lado si la escena no las tiene
    private void CrearGemasSiFaltan()
    {
        if (wallyGemas == null || wallyGemas.Length < 2) wallyGemas = new Image[2];
        if (rivalGemas == null || rivalGemas.Length < 2) rivalGemas = new Image[2];

        Canvas canvas = wallyBar != null ? wallyBar.canvas : Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        for (int i = 0; i < wallyGemas.Length; i++)
        {
            if (wallyGemas[i] == null) wallyGemas[i] = CrearGema(canvas.transform, true, i);
            if (rivalGemas[i] == null) rivalGemas[i] = CrearGema(canvas.transform, false, i);
        }
    }

    private Image CrearGema(Transform parent, bool left, int index)
    {
        GameObject go = new GameObject(left ? "WallyGema" + index : "RivalGema" + index);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        float x = left ? 0.045f + index * 0.022f : 0.955f - index * 0.022f;
        rt.anchorMin = new Vector2(x, 0.795f);
        rt.anchorMax = new Vector2(x, 0.795f);
        rt.sizeDelta = new Vector2(28f, 28f);
        rt.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image img = go.AddComponent<Image>();
        img.color = cGemaVacia;
        return img;
    }

    private void ActualizarGemas(int wallyWins, int rivalWins)
    {
        for (int i = 0; i < wallyGemas.Length; i++)
            if (wallyGemas[i] != null)
                wallyGemas[i].color = i < wallyWins ? cGemaLlena : cGemaVacia;

        for (int i = 0; i < rivalGemas.Length; i++)
            if (rivalGemas[i] != null)
                rivalGemas[i].color = i < rivalWins ? cGemaLlena : cGemaVacia;
    }

    private void ActualizarVidaWally(int actual, int maximo)
    {
        if (wallyBar == null) return;
        float f = (float)actual / maximo;
        wallyBar.fillAmount = f;
        wallyBar.color      = ColorVida(f);
    }

    private void ActualizarVidaRival(int actual, int maximo)
    {
        if (rivalBar == null) return;
        float f = (float)actual / maximo;
        rivalBar.fillAmount = f;
        rivalBar.color      = ColorVida(f);
    }

    private void ActualizarStamina(float actual, float maximo)
    {
        if (wallyStaminaBar != null)
            wallyStaminaBar.fillAmount = actual / maximo;
    }

    private Color ColorVida(float f)
    {
        if (f > 0.50f) return cVidaAlta;
        if (f > 0.25f) return cVidaMedia;
        return cVidaBaja;
    }

    private void Reintentar() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    private void IrAlMenu()   => SceneManager.LoadScene(escenaMenu);
}
