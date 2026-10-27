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
        tiempoRestante    = roundDuration;
        timerActivo       = false;
        tiempoYaDisparado = false;

        // Ghost bars saltan al valor actual de las barras
        wallyGhostFill = wallyBar != null ? wallyBar.fillAmount : 1f;
        rivalGhostFill = rivalBar != null ? rivalBar.fillAmount : 1f;

        ActualizarGemas(wallyWins, rivalWins);
        anunciador?.MostrarRoundYFight(round);
    }

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

    /// <summary>Muestra el cartel de victoria/derrota y después el panel con botones.</summary>
    public void MostrarResultado(bool wallyGana, int wallyWins, int rivalWins)
    {
        timerActivo = false;
        if (timerText != null) timerText.text = "--";
        ActualizarGemas(wallyWins, rivalWins);

        if (wallyGana) anunciador?.MostrarWin();
        else           anunciador?.MostrarDerrota();

        StartCoroutine(MostrarPanelConDelay(wallyGana, wallyWins, rivalWins));
    }

    // ── PRIVADOS ──────────────────────────────────────────────────

    private IEnumerator MostrarPanelConDelay(bool wallyGana, int w, int r)
    {
        yield return new WaitForSeconds(2.2f);
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText  != null)
            resultText.text = wallyGana
                ? $"¡WALLY GANA!\n{w}  -  {r}"
                : $"¡DERROTA!\n{w}  -  {r}";
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
