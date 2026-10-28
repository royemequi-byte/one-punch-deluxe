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

    // Layout KOF XV: igual que la referencia
    // Wally nuevo (1.565x1.95u/frame): escala 3 → 4.7x5.85u, cabeza en y=2.85
    // Kiriku (1.27x1.765u/frame): escala 3 → 3.81x5.3u, cabeza en y=2.3
    // Ambos con pies en y=-3 (suelo del ring), x=-2 / +2 enfrentados
    private readonly Vector3 wallyPosicion = new Vector3(-2f, -3f, 0f);
    private readonly Vector3 wallyEscala   = new Vector3(3f, 3f, 1f);
    private readonly Vector3 rivalPosicion = new Vector3(2f, -3f, 0f);
    private readonly Vector3 rivalEscala   = new Vector3(-3f, 3f, 1f);

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

    private void Awake()
    {
        // Auto-conectar referencias si el Inspector las dejó vacías
        if (rivalAI      == null) rivalAI      = Object.FindFirstObjectByType<RivalAI>();
        if (playerInput  == null) playerInput  = Object.FindFirstObjectByType<PlayerInputHandler>();
        if (hud          == null) hud          = Object.FindFirstObjectByType<HUDController>();

        if (rivalAI != null)
        {
            if (rivalHealth   == null) rivalHealth   = rivalAI.GetComponent<Health>();
            if (rivalStamina  == null) rivalStamina  = rivalAI.GetComponent<Stamina>();
            if (rivalAnimator == null) rivalAnimator = rivalAI.GetComponent<Animator>();
        }
        if (playerInput != null)
        {
            if (wallyHealth   == null) wallyHealth   = playerInput.GetComponent<Health>();
            if (wallyStamina  == null) wallyStamina  = playerInput.GetComponent<Stamina>();
            if (wallyAnimator == null) wallyAnimator = playerInput.GetComponent<Animator>();
        }
    }

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

        if (rivalHealth != null)
        {
            rivalHealth.transform.position   = rivalPosicion;
            rivalHealth.transform.localScale = rivalEscala;
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
