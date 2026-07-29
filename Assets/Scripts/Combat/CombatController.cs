using UnityEngine;
using System.Collections;

/// <summary>
/// Ejecuta los ataques de un peleador.
/// Maneja el ciclo Windup → Active → Recovery de cada golpe.
/// Durante la fase Active aplica daño al rival si no está esquivando ni bloqueando.
/// </summary>
public class CombatController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Stamina stamina;
    [SerializeField] private Animator animator;

    [Header("Objetivo")]
    [SerializeField] private Health targetHealth;

    [Header("Bloqueo")]
    [Tooltip("Multiplicador de daño cuando el objetivo está bloqueando (0.25 = 75% reducción).")]
    [SerializeField] private float blockDamageMultiplier = 0.25f;

    // Posición de esquive actual del objetivo (la informa PlayerInputHandler o RivalAI)
    private DodgePosition targetDodge = DodgePosition.Neutral;

    // Si el objetivo está bloqueando activamente
    private bool targetIsBlocking = false;

    // Verdadero mientras se está ejecutando un golpe
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    /// <summary>
    /// Asigna el objetivo que recibirá el daño. Lo llama FightManager.
    /// </summary>
    public void SetTarget(Health target)
    {
        targetHealth = target;
    }

    /// <summary>
    /// Informa la posición de esquive del objetivo.
    /// Lo llama PlayerInputHandler (para el rival) o RivalAI (para Wally).
    /// </summary>
    public void SetTargetDodge(DodgePosition dodge)
    {
        targetDodge = dodge;
    }

    /// <summary>
    /// Informa si el objetivo está bloqueando.
    /// Un bloqueo activo no anula el golpe — lo reduce a un 25% del daño.
    /// </summary>
    public void SetTargetBlocking(bool blocking)
    {
        targetIsBlocking = blocking;
    }

    /// <summary>
    /// Intenta ejecutar un ataque. Si no hay stamina o ya está atacando, no hace nada.
    /// </summary>
    public void TryAttack(AttackData attack)
    {
        if (isAttacking) return;
        if (attack == null) return;
        if (stamina == null) return;
        if (!stamina.UseStamina(attack.staminaCost)) return;

        StartCoroutine(ExecuteAttack(attack));
    }

    private IEnumerator ExecuteAttack(AttackData attack)
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger(attack.animationTrigger);

        // Fase Windup: preparación, aún no golpea
        yield return new WaitForSeconds(attack.windupTime);

        // Fase Active: ventana donde el golpe puede impactar
        bool impacta = EvaluarImpacto(attack);
        if (impacta && targetHealth != null)
        {
            // Si el objetivo bloquea, solo pasa el 25% del daño
            int danoFinal = targetIsBlocking
                ? Mathf.Max(1, Mathf.RoundToInt(attack.damage * blockDamageMultiplier))
                : attack.damage;

            string estado = targetIsBlocking ? "(BLOQUEADO)" : "";
            Debug.Log($"[Combate] {attack.attackName} impacta {targetHealth.name} → {danoFinal} daño {estado}");

            targetHealth.TakeDamage(danoFinal);
        }

        yield return new WaitForSeconds(attack.activeTime);

        // Fase Recovery: vulnerable, no puede hacer nada
        yield return new WaitForSeconds(attack.recoveryTime);

        isAttacking = false;
    }

    /// <summary>
    /// El golpe impacta si el objetivo no está muerto ni en posición correcta de esquive.
    /// </summary>
    private bool EvaluarImpacto(AttackData attack)
    {
        if (targetHealth == null || targetHealth.IsDead) return false;

        foreach (DodgePosition esquive in attack.dodgesThatAvoid)
        {
            if (targetDodge == esquive)
            {
                Debug.Log($"[Combate] {attack.attackName} esquivado con {esquive}");
                return false;
            }
        }

        return true;
    }
}
