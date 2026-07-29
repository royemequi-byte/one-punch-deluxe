using UnityEngine;

/// <summary>
/// Árbitro de la pelea: inicializa, escucha KOs y activa/desactiva la IA.
/// Es el único script que conoce a todos los participantes y los conecta.
/// </summary>
public class FightManager : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private Health wallyHealth;
    [SerializeField] private Health rivalHealth;

    [Header("Combate")]
    [SerializeField] private CombatController wallyCombat;
    [SerializeField] private CombatController rivalCombat;

    [Header("IA del rival")]
    [SerializeField] private RivalAI rivalAI;

    [Header("HUD")]
    [SerializeField] private HUDController hud;

    private bool fightOver = false;

    // Guardamos los handlers para poder desuscribirlos limpiamente
    private System.Action wallyDeathHandler;
    private System.Action rivalDeathHandler;

    private void Start()
    {
        IniciarPelea();
    }

    private void IniciarPelea()
    {
        fightOver = false;

        // Restaurar vida
        wallyHealth.ResetHealth();
        rivalHealth.ResetHealth();

        // Preparar handlers antes de suscribir (evita lambdas que no se pueden desuscribir)
        wallyDeathHandler = () => TerminarPelea(ganadorEsWally: false);
        rivalDeathHandler = () => TerminarPelea(ganadorEsWally: true);

        wallyHealth.OnDeath += wallyDeathHandler;
        rivalHealth.OnDeath += rivalDeathHandler;

        // Activar la IA del rival
        rivalAI?.Activate();

        Debug.Log("¡Pelea iniciada! WASD=esquive, Space=bloqueo, J/K/L=golpes");
    }

    private void TerminarPelea(bool ganadorEsWally)
    {
        if (fightOver) return;
        fightOver = true;

        // Detener la IA para que el rival no siga atacando después del KO
        rivalAI?.Deactivate();

        // Mostrar pantalla de resultado
        hud?.MostrarResultado(ganadorEsWally);
    }

    private void OnDestroy()
    {
        // Desuscribir limpiamente para evitar errores si la escena se reinicia
        if (wallyHealth != null) wallyHealth.OnDeath -= wallyDeathHandler;
        if (rivalHealth != null) rivalHealth.OnDeath -= rivalDeathHandler;
    }
}
