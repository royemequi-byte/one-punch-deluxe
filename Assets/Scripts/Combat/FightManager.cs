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

    [Header("Configuración de rounds")]
    [SerializeField] private int   roundsParaGanar   = 2;
    [SerializeField] private float pausaEntreRounds  = 3.5f;

    private int  wallyRoundsWon = 0;
    private int  rivalRoundsWon = 0;
    private int  roundActual    = 0;
    private bool roundActivo    = false;

    private FighterBody wallyBody;
    private FighterBody rivalBody;
    private CombatController wallyCombat;
    private CombatController rivalCombat;

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

        // Balance: la pelea dura lo suficiente para aprender y conectar combos
        if (wallyHealth != null) wallyHealth.SetMaxHealth(300);
        if (rivalHealth != null) rivalHealth.SetMaxHealth(300);
    }

    private IEnumerator Start()
    {
        yield return null; // esperar un frame a que todos los Start() terminen

        if (playerInput != null && rivalAI != null)
        {
            CombatController rivalCC = rivalAI.GetCombatController();
            CombatController wallyCC = playerInput.GetComponent<CombatController>();
            wallyCombat = wallyCC;
            rivalCombat = rivalCC;
            if (rivalCC != null) playerInput.SetRivalCombat(rivalCC);

            // Cada peleador conoce a su objetivo: así el combate sabe a quién golpear
            if (rivalCC != null && wallyCC != null)
            {
                wallyCC.SetTarget(rivalHealth, rivalCC);
                rivalCC.SetTarget(wallyHealth, wallyCC);
            }

            wallyBody = playerInput.GetComponent<FighterBody>();
            rivalBody = rivalAI.GetComponent<FighterBody>();
        }

        // Modo perfil: sprites laterales nuevos + ring lateral del mismo lugar
        if (wallyBody != null && wallyBody.GetComponent<ProfileFighterAnimator>() == null)
            wallyBody.gameObject.AddComponent<ProfileFighterAnimator>();
        if (rivalBody != null && rivalBody.GetComponent<ProfileFighterAnimator>() == null)
            rivalBody.gameObject.AddComponent<ProfileFighterAnimator>();
        if (GetComponent<StageSwitcher>() == null)
            gameObject.AddComponent<StageSwitcher>();

        PrepararCombatientes();
        if (playerInput != null) playerInput.enabled = false;

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(IniciarRound(1));
    }

    // Evita que los cuerpos se atraviesen: separación mínima entre peleadores
    private void LateUpdate()
    {
        if (wallyBody == null || rivalBody == null) return;
        if (!roundActivo) return;
        if (wallyHealth != null && wallyHealth.IsDead) return;
        if (rivalHealth != null && rivalHealth.IsDead) return;

        float dx = rivalBody.X - wallyBody.X;
        // 1.35 deja aire entre los dos cuerpos: antes se solapaban y se veía sucio.
        const float minDistance = 1.35f;

        if (Mathf.Abs(dx) < minDistance)
        {
            float dir = dx >= 0f ? 1f : -1f;
            float overlap = minDistance - Mathf.Abs(dx);
            wallyBody.Push(-dir * overlap * 0.5f);
            rivalBody.Push(dir * overlap * 0.5f);
        }
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

        // El perdedor se queda quieto en el suelo
        wallyBody?.Move(0f);
        rivalBody?.Move(0f);

        if (wallyGana) wallyRoundsWon++;
        else rivalRoundsWon++;

        // El perdedor cae con una animación procedural (FighterVisual).
        // No se usan Victory/Knockdown del Animator: sus hojas son traslúcidas.

        // El K.O. visual ya lo dispara Health al llegar a cero
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

        // Se corta la música en ambos casos: la pantalla final y la voz del
        // locutor necesitan silencio para pegar fuerte.
        AudioManager.Instance?.StopMusic();

        if (wallyGanaFinal)
        {
            AudioManager.Instance?.PlaySFX(lib?.sfxCampana, 1f);
            AudioManager.Instance?.PlaySFX(lib?.sfxImpacto, 0.6f, 0.9f);
        }
        else
        {
            AudioManager.Instance?.PlaySFX(lib?.sfxCaida, 0.8f, 0.85f);
        }
    }

    // ── HELPERS ────────────────────────────────────────────────────

    private void PrepararCombatientes()
    {
        wallyHealth?.ResetHealth();
        rivalHealth?.ResetHealth();
        wallyStamina?.ResetStamina();
        rivalStamina?.ResetStamina();

        // La posición y la escala viven en la escena, no aquí.
        // Si el código las reescribe, cualquier ajuste del Inspector se pierde al dar Play.

        if (wallyAnimator != null && wallyAnimator.enabled) wallyAnimator.Play("WallyIdle", 0, 0f);
        if (rivalAnimator != null && rivalAnimator.enabled) rivalAnimator.Play("Idle", 0, 0f);

        if (playerInput != null) playerInput.enabled = true;
    }

    private void SuscribirEventos()
    {
        wallyDeathHandler  = () => { hud?.MostrarKO(); TerminarRound(wallyGana: false); };
        rivalDeathHandler  = () => { hud?.MostrarKO(); TerminarRound(wallyGana: true); };
        // El daño se reacciona con flash y retroceso (FighterVisual), sin usar
        // las hojas "hit" que son traslúcidas y se veían azules sobre el ring.
        wallyDamageHandler = () => { };
        rivalDamageHandler = () => { };

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
