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

    [Header("Animadores")]
    [SerializeField] private Animator wallyAnimator;
    [SerializeField] private Animator rivalAnimator;

    [Header("Input del jugador")]
    [SerializeField] private PlayerInputHandler playerInput;

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

        // Habilitar input del jugador
        if (playerInput != null) playerInput.enabled = true;

        // Preparar handlers antes de suscribir (evita lambdas que no se pueden desuscribir)
        wallyDeathHandler = () => TerminarPelea(ganadorEsWally: false);
        rivalDeathHandler = () => TerminarPelea(ganadorEsWally: true);

        wallyHealth.OnDeath += wallyDeathHandler;
        rivalHealth.OnDeath += rivalDeathHandler;

        // Animación de golpe recibido para cada peleador
        wallyHealth.OnDamage += () => wallyAnimator?.SetTrigger("Hit");
        rivalHealth.OnDamage += () => rivalAnimator?.SetTrigger("Hit");

        // Suscribir evento de tiempo agotado del HUD
        if (hud != null) hud.OnTiempoAgotado += FueraDeTiempo;

        // Activar la IA del rival
        rivalAI?.Activate();

        // Iniciar música de combate y campana de round
        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlaySFX(lib?.sfxCampana);
        AudioManager.Instance?.PlayMusic(lib?.musicaCombate);

        Debug.Log("¡Pelea iniciada! WASD=esquive, Space=bloqueo, J/K/L=golpes");
    }

    /// <summary>
    /// Llamado por el HUD cuando el temporizador llega a cero.
    /// El ganador se decide por quién tiene más vida.
    /// </summary>
    public void FueraDeTiempo()
    {
        if (fightOver) return;
        bool wallyGana = wallyHealth.CurrentHealth >= rivalHealth.CurrentHealth;
        Debug.Log($"[Tiempo] ¡Tiempo agotado! Wally:{wallyHealth.CurrentHealth} vs Rival:{rivalHealth.CurrentHealth}");
        TerminarPelea(ganadorEsWally: wallyGana);
    }

    private void TerminarPelea(bool ganadorEsWally)
    {
        if (fightOver) return;
        fightOver = true;

        // Detener la IA para que el rival no siga atacando después del KO
        rivalAI?.Deactivate();

        // Deshabilitar input del jugador (ya no puede golpear ni esquivar)
        if (playerInput != null) playerInput.enabled = false;

        // Animaciones de fin de pelea
        if (ganadorEsWally)
        {
            wallyAnimator?.SetTrigger("Victory");
            rivalAnimator?.SetTrigger("Defeat");
        }
        else
        {
            wallyAnimator?.SetTrigger("Knockdown");
            rivalAnimator?.SetTrigger("Victory");
        }

        // Guardar resultado antes de mostrar la pantalla
        SaveManager.Instance?.RegistrarResultado(ganadorEsWally);

        // Mostrar pantalla de resultado
        hud?.MostrarResultado(ganadorEsWally);

        // Música de resultado
        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlayMusic(ganadorEsWally ? lib?.musicaVictoria : lib?.musicaCombate);
    }

    private void OnDestroy()
    {
        // Desuscribir limpiamente para evitar errores si la escena se reinicia
        if (wallyHealth != null) wallyHealth.OnDeath -= wallyDeathHandler;
        if (rivalHealth != null) rivalHealth.OnDeath -= rivalDeathHandler;
        if (hud != null) hud.OnTiempoAgotado -= FueraDeTiempo;
    }
}
