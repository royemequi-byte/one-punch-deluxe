using UnityEngine;

/// <summary>
/// Ejecuta los golpes de un peleador y decide si impactan.
/// Un golpe pasa por Windup → Active → Recovery. Si impacta, el Recovery se puede
/// cancelar con el siguiente golpe: así salen los combos (jab → recto → gancho).
/// No usa colliders: el impacto se mide por distancia y por estado del rival.
/// </summary>
public class CombatController : MonoBehaviour
{
    private enum Phase { None, Windup, Active, Recovery }

    [Header("Referencias")]
    [SerializeField] private Stamina stamina;
    [SerializeField] private Animator animator;

    [Header("Objetivo")]
    [SerializeField] private Health targetHealth;
    [SerializeField] private CombatController targetCombat;

    [Header("Bloqueo")]
    [SerializeField] private float blockDamageMultiplier = 0.2f;

    private Phase phase = Phase.None;
    private AttackData current;
    private AttackData buffered;
    private float bufferedAt;
    private float phaseTimer;
    private float hitstunTimer;
    private float blockstunTimer;
    private bool blocking;
    private bool hitConfirmed;
    private bool hasBlockParameter;
    private FighterBody body;

    private int comboHits;
    private float lastHitAt;

    /// <summary>Se dispara cuando un golpe conecta. true = lo bloquearon.</summary>
    public event System.Action<bool> OnHitLanded;

    public bool IsAttacking => phase != Phase.None;
    public bool CanAct => phase == Phase.None && hitstunTimer <= 0f && blockstunTimer <= 0f;
    public bool IsBlocking => blocking && hitstunTimer <= 0f && blockstunTimer <= 0f;

    private bool invulnerable;
    private float hitInvulnTimer;

    /// <summary>Invulnerable por esquive o por el instante de recuperación tras un stun.</summary>
    public bool IsInvulnerable
    {
        get => invulnerable || hitInvulnTimer > 0f;
        set => invulnerable = value;
    }
    public AttackData CurrentAttack => current;

    /// <summary>Progreso del golpe actual de 0 a 1. Lo usa el animador de perfil.</summary>
    public float AttackProgress
    {
        get
        {
            if (phase == Phase.None || current == null) return 0f;
            float total = current.windupTime + current.activeTime + current.recoveryTime;
            if (total <= 0f) return 0f;

            float elapsed = phaseTimer;
            if (phase == Phase.Active) elapsed += current.windupTime;
            else if (phase == Phase.Recovery) elapsed += current.windupTime + current.activeTime;

            return Mathf.Clamp01(elapsed / total);
        }
    }

    /// <summary>Golpes encadenados en los últimos 0.8 s. Lo muestra el contador de combos.</summary>
    public int ComboHits => comboHits;

    private void Awake()
    {
        // Si el Inspector no las conectó, se buscan en el mismo objeto
        if (animator == null) animator = GetComponent<Animator>();
        if (stamina == null) stamina = GetComponent<Stamina>();

        body = GetComponent<FighterBody>();
        hasBlockParameter = animator != null &&
            System.Array.Exists(animator.parameters, p => p.name == "IsBlocking");
    }

    private void Start()
    {
        // FighterBody puede añadirse en el Awake de otro script
        if (body == null) body = GetComponent<FighterBody>();
    }

    /// <summary>Asigna a quién golpea este peleador. Lo llama FightManager.</summary>
    public void SetTarget(Health health) { targetHealth = health; }

    /// <summary>Asigna objetivo y su controlador de combate (para bloqueo y stun).</summary>
    public void SetTarget(Health health, CombatController combat)
    {
        targetHealth = health;
        targetCombat = combat;
    }

    /// <summary>El peleador mantiene la guardia (dirección contraria al rival).</summary>
    public void SetBlocking(bool value)
    {
        blocking = value && CanAct;
        if (hasBlockParameter && animator.enabled) animator.SetBool("IsBlocking", blocking);
    }

