using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el comportamiento automático del rival.
/// Ciclo: espera → telegrafía → golpea → repite.
/// FightManager activa y desactiva este script al iniciar/terminar la pelea.
/// </summary>
public class RivalAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CombatController combatController;
    [SerializeField] private Animator kirikuAnimator;
    [SerializeField] private GameObject telegraphIndicator;

    [Header("Perfil de comportamiento")]
    [SerializeField] private AIProfile profile;

    // Referencia a la coroutine activa para poder detenerla limpiamente
    private Coroutine aiCoroutine;

    /// <summary>
    /// Inicia el loop de IA. Lo llama FightManager al comenzar la pelea.
    /// </summary>
    public void Activate()
    {
        if (profile == null || profile.attacks.Length == 0)
        {
            Debug.LogWarning("[IA] Sin perfil o sin ataques asignados. La IA no se activará.");
            return;
        }

        aiCoroutine = StartCoroutine(AILoop());
    }

    /// <summary>Expone el CombatController para que FightManager lo conecte a PlayerInputHandler.</summary>
    public CombatController GetCombatController() => combatController;

    /// <summary>
    /// Detiene el loop de IA. Lo llama FightManager cuando alguien cae KO.
    /// </summary>
    public void Deactivate()
    {
        if (aiCoroutine != null)
        {
            StopCoroutine(aiCoroutine);
            aiCoroutine = null;
        }

        // Ocultar el indicador si la pelea termina durante un telegraph
        if (telegraphIndicator != null)
            telegraphIndicator.SetActive(false);
    }

    // 35% de posibilidades de encadenar un segundo golpe (combo)
    private const float COMBO_CHANCE = 0.35f;

    private IEnumerator AILoop()
    {
        while (true)
        {
            // Esperar entre ataques (tiempo aleatorio para que no sea predecible)
            float espera = Random.Range(profile.minCooldown, profile.maxCooldown);
            yield return new WaitForSeconds(espera);

            // No atacar si ya está ejecutando otro golpe
            if (combatController.IsAttacking) continue;

            // Determinar cuántos golpes en esta ráfaga (1 o 2)
            int golpes = Random.value < COMBO_CHANCE ? 2 : 1;

            for (int i = 0; i < golpes; i++)
            {
                // Si el anterior golpe aún se ejecuta, no encadenar
                if (combatController.IsAttacking) break;

                AttackData ataque = profile.attacks[Random.Range(0, profile.attacks.Length)];

                // El segundo golpe del combo telegrafía más rápido (sin aviso extra)
                float duracionTelegraph = i == 0
                    ? profile.telegraphDuration
                    : profile.telegraphDuration * 0.4f;

                // Mostrar pose de telegraph (sprite dedicado + indicador "!!")
                kirikuAnimator?.Play("Telegraph");
                if (telegraphIndicator != null)
                    telegraphIndicator.SetActive(true);

                string tipo = i == 0 ? "" : " [COMBO]";
                Debug.Log($"[IA] {profile.rivalName} TELEGRAFÍA{tipo}: {ataque.attackName} ({duracionTelegraph:F2}s)");
                yield return new WaitForSeconds(duracionTelegraph);

                if (telegraphIndicator != null)
                    telegraphIndicator.SetActive(false);

                combatController.TryAttack(ataque);

                // Pequeña pausa entre los dos golpes del combo
                if (i < golpes - 1)
                    yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
