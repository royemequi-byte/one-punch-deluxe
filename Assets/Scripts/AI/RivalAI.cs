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

    private IEnumerator AILoop()
    {
        while (true)
        {
            // Esperar entre ataques (tiempo aleatorio para que no sea predecible)
            float espera = Random.Range(profile.minCooldown, profile.maxCooldown);
            yield return new WaitForSeconds(espera);

            // No atacar si ya está ejecutando otro golpe
            if (combatController.IsAttacking) continue;

            // Elegir un ataque al azar
            AttackData ataque = profile.attacks[Random.Range(0, profile.attacks.Length)];

            // Fase telegraph: mostrar el "!" para avisar al jugador
            if (telegraphIndicator != null)
                telegraphIndicator.SetActive(true);

            Debug.Log($"[IA] {profile.rivalName} TELEGRAFÍA: {ataque.attackName} ({profile.telegraphDuration}s)");
            yield return new WaitForSeconds(profile.telegraphDuration);

            // Ocultar el aviso y ejecutar el golpe
            if (telegraphIndicator != null)
                telegraphIndicator.SetActive(false);

            combatController.TryAttack(ataque);
        }
    }
}
