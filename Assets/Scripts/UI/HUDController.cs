using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Controla el HUD de combate: barras de vida/stamina, temporizador por round
/// y paneles de resultado. FightManager es quien llama a IniciarRound() cada vez.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Referencias de combate")]
    [SerializeField] private Health wallyHealth;
    [SerializeField] private Health rivalHealth;
    [SerializeField] private Stamina wallyStamina;

    [Header("Barras")]
    [SerializeField] private Image wallyHealthBar;
    [SerializeField] private Image rivalHealthBar;
    [SerializeField] private Image wallyStaminaBar;

    [Header("Textos del combate")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundText;       // muestra "Round 1", "Round 2"...

    [Header("Panel entre rounds (marcador parcial)")]
    [SerializeField] private GameObject entreRoundsPanel;
    [SerializeField] private TMP_Text entreRoundsText;

    [Header("Panel resultado final")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button botonReintentar;
    [SerializeField] private Button botonMenu;

    [Header("Configuración")]
    [SerializeField] private float roundDuration = 120f;
    [SerializeField] private string nombreEscenaMenu = "menu";

    // FightManager se suscribe para saber cuando el tiempo se acaba
    public event System.Action OnTiempoAgotado;

    private float tiempoRestante;
    private bool  timerActivo        = false;
    private bool  tiempoYaDisparado  = false;

    private void Start()
    {
        // Suscribir las barras a los eventos de cambio
        if (wallyHealth  != null) wallyHealth.OnHealthChanged  += ActualizarVidaWally;
        if (rivalHealth  != null) rivalHealth.OnHealthChanged  += ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged += ActualizarStamina;

        botonReintentar?.onClick.AddListener(Reintentar);
        botonMenu?.onClick.AddListener(IrAlMenu);

        // Ocultar paneles al inicio — FightManager los activará cuando corresponda
        if (resultPanel     != null) resultPanel.SetActive(false);
        if (entreRoundsPanel != null) entreRoundsPanel.SetActive(false);
        if (roundText       != null) roundText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (wallyHealth  != null) wallyHealth.OnHealthChanged   -= ActualizarVidaWally;
        if (rivalHealth  != null) rivalHealth.OnHealthChanged   -= ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged -= ActualizarStamina;
    }

    private void Update()
    {
        if (!timerActivo) return;

        tiempoRestante = Mathf.Max(0f, tiempoRestante - Time.deltaTime);

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(tiempoRestante).ToString();

        if (tiempoRestante <= 0f && !tiempoYaDisparado)
        {
            tiempoYaDisparado = true;
            timerActivo       = false;
            OnTiempoAgotado?.Invoke();
        }
    }

    // ── API PÚBLICA (usada por FightManager) ───────────────────

    /// <summary>
    /// Arranca el cronómetro y muestra el banner de round.
    /// Llamado por FightManager al iniciar cada round.
    /// </summary>
    public void IniciarRound(int round, int wallyWins, int rivalWins)
    {
        tiempoRestante      = roundDuration;
        timerActivo         = true;
        tiempoYaDisparado   = false;

        if (roundText != null)
        {
            roundText.gameObject.SetActive(true);
            roundText.text = $"ROUND {round}";
            StartCoroutine(OcultarRoundText());
        }
    }

    /// <summary>
    /// Muestra el marcador parcial entre rounds. Llamado por FightManager.
    /// </summary>
    public void MostrarEntreRounds(int roundTerminado, int wallyWins, int rivalWins)
    {
        timerActivo = false;
        if (timerText != null) timerText.text = "--";

        string texto = $"Round {roundTerminado} terminado\n" +
                       $"Wally  {wallyWins}  -  {rivalWins}  Rival";

        // Si hay panel dedicado lo usa; si no, usa el roundText como fallback
        if (entreRoundsPanel != null)
        {
            entreRoundsPanel.SetActive(true);
            if (entreRoundsText != null) entreRoundsText.text = texto;
        }
        else if (roundText != null)
        {
            roundText.gameObject.SetActive(true);
            roundText.text = texto;
        }
    }

    /// <summary>Oculta el panel de entre-rounds antes de empezar el siguiente.</summary>
    public void OcultarEntreRounds()
    {
        if (entreRoundsPanel != null) entreRoundsPanel.SetActive(false);
        if (roundText        != null) roundText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Muestra el resultado final con el marcador de rounds.
    /// Llamado por FightManager al terminar la pelea.
    /// </summary>
    public void MostrarResultado(bool wallyGana, int wallyWins, int rivalWins)
    {
        timerActivo = false;
        if (entreRoundsPanel != null) entreRoundsPanel.SetActive(false);
        if (resultPanel      != null) resultPanel.SetActive(true);

        if (resultText != null)
        {
            string marcador = $"{wallyWins} - {rivalWins}";
            resultText.text = wallyGana
                ? $"¡WALLY GANA!\n¡KO!  ({marcador})"
                : $"¡DERROTA!\nSigue entrenando...  ({marcador})";
        }
    }

    // ── NAVEGACIÓN ──────────────────────────────────────────────

    private void Reintentar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void IrAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    // ── CALLBACKS DE EVENTOS ────────────────────────────────────

    private IEnumerator OcultarRoundText()
    {
        yield return new WaitForSeconds(2f);
        if (roundText != null) roundText.gameObject.SetActive(false);
    }

    private void ActualizarVidaWally(int actual, int maximo)
    {
        if (wallyHealthBar != null)
            wallyHealthBar.fillAmount = (float)actual / maximo;
    }

    private void ActualizarVidaRival(int actual, int maximo)
    {
        if (rivalHealthBar != null)
            rivalHealthBar.fillAmount = (float)actual / maximo;
    }

    private void ActualizarStamina(float actual, float maximo)
    {
        if (wallyStaminaBar != null)
            wallyStaminaBar.fillAmount = actual / maximo;
    }
}
