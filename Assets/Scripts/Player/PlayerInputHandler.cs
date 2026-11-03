using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lee teclado y mando de PlayStation (se detecta solo).
/// Teclado: flechas o A/D para mover, W saltar, J/K/L golpes, Espacio bloquear.
/// Mando: stick/D-pad mover, Cuadrado/Triángulo/Círculo golpear, L1 bloquear, Cruz saltar.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Combate de Wally")]
    [SerializeField] private CombatController wallyCombat;
    [SerializeField] private Animator wallyAnimator;

    [Header("Stamina")]
    [SerializeField] private Stamina wallyStamina;

    [Header("Ataques")]
    [SerializeField] private AttackData jabData;
    [SerializeField] private AttackData straightData;
    [SerializeField] private AttackData hookData;

    [Header("Rival")]
    [SerializeField] private CombatController rivalCombat;

    [Header("Bloqueo")]
    [SerializeField] private float blockStaminaDrain = 10f;

    [Header("Entrada")]
    [SerializeField] private float doubleTapTime = 0.22f;

    [Header("Esquives")]
    [SerializeField] private float swayCooldown = 0.5f;
    [SerializeField] private float swayInvulnerability = 0.28f;

    private FighterBody body;
    private PowerMeter power;
    private AttackData superData;
    private float lastTapLeft = -1f;
    private float lastTapRight = -1f;
    private float swayReadyAt;
    private float invulnerableTimer;

    private void Awake()
    {
        body = GetComponent<FighterBody>();
        if (body == null) body = gameObject.AddComponent<FighterBody>();

        power = GetComponent<PowerMeter>();
        if (power == null) power = gameObject.AddComponent<PowerMeter>();

        if (GetComponent<FighterAura>() == null) gameObject.AddComponent<FighterAura>();

        CrearSuper();
    }

    // Ataque especial que solo sale con el medidor MAXIMUM lleno
    private void CrearSuper()
    {
        superData = ScriptableObject.CreateInstance<AttackData>();
        superData.attackName = "MAXIMUM";
        superData.damage = 55;
        superData.staminaCost = 0f;
        superData.windupTime = 0.3f;
        superData.activeTime = 0.14f;
        superData.recoveryTime = 0.35f;
        superData.reach = 1.9f;
        superData.hitstun = 0.45f;
        superData.blockstun = 0.2f;
        superData.knockback = 0.8f;
        superData.animationTrigger = "Hook";
    }

    private void OnDisable()
    {
        if (body != null) body.Move(0f);
        wallyCombat?.SetBlocking(false);
    }

    private void Update()
    {
        if (wallyCombat == null || body == null) return;

        Keyboard kb = Keyboard.current;

        // PadInput busca cualquier mando conectado (PlayStation, Xbox o
        // genérico) y también los que aparecen después de abrir el juego.
        Gamepad pad = PadInput.GamepadActivo;
        PadInput.RegistrarDispositivos();

        Transform enemy = rivalCombat != null ? rivalCombat.transform : null;
        if (enemy != null)
            body.SetFacing(enemy.position.x >= transform.position.x ? 1 : -1);

        if (invulnerableTimer > 0f)
        {
            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer <= 0f) wallyCombat.IsInvulnerable = false;
        }

        // ── Dirección: teclado (flechas o A/D) + mando ─────────────
        bool left = kb != null && (kb.aKey.isPressed || kb.leftArrowKey.isPressed);
        bool right = kb != null && (kb.dKey.isPressed || kb.rightArrowKey.isPressed);
        bool down = kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed);
        bool upPress = kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame);

        float stick = 0f;
        if (PadInput.HayMando)
        {
            stick = PadInput.EjeHorizontal;
            down |= PadInput.AbajoMantenido;
            upPress |= PadInput.ArribaPulsado;
        }

        bool leftTap = (kb != null && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame))
                    || (pad != null && (pad.dpad.left.wasPressedThisFrame || pad.leftStick.left.wasPressedThisFrame));
        bool rightTap = (kb != null && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame))
                     || (pad != null && (pad.dpad.right.wasPressedThisFrame || pad.leftStick.right.wasPressedThisFrame));

        bool canAct = wallyCombat.CanAct;

        if (canAct && power != null && power.IsFull && SuperPressed(kb, pad))
            LanzarSuper();

        float dir = 0f;

        if (canAct)
        {
            if (left) dir -= 1f;
            if (right) dir += 1f;
            if (dir == 0f && Mathf.Abs(stick) > 0.3f) dir = Mathf.Sign(stick);

            // Agachado + toque lateral = esquive
            if (down && (leftTap || rightTap) && Time.time >= swayReadyAt)
                HacerSway((right || stick > 0f) ? 1 : -1);

            if (kb != null && kb.sKey.wasPressedThisFrame &&
                wallyAnimator != null && wallyAnimator.enabled)
                wallyAnimator.SetTrigger("Duck");

            // Doble toque = dash / paso atrás
            if (leftTap)
            {
                if (Time.time - lastTapLeft <= doubleTapTime && !down)
                {
                    body.Dash(-1);
                    wallyCombat.IsInvulnerable = true;
                    invulnerableTimer = 0.16f;
                }
                lastTapLeft = Time.time;
            }
            if (rightTap)
            {
                if (Time.time - lastTapRight <= doubleTapTime && !down) body.Dash(1);
                lastTapRight = Time.time;
            }

            if (upPress || PadInput.SaltoPulsado) body.Jump();
        }

        body.Move(dir);
        LeerBloqueo(dir, canAct, enemy, pad);
        LeerGolpes(kb, pad);
    }

    private bool SuperPressed(Keyboard kb, Gamepad pad)
    {
        bool keyboard = kb != null && kb.uKey.wasPressedThisFrame;
        return keyboard || PadInput.SuperPulsado;
    }

    private void LanzarSuper()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.Library != null)
            superData.hitSound = AudioManager.Instance.Library.sfxHook;

        if (!wallyCombat.TryAttack(superData)) return;

        power.Consume();
        VisualJuice.Instance?.ZoomPunch(0.25f);
        CombatFX.Spark(transform.position + Vector3.up * 1.7f, new Color(1f, 0.85f, 0.1f), 2.4f);

        HUDController hud = Object.FindFirstObjectByType<HUDController>();
        if (hud != null) hud.MostrarMaximum();
    }

    private void HacerSway(int dir)
    {
        swayReadyAt = Time.time + swayCooldown;
        if (wallyAnimator != null && wallyAnimator.enabled)
            wallyAnimator.SetTrigger(dir > 0 ? "WeaveRight" : "WeaveLeft");

        wallyCombat.IsInvulnerable = true;
        invulnerableTimer = swayInvulnerability;
    }

    // Mantener la dirección contraria al rival activa la guardia
    private void LeerBloqueo(float dir, bool canAct, Transform enemy, Gamepad pad)
    {
        bool away = false;
        if (enemy != null && dir != 0f)
            away = Mathf.Sign(dir) != Mathf.Sign(enemy.position.x - transform.position.x);

        bool keyboardBlock = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool padBlock = PadInput.BloqueoMantenido;

        // Caminar hacia atrás NO debe verse como bloqueo: solo se bloquea
        // con Espacio/L1 o si el rival está atacando mientras retrocedes.
        bool threat = rivalCombat != null && rivalCombat.IsAttacking;
        bool blocking = canAct && body.IsGrounded && (keyboardBlock || padBlock || (away && threat));
        wallyCombat.SetBlocking(blocking);

        if (blocking && wallyStamina != null && rivalCombat != null && rivalCombat.IsAttacking)
            wallyStamina.UseStamina(blockStaminaDrain * Time.deltaTime);
    }

    // Los golpes se pueden pedir en cualquier momento: CombatController decide
    // si se ejecutan ahora o se guardan para encadenar un combo.
    private void LeerGolpes(Keyboard kb, Gamepad pad)
    {
        bool jab      = (kb != null && kb.jKey.wasPressedThisFrame) || PadInput.JabPulsado;
        bool straight = (kb != null && kb.kKey.wasPressedThisFrame) || PadInput.RectoPulsado;
        bool hook     = (kb != null && kb.lKey.wasPressedThisFrame) || PadInput.GanchoPulsado;

        if (jab) wallyCombat.TryAttack(jabData);
        if (straight) wallyCombat.TryAttack(straightData);
        if (hook) wallyCombat.TryAttack(hookData);
    }

    /// <summary>Compatibilidad: ya no se usan posiciones de esquive.</summary>
    public DodgePosition GetCurrentDodge() => DodgePosition.Neutral;

    /// <summary>Permite que FightManager conecte el CombatController del rival.</summary>
    public void SetRivalCombat(CombatController cc) { rivalCombat = cc; }
}
