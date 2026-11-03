using UnityEngine;

/// <summary>
/// Medidor MAXIMUM: se llena al conectar golpes y se vacía al usar el súper.
/// Es independiente de la stamina (la barra azul de GUARD).
/// </summary>
public class PowerMeter : MonoBehaviour
{
    [Header("Medidor")]
    [SerializeField] private float maxPower = 100f;
    [SerializeField] private float gainPerHit = 14f;
    [SerializeField] private float gainWhenBlocked = 6f;

    private float power;
    private bool wasFull;
    private CombatController combat;
    private Health health;

    /// <summary>Se dispara la primera vez que el medidor llega al máximo.</summary>
    public event System.Action OnFull;

    public float Current => power;
    public float Max => maxPower;
    public bool IsFull => power >= maxPower - 0.01f;

    private void Awake()
    {
        combat = GetComponent<CombatController>();
        health = GetComponent<Health>();

        if (combat != null) combat.OnHitLanded += OnHitLanded;
        if (health != null) health.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        if (combat != null) combat.OnHitLanded -= OnHitLanded;
        if (health != null) health.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHitLanded(bool blocked)
    {
        power = Mathf.Clamp(power + (blocked ? gainWhenBlocked : gainPerHit), 0f, maxPower);

        if (IsFull && !wasFull)
        {
            wasFull = true;
            CombatFX.Spark(transform.position + Vector3.up * 1.7f, new Color(1f, 0.85f, 0.1f), 2.2f);
            OnFull?.Invoke();
        }
        else if (!IsFull)
        {
            wasFull = false;
        }
    }

    // Al empezar un round la vida vuelve al máximo: el medidor también se vacía
    private void OnHealthChanged(int current, int maximum)
    {
        if (current >= maximum)
        {
            power = 0f;
            wasFull = false;
        }
    }

    /// <summary>Gasta todo el medidor. Devuelve false si no estaba lleno.</summary>
    public bool Consume()
    {
        if (!IsFull) return false;
        power = 0f;
        wasFull = false;
        return true;
    }
}
