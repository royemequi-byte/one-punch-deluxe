using UnityEngine;
using System.Collections;

/// <summary>
/// Árbitro de la pelea: maneja los 3 rounds en formato mejor-de-tres (ganar 2 rounds).
/// IniciarRound es ahora una coroutine: espera el cartel "ROUND X → FIGHT!" antes de
/// activar la IA y habilitar el input del jugador.
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

    [Header("Posiciones en escena")]
    [SerializeField] private Vector3 wallyPosicion = new Vector3(0.4f, -1.5f, 0.5f);
    [SerializeField] private Vector3 wallyEscala   = new Vector3(1.7f, 1.7f, 1f);

    [Header("Configuración de rounds")]
    [SerializeField] private int   roundsParaGanar   = 2;
    [SerializeField] private float pausaEntreRounds  = 3.5f;

    private int  wallyRoundsWon = 0;
    private int  rivalRoundsWon = 0;
    private int  roundActual    = 0;
    private bool roundActivo    = false;

    private System.Action wallyDeathHandler;
    private System.Action rivalDeathHandler;
    private System.Action wallyDamageHandler;
    private System.Action rivalDamageHandler;

    private IEnumerator Start()
    {
        yield return null; // esperar un frame a que todos los Start() terminen

        if (playerInput != null && rivalAI != null)
        {
            CombatController rivalCC = rivalAI.GetCombatController();
            if (rivalCC != null) playerInput.SetRivalCombat(rivalCC);
        }

        PrepararCombatientes();
        if (playerInput != null) playerInput.enabled = false;

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(IniciarRound(1));
    }

    // ── FLUJO PRINCIPAL ────────────────────────────────────────────

    /// <summary>
    /// Muestra el cartel "ROUND X → FIGHT!", espera que termine y ENTONCES activa la IA.
    /// </summary>
    private IEnumerator IniciarRound(int numeroRound)
    {
        roundActual  = numeroRound;
        roundActivo  = false;

        PrepararCombatientes();
        if (playerInput != null) playerInput.enabled = false;
        rivalAI?.Deactivate();

        // Disparar cartel; el timer empieza cuando termina el cartel (IniciarTimer más abajo)
        hud?.IniciarRound(roundActual, wallyRoundsWon, rivalRoundsWon);
        float espera = hud != null ? hud.DuracionAnuncio : 2.5f;
        yield return new WaitForSeconds(espera);

        // ¡Ahora comienza la pelea!
        roundActivo = true;
        SuscribirEventos();
        rivalAI?.Activate();
        hud?.IniciarTimer();
        if (playerInput != null) playerInput.enabled = true;

        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlaySFX(lib?.sfxCampana);
        AudioManager.Instance?.PlayMusic(lib?.musicaCombate);

        Debug.Log($"[FightManager] ROUND {roundActual} iniciado — Wally:{wallyRoundsWon} Rival:{rivalRoundsWon}");
    }

    /// <summary>Llamado por el HUD cuando el tiempo del round se agota.</summary>
    public void FueraDeTiempo()
    {
        if (!roundActivo) return;
        bool wallyGana = wallyHealth.CurrentHealth >= rivalHealth.CurrentHealth;
        Debug.Log($"[FightManager] Tiempo — Wally:{wallyHealth.CurrentHealth} Rival:{rivalHealth.CurrentHealth}");
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

        VisualJuice.Instance?.KnockOut();
        Debug.Log($"[FightManager] Round {roundActual} — Wally:{wallyRoundsWon} Rival:{rivalRoundsWon}");

        bool peleatTerminada = wallyRoundsWon >= roundsParaGanar || rivalRoundsWon >= roundsParaGanar;

        if (peleatTerminada) StartCoroutine(FinalizarPelea(wallyRoundsWon >= roundsParaGanar));
        else                 StartCoroutine(SiguienteRound());
    }

    // ── COROUTINES ──────────────────────────────────────────────────

    private IEnumerator SiguienteRound()
    {
        hud?.MostrarEntreRounds(roundActual, wallyRoundsWon, rivalRoundsWon);
        yield return new WaitForSeconds(pausaEntreRounds);
        hud?.OcultarEntreRounds();
        yield return StartCoroutine(IniciarRound(roundActual + 1));
    }

    private IEnumerator FinalizarPelea(bool wallyGanaFinal)
    {
        yield return new WaitForSeconds(2f);
        SaveManager.Instance?.RegistrarResultado(wallyGanaFinal);
        hud?.MostrarResultado(wallyGanaFinal, wallyRoundsWon, rivalRoundsWon);

        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlayMusic(wallyGanaFinal ? lib?.musicaVictoria : lib?.musicaCombate);
    }

    // ── HELPERS ────────────────────────────────────────────────────

    private void PrepararCombatientes()
    {
        wallyHealth?.ResetHealth();
        rivalHealth?.ResetHealth();
        wallyStamina?.ResetStamina();
        rivalStamina?.ResetStamina();

        if (wallyHealth != null)
        {
            wallyHealth.transform.position   = wallyPosicion;
            wallyHealth.transform.localScale = wallyEscala;
        }

        wallyAnimator?.Play("WallyIdle", 0, 0f);
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
        if (wallyHealth != null) { wallyHealth.OnDeath -= wallyDeathHandler; wallyHealth.OnDamage -= wallyDamageHandler; }
        if (rivalHealth != null) { rivalHealth.OnDeath -= rivalDeathHandler; rivalHealth.OnDamage -= rivalDamageHandler; }
        if (hud != null) hud.OnTiempoAgotado -= FueraDeTiempo;
    }

    private void OnDestroy() => DesuscribirEventos();
}
