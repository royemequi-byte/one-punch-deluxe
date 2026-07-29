using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla el HUD de combate.
/// Escucha eventos de Health y Stamina para actualizar barras y lleva el timer del round.
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

    [Header("Configuración")]
    [SerializeField] private float roundDuration = 120f;

    private float tiempoRestante;
    private bool peleaActiva = false;

    private void Start()
    {
        if (wallyHealth != null) wallyHealth.OnHealthChanged += ActualizarVidaWally;
        if (rivalHealth != null) rivalHealth.OnHealthChanged += ActualizarVidaRival;
        if (wallyStamina != null) wallyStamina.OnStaminaChanged += ActualizarStamina;

        tiempoRestante = roundDuration;
        peleaActiva = true;

        if (roundText != null) roundText.text = "Round 1";
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
        if (tiempoRestante == 0f) peleaActiva = false;

        if (timerText != null)
        {
            int min = Mathf.FloorToInt(tiempoRestante / 60f);
            int seg = Mathf.FloorToInt(tiempoRestante % 60f);
            timerText.text = $"{min}:{seg:00}";
        }
    }

    /// <summary>
    /// Detiene el temporizador. Lo llama FightManager cuando hay KO.
    /// </summary>
    public void DetenerPelea()
    {
        peleaActiva = false;
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
