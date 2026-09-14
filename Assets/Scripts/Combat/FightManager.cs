using UnityEngine;
using System.Collections;

/// <summary>
/// Árbitro de la pelea: maneja los 3 rounds en formato mejor-de-tres (ganar 2 rounds).
/// Orquesta vida, stamina, IA, input y audio en cada round.
/// </summary>
public class FightManager : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private Health wallyHealth;
    [SerializeField] private Health rivalHealth;

    [Header("Stamina")]
    [SerializeField] private Stamina wallyStamina;
    [SerializeField] private Stamina rivalStamina;

    [Header("IA del rival")]
    [SerializeField] private RivalAI rivalAI;

    [Header("HUD")]
    [SerializeField] private HUDController hud;

    [Header("Animadores")]
    [SerializeField] private Animator wallyAnimator;
    [SerializeField] private Animator rivalAnimator;

    [Header("Input del jugador")]
    [SerializeField] private PlayerInputHandler playerInput;

    [Header("Configuración de rounds")]
    [SerializeField] private int roundsParaGanar = 2;      // ganar 2 de 3 rounds
    [SerializeField] private float pausaEntreRounds = 3.5f; // segundos de pausa antes del siguiente round

    // Marcador de la pelea
    private int wallyRoundsWon = 0;
    private int rivalRoundsWon = 0;
    private int roundActual   = 0;
    private bool roundActivo  = false;

    // Handlers almacenados para poder desuscribirlos correctamente
    private System.Action wallyDeathHandler;
    private System.Action rivalDeathHandler;
    private System.Action wallyDamageHandler;
    private System.Action rivalDamageHandler;

    private IEnumerator Start()
    {
        // Esperar un frame para que todos los Start() del resto de scripts terminen
        yield return null;
        IniciarRound(1);
    }

    // ── FLUJO PRINCIPAL ───────────────────────────────────────

    private void IniciarRound(int numeroRound)
    {
        roundActual  = numeroRound;
        roundActivo  = true;

        PrepararCombatientes();
        SuscribirEventos();

        rivalAI?.Activate();
        hud?.IniciarRound(roundActual, wallyRoundsWon, rivalRoundsWon);

        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlaySFX(lib?.sfxCampana);
        AudioManager.Instance?.PlayMusic(lib?.musicaCombate);

        Debug.Log($"[FightManager] ROUND {roundActual} — Marcador Wally:{wallyRoundsWon} Rival:{rivalRoundsWon}");
    }

    /// <summary>Llamado por el HUD cuando el tiempo del round se agota.</summary>
    public void FueraDeTiempo()
    {
        if (!roundActivo) return;
        bool wallyGana = wallyHealth.CurrentHealth >= rivalHealth.CurrentHealth;
        Debug.Log($"[FightManager] Tiempo agotado — Wally:{wallyHealth.CurrentHealth} Rival:{rivalHealth.CurrentHealth}");
        TerminarRound(wallyGana);
    }

    private void TerminarRound(bool wallyGana)
    {
        if (!roundActivo) return;
        roundActivo = false;

        DesuscribirEventos();
        rivalAI?.Deactivate();
        if (playerInput != null) playerInput.enabled = false;

        if (wallyGana)
        {
            wallyRoundsWon++;
            wallyAnimator?.SetTrigger("Victory");
            rivalAnimator?.SetTrigger("Defeat");
        }
        else
        {
            rivalRoundsWon++;
            wallyAnimator?.SetTrigger("Knockdown");
            rivalAnimator?.SetTrigger("Victory");
        }

        Debug.Log($"[FightManager] Round {roundActual} terminado — Wally:{wallyRoundsWon} Rival:{rivalRoundsWon}");

        bool peleatTerminada = wallyRoundsWon >= roundsParaGanar || rivalRoundsWon >= roundsParaGanar;

        if (peleatTerminada)
            StartCoroutine(FinalizarPelea(wallyRoundsWon >= roundsParaGanar));
        else
            StartCoroutine(SiguienteRound());
    }

    // ── COROUTINES ─────────────────────────────────────────────

    private IEnumerator SiguienteRound()
    {
        hud?.MostrarEntreRounds(roundActual, wallyRoundsWon, rivalRoundsWon);
        yield return new WaitForSeconds(pausaEntreRounds);
        hud?.OcultarEntreRounds();
        IniciarRound(roundActual + 1);
    }

    private IEnumerator FinalizarPelea(bool wallyGanaFinal)
    {
        yield return new WaitForSeconds(2f);

        SaveManager.Instance?.RegistrarResultado(wallyGanaFinal);
        hud?.MostrarResultado(wallyGanaFinal, wallyRoundsWon, rivalRoundsWon);

        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlayMusic(wallyGanaFinal ? lib?.musicaVictoria : lib?.musicaCombate);
    }

    // ── HELPERS ────────────────────────────────────────────────

    private void PrepararCombatientes()
    {
        // Restaurar vida y stamina al máximo al inicio de cada round
        wallyHealth.ResetHealth();
        rivalHealth.ResetHealth();
        wallyStamina?.ResetStamina();
        rivalStamina?.ResetStamina();

        // Volver al estado Idle en el Animator
        wallyAnimator?.Play("Idle", 0, 0f);
        rivalAnimator?.Play("Idle", 0, 0f);

        if (playerInput != null) playerInput.enabled = true;
    }

    private void SuscribirEventos()
    {
        wallyDeathHandler  = () => TerminarRound(wallyGana: false);
        rivalDeathHandler  = () => TerminarRound(wallyGana: true);
        wallyDamageHandler = () => wallyAnimator?.SetTrigger("Hit");
        rivalDamageHandler = () => rivalAnimator?.SetTrigger("Hit");

        wallyHealth.OnDeath  += wallyDeathHandler;
        rivalHealth.OnDeath  += rivalDeathHandler;
        wallyHealth.OnDamage += wallyDamageHandler;
        rivalHealth.OnDamage += rivalDamageHandler;

        if (hud != null) hud.OnTiempoAgotado += FueraDeTiempo;
    }

    private void DesuscribirEventos()
    {
        if (wallyHealth != null)  wallyHealth.OnDeath  -= wallyDeathHandler;
        if (rivalHealth != null)  rivalHealth.OnDeath  -= rivalDeathHandler;
        if (wallyHealth != null)  wallyHealth.OnDamage -= wallyDamageHandler;
        if (rivalHealth != null)  rivalHealth.OnDamage -= rivalDamageHandler;
        if (hud != null)          hud.OnTiempoAgotado  -= FueraDeTiempo;
    }

    private void OnDestroy() => DesuscribirEventos();
}
