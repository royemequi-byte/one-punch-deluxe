using UnityEngine;

/// <summary>
/// Administra la energía (stamina) de un peleador.
/// Baja al atacar y sube sola con el tiempo.
/// Sin stamina no se puede atacar.
/// </summary>
public class Stamina : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int maxStamina = 100;
    [SerializeField] private float regenPerSecond = 15f;   // cuánta stamina recupera por segundo

    private float currentStamina;

    // El HUD se suscribe a este evento para actualizar la barra
    public event System.Action<float, float> OnStaminaChanged;  // (actual, máxima)

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    // Verdadero si hay suficiente stamina para usar (se consulta antes de atacar)
    public bool HasStamina(float cost) => currentStamina >= cost;

    private void Start()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void Update()
    {
        // Recuperar stamina con el tiempo si no está al máximo
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenPerSecond * Time.deltaTime);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    /// <summary>
    /// Resta stamina al ejecutar un ataque o bloqueo.
    /// Devuelve false si no había suficiente stamina.
    /// </summary>
    public bool UseStamina(float cost)
    {
        if (!HasStamina(cost)) return false;

        currentStamina -= cost;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        return true;
    }

    /// <summary>
    /// Restaura la stamina al máximo. Útil al iniciar una nueva pelea.
    /// </summary>
    public void ResetStamina()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}