    /// <summary>
    /// Intenta ejecutar un ataque. Si ya está atacando, lo guarda como buffer
    /// para encadenarlo en cuanto el golpe actual confirme impacto.
    /// </summary>
    public bool TryAttack(AttackData attack)
    {
        if (attack == null) return false;

        // Cancelación de combo: si el golpe anterior impactó, el recovery se corta.
        if (phase == Phase.Recovery && hitConfirmed)
        {
            if (stamina != null && !stamina.UseStamina(attack.staminaCost)) return false;
            StartAttack(attack);
            return true;
        }

        if (!CanAct)
        {
            if (IsAttacking) { buffered = attack; bufferedAt = Time.time; }
            return false;
        }

        if (stamina != null && !stamina.UseStamina(attack.staminaCost)) return false;
        StartAttack(attack);
        return true;
    }

    /// <summary>Recibe un golpe: queda aturdido y pierde la acción actual.</summary>
    public void ApplyHitstun(float seconds)
    {
        hitstunTimer = Mathf.Max(hitstunTimer, seconds);
        phase = Phase.None;
        current = null;
        buffered = null;
        blocking = false;
        if (hasBlockParameter && animator.enabled) animator.SetBool("IsBlocking", false);
    }

    /// <summary>Bloqueó un golpe: no puede actuar un instante, pero no recibe stun completo.</summary>
    public void ApplyBlockstun(float seconds)
    {
        blockstunTimer = Mathf.Max(blockstunTimer, seconds);
        phase = Phase.None;
        current = null;
    }

    private void StartAttack(AttackData attack)
    {
        current = attack;
        phase = Phase.Windup;
        phaseTimer = 0f;
        hitConfirmed = false;
        buffered = null;
        blocking = false;
        IsInvulnerable = false;
        if (hasBlockParameter && animator.enabled) animator.SetBool("IsBlocking", false);

        if (animator != null && animator.enabled && !string.IsNullOrEmpty(attack.animationTrigger))
            animator.SetTrigger(attack.animationTrigger);

        // Whoosh de aire al lanzar; el impacto suena recién cuando conecta
        AudioClip swing = AudioManager.Instance?.Library?.sfxSwing;
        AudioManager.Instance?.PlaySFX(swing, 0.55f, Random.Range(1f, 1.12f));
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // El combo se reinicia si pasa casi un segundo sin conectar
        if (comboHits > 0 && Time.time - lastHitAt > 0.9f) comboHits = 0;
        bool estabaAturdido = hitstunTimer > 0f;
        if (hitstunTimer > 0f) hitstunTimer -= dt;
        if (estabaAturdido && hitstunTimer <= 0f) hitInvulnTimer = 0.22f;
        if (hitInvulnTimer > 0f) hitInvulnTimer -= dt;
        if (blockstunTimer > 0f) blockstunTimer -= dt;

        if (phase == Phase.None)
        {
            if (buffered != null && Time.time - bufferedAt <= 0.18f && CanAct)
            {
                AttackData next = buffered;
                buffered = null;
                if (stamina == null || stamina.UseStamina(next.staminaCost)) StartAttack(next);
            }
            return;
        }

        phaseTimer += dt;

        if (phase == Phase.Windup && phaseTimer >= current.windupTime)
        {
            phase = Phase.Active;
            phaseTimer = 0f;
            ResolveHit();
        }
        else if (phase == Phase.Active && phaseTimer >= current.activeTime)
        {
            phase = Phase.Recovery;
            phaseTimer = 0f;

            // Si el golpe impactó y el jugador ya pidió el siguiente, se encadena.
            if (hitConfirmed && buffered != null && Time.time - bufferedAt <= 0.25f)
            {
                AttackData next = buffered;
                buffered = null;
                if (stamina == null || stamina.UseStamina(next.staminaCost)) StartAttack(next);
            }
        }
        else if (phase == Phase.Recovery && phaseTimer >= current.recoveryTime)
        {
            phase = Phase.None;
            current = null;
            hitConfirmed = false;
        }
    }

    private void ResolveHit()
    {
        // La lógica de impacto vive en HitResolver para mantener este script corto.
        FighterBody ownBody = body != null ? body : GetComponent<FighterBody>();
        bool blocked = targetCombat != null && targetCombat.IsBlocking;

        hitConfirmed = HitResolver.Resolve(
            current, transform, targetHealth, targetCombat, ownBody, blockDamageMultiplier);

        if (hitConfirmed)
        {
            RegistrarGolpeDeCombo();
            OnHitLanded?.Invoke(blocked);
        }
    }

    private void RegistrarGolpeDeCombo()
    {
        if (Time.time - lastHitAt <= 0.8f) comboHits++;
        else comboHits = 1;

        lastHitAt = Time.time;
    }
}
