using UnityEngine;
using System.Collections;

/// <summary>
/// Ejecuta los ataques de un peleador.
/// Maneja el ciclo Windup → Active → Recovery de cada golpe.
/// Durante la fase Active aplica daño al rival si no está esquivando.
/// </summary>
public class CombatController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Stamina stamina;
    [SerializeField] private Animator animator;

    [Header("Objetivo")]
    [SerializeField] private Health targetHealth;  // quién recibe el daño de los golpes

    // Posición de esquive actual del objetivo (Neutro = puede recibir golpes)
    private DodgePosition targetDodge = DodgePosition.Neutral;

    // Verdadero mientras se está ejecutando un golpe
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    /// <summary>
    /// Asigna el objetivo que recibirá el daño de los golpes.
    /// Lo llama FightManager al iniciar la pelea.
    /// </summary>
    public void SetTarget(Health target)
    {
        targetHealth = target;
    }

    /// <summary>
    /// Informa al controlador la posición de esquive actual del objetivo.
    /// Lo llama el AIController o PlayerController del rival.
    /// </summary>
    public void SetTargetDodge(DodgePosition dodge)
    {
        targetDodge = dodge;
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

        // Dispara la animación del golpe
        if (animator != null)
            animator.SetTrigger(attack.animationTrigger);

        // Fase Windup: preparación, aún no golpea
        yield return new WaitForSeconds(attack.windupTime);

        // Fase Active: ventana donde el golpe puede impactar
        bool impacta = EvaluarImpacto(attack);
        if (impacta && targetHealth != null)
            targetHealth.TakeDamage(attack.damage);

        yield return new WaitForSeconds(attack.activeTime);

        // Fase Recovery: vulnerable, no puede hacer nada
        yield return new WaitForSeconds(attack.recoveryTime);

        isAttacking = false;
    }

    /// <summary>
    /// Determina si el golpe impacta según la posición de esquive del objetivo.
    /// Cada AttackData define qué posiciones lo anulan.
    /// </summary>
    private bool EvaluarImpacto(AttackData attack)
    {
        if (targetHealth == null || targetHealth.IsDead) return false;

        // Si el objetivo está en una posición que esquiva este golpe, falla
        foreach (DodgePosition esquive in attack.dodgesThatAvoid)
        {
            if (targetDodge == esquive) return false;
        }

        return true;
    }
}
