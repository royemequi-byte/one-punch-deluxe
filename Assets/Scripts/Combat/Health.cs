using UnityEngine;

/// <summary>
/// Administra la vida de un peleador.
/// Reduce vida al recibir daño y avisa cuando cambia o llega a cero.
/// Se usa tanto en Wally como en los rivales.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int maxHealth = 100;

    // Vida actual — [SerializeField] solo para verla en el Inspector durante Play
    [SerializeField] private int currentHealth;

    // Otros scripts se suscriben a estos eventos para enterarse de los cambios
    public event System.Action<int, int> OnHealthChanged;  // (vidaActual, vidaMaxima)
    public event System.Action OnDamage;
    public event System.Action OnDeath;

    // Propiedad de solo lectura para consultar la vida desde afuera
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        // Al iniciar, la vida empieza al máximo
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Reduce la vida por la cantidad de daño recibida.
    /// Si llega a cero dispara el evento OnDeath.
    /// </summary>
    public void TakeDamage(int damage)
    {
        // No procesar daño si ya está KO
        if (IsDead) return;

        // Clamp para que nunca baje de cero
        currentHealth = Mathf.Max(0, currentHealth - damage);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth > 0)
            OnDamage?.Invoke();

        if (currentHealth <= 0)
            OnDeath?.Invoke();
    }

    /// <summary>
    /// Restaura la vida al máximo. Útil al iniciar una nueva pelea.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
