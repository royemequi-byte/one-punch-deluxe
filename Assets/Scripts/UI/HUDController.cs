using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controla el HUD de combate.
/// Escucha eventos de Health y Stamina para actualizar barras y lleva el timer del round.
/// Cuando el tiempo llega a cero dispara OnTiempoAgotado para que FightManager declare al ganador.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Referencias de combate")]
    [SerializeField] private Health wallyHealth;
    [SerializeField] private Health rivalHealth;
    [SerializeField] private Stamina wallyStamina;

    [Header("Barras (asignar en Inspector)")]
    [SerializeField] private Image wallyHealthBar;
    [SerializeField] private Image rivalHealthBar;
    [SerializeField] private Image wallyStaminaBar;

    [Header("Textos")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundText;

    [Header("Pantalla de resultado")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button botonReintentar;
    [SerializeField] private Button botonMenu;

    [Header("Configuración")]
    [SerializeField] private float roundDuration = 120f;
    [SerializeField] private string nombreEscenaMenu = "menu";

    // FightManager se suscribe a este evento para saber cuando se acaba el tiempo
    public event System.Action OnTiempoAgotado;

    private float tiempoRestante;
    private bool peleaActiva = false;
    private bool tiempoYaDisparado = false;

    private void Start()
    {
        if (wallyHealth != null) wallyHealth.OnHealthChanged += ActualizarVidaWally;
        if (rivalHealth != null) rivalHealth.OnHealthChanged += ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged += ActualizarStamina;

        tiempoRestante = roundDuration;
        peleaActiva = true;
        tiempoYaDisparado = false;

        if (roundText != null)
        {
            roundText.text = "Round 1";
            StartCoroutine(OcultarRoundText());
        }

        // Conectar botones del panel de resultado
        botonReintentar?.onClick.AddListener(Reintentar);
        botonMenu?.onClick.AddListener(IrAlMenu);

        // Ocultar panel de resultado al inicio
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (wallyHealth != null) wallyHealth.OnHealthChanged -= ActualizarVidaWally;
        if (rivalHealth != null) rivalHealth.OnHealthChanged -= ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged -= ActualizarStamina;
    }

    private void Update()
    {
        if (!peleaActiva) return;

        tiempoRestante = Mathf.Max(0f, tiempoRestante - Time.deltaTime);

        if (timerText != null)
        {
            int min = Mathf.FloorToInt(tiempoRestante / 60f);
            int seg = Mathf.FloorToInt(tiempoRestante % 60f);
            timerText.text = $"{min}:{seg:00}";
        }

        // Cuando el tiempo llega a cero, avisar una sola vez
        if (tiempoRestante <= 0f && !tiempoYaDisparado)
        {
            tiempoYaDisparado = true;
            peleaActiva = false;
            OnTiempoAgotado?.Invoke();
        }
    }

    /// <summary>
    /// Detiene el timer y muestra el resultado. Lo llama FightManager cuando hay KO o tiempo agotado.
    /// </summary>
    public void MostrarResultado(bool wallyGana)
    {
        peleaActiva = false;

        if (resultPanel != null) resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = wallyGana ? "¡WALLY GANA!\n¡KO!" : "¡KO!\nWALLY PIERDE";
    }

    private void Reintentar()
    {
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void IrAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    private System.Collections.IEnumerator OcultarRoundText()
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
