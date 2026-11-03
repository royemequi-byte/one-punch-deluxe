using UnityEngine;

/// <summary>
/// IA del rival con comportamiento de juego de pelea: se acerca, se aleja,
/// bloquea cuando Wally ataca y encadena combos. Ya no espera quieta 2 segundos.
/// La personalidad sale de un AIProfile (agresividad, bloqueo, reacción, combo).
/// </summary>
public class RivalAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CombatController combatController;
    [SerializeField] private GameObject telegraphIndicator;

    [Header("Perfil de comportamiento")]
    [SerializeField] private AIProfile profile;

    [Header("Objetivo (Wally)")]
    [SerializeField] private Transform rival;

    private FighterBody body;
    private float dodgeTimer;
    private float pauseTimer;
    private float thinkTimer;
    private float moveDir;
    private float blockTimer;
    private float reactionTimer;
    private float comboGap;
    private int comboLeft;
    private bool playerWasAttacking;

    /// <summary>FightManager lo usa para conectar el combate del rival con el jugador.</summary>
    public CombatController GetCombatController() => combatController;

    private void Awake()
    {
        body = GetComponent<FighterBody>();
        if (body == null) body = gameObject.AddComponent<FighterBody>();

        if (GetComponent<PowerMeter>() == null) gameObject.AddComponent<PowerMeter>();
        if (GetComponent<FighterAura>() == null) gameObject.AddComponent<FighterAura>();

        // Si el Inspector no asignó a Wally, se busca en la escena
        if (rival == null)
        {
            PlayerInputHandler player = Object.FindFirstObjectByType<PlayerInputHandler>();
            if (player != null) rival = player.transform;
        }
    }

    /// <summary>Arranca la IA al empezar el round.</summary>
    public void Activate()
    {
        enabled = true;
        thinkTimer = 0f;
        comboLeft = 0;
        blockTimer = 0f;
        pauseTimer = 1.5f; // deja respirar al empezar el round
    }

    /// <summary>Detiene la IA al terminar el round.</summary>
    public void Deactivate()
    {
        enabled = false;
        if (body != null) body.Move(0f);
        if (combatController != null) { combatController.SetBlocking(false); combatController.IsInvulnerable = false; }
    }

    private void Update()
    {
        if (combatController == null || profile == null || body == null) return;

        Transform enemy = rival != null ? rival : null;
        if (enemy == null) return;

        float dx = enemy.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);
        body.SetFacing(dx >= 0f ? 1 : -1);

        // Esquive: paso atrás rápido con invulnerabilidad, sin poses traslúcidas
        if (dodgeTimer > 0f)
        {
            dodgeTimer -= Time.deltaTime;
            body.Move(-Mathf.Sign(dx));
            if (dodgeTimer <= 0f) combatController.IsInvulnerable = false;
            return;
        }

        if (!combatController.CanAct)
        {
            body.Move(0f);
            return;
        }

        // Reacción al ataque del jugador: tras un tiempo de reacción puede bloquear.
        CombatController playerCombat = enemy.GetComponent<CombatController>();
        bool playerAttacking = playerCombat != null && playerCombat.IsAttacking && dist < 2.2f;

        if (playerAttacking)
        {
            if (!playerWasAttacking) reactionTimer = profile.reactionTime;
            playerWasAttacking = true;

            if (reactionTimer > 0f) reactionTimer -= Time.deltaTime;
            else if (blockTimer <= 0f && dodgeTimer <= 0f)
            {
                float r = Random.value;
                if (r < profile.blockChance)
                {
                    blockTimer = 0.35f;
                }
                else if (r < profile.blockChance + profile.dodgeChance)
                {
                    combatController.IsInvulnerable = true;
                    dodgeTimer = 0.28f;
                }
            }
        }
        else playerWasAttacking = false;

        if (blockTimer > 0f)
        {
            blockTimer -= Time.deltaTime;
            combatController.SetBlocking(true);
            body.Move(0f);
            return;
        }
        combatController.SetBlocking(false);

        // Pausa después de cada combo: sin esto el jugador queda stuneado para siempre
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            if (dist > 2.0f) body.Move(Mathf.Sign(dx));
            else body.Move(0f);
            return;
        }

        // Combo en curso: sigue golpeando con un pequeño intervalo.
        if (comboLeft > 0)
        {
            comboGap -= Time.deltaTime;
            body.Move(0f);

            if (comboGap <= 0f)
            {
                AttackData attack = ChooseAttack(dist);
                if (attack != null && combatController.TryAttack(attack))
                {
                    comboLeft--;
                    comboGap = 0.1f;
                    if (comboLeft == 0) pauseTimer = Random.Range(0.8f, 1.3f);
                }
                else comboGap = 0.05f;
            }
            return;
        }

        // Toma de decisiones rápida: cada 0.08–0.18 s, como una IA de pelea.
        thinkTimer -= Time.deltaTime;
        if (thinkTimer > 0f)
        {
            body.Move(moveDir);
            return;
        }

        thinkTimer = Random.Range(0.12f, 0.22f);

        if (dist > 1.55f)
        {
            moveDir = Mathf.Sign(dx);
        }
        else if (dist < 0.95f)
        {
            moveDir = -Mathf.Sign(dx);
        }
        else
        {
            moveDir = 0f;

            if (Random.value < profile.aggressiveness)
            {
                comboLeft = Random.Range(1, profile.comboLength + 1);
                comboGap = 0f;
            }
            else if (Random.value < 0.4f)
            {
                moveDir = Random.value < 0.5f ? -1f : 1f;
            }
        }

        body.Move(moveDir);
    }

    // Elige un golpe que alcance la distancia actual
    private AttackData ChooseAttack(float dist)
    {
        if (profile.attacks == null || profile.attacks.Length == 0) return null;

        for (int i = 0; i < 5; i++)
        {
            AttackData candidate = profile.attacks[Random.Range(0, profile.attacks.Length)];
            if (candidate != null && candidate.reach >= dist - 0.05f) return candidate;
        }

        return profile.attacks[Random.Range(0, profile.attacks.Length)];
    }
}
