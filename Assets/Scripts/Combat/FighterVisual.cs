using UnityEngine;

/// <summary>
/// Acabado visual del peleador sin tocar su arte: paso suave, inclinación leve,
/// polvo en los pies, destello y retroceso al recibir daño.
/// Copia el sprite que anima el Animator a un hijo visual, para no alterar
/// la física ni los límites del ring.
/// </summary>
public class FighterVisual : MonoBehaviour
{
    [Header("Paso")]
    [SerializeField] private float bobHeight = 0.008f;
    [SerializeField] private float leanAngle = 1.2f;
    [SerializeField] private float stepRate = 8f;
    [SerializeField] private float dustEvery = 0.22f;

    [Header("Daño")]
    [SerializeField] private float flashTime = 0.12f;
    [SerializeField] private float hitDuration = 0.18f;
    [SerializeField] private float hitRecoil = 0.12f;

    private FighterBody body;
    private CombatController combat;
    private Animator animator;
    private Health health;
    private PowerMeter power;
    private SpriteRenderer source;
    private SpriteRenderer visual;
    private Transform visualT;

    private float phase;
    private float dustTimer;
    private float flashTimer;
    private float hitTimer;
    private bool falling;
    private float fallProgress;

    // Si el peleador tiene hoja de caída, la animación manda y no se rota nada
    private ProfileFighterAnimator perfilAnim;
    private bool perfilBuscado;

    private void Awake()
    {
        body = GetComponent<FighterBody>();
        combat = GetComponent<CombatController>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        power = GetComponent<PowerMeter>();
        source = GetComponent<SpriteRenderer>();

        if (source == null) return;

        GameObject go = new GameObject("Visual");
        go.transform.SetParent(transform, false);

        visual = go.AddComponent<SpriteRenderer>();
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder;
        visual.sharedMaterial = source.sharedMaterial;

        visualT = go.transform;
        source.enabled = false;

        if (health != null)
        {
            health.OnDamage += OnDamage;
            health.OnDeath += OnFall;
            health.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamage -= OnDamage;
            health.OnDeath -= OnFall;
            health.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnDamage()
    {
        flashTimer = flashTime;
        hitTimer = hitDuration;
    }

    // Al caer K.O. el peleador se tumba con una rotación suave, sin hojas fantasma
    private void OnFall()
    {
        falling = true;
        fallProgress = 0f;
        hitTimer = 0f;

        AudioManager.Instance?.PlaySFX(AudioManager.Instance?.Library?.sfxCaida, 1f, 0.9f);
    }

    // Al empezar un round nuevo la vida vuelve al máximo: se levanta y se limpia
    private void OnHealthChanged(int actual, int maximo)
    {
        if (actual < maximo) return;

        falling = false;
        fallProgress = 0f;
        hitTimer = 0f;
        flashTimer = 0f;

        if (visualT != null)
        {
            visualT.localRotation = Quaternion.identity;
            visualT.localPosition = Vector3.zero;
            visualT.localScale = Vector3.one;
        }
    }

    private void LateUpdate()
    {
        if (visual == null || source == null) return;

        visual.sprite = source.sprite;
        visual.flipX = source.flipX;
        visual.sortingOrder = source.sortingOrder;
        visual.color = source.color;

        float dt = Time.deltaTime;

        // K.O.: se tumba y se queda quieto, sin idle, polvo ni rebote
        if (falling)
        {
            if (!perfilBuscado)
            {
                perfilBuscado = true;
                perfilAnim = GetComponent<ProfileFighterAnimator>();
            }

            // Con hoja de caída propia no hace falta inclinar el sprite
            if (perfilAnim != null && perfilAnim.TieneCaida)
            {
                visualT.localRotation = Quaternion.identity;
                visualT.localPosition = Vector3.zero;
                visualT.localScale    = Vector3.one;
                return;
            }

            fallProgress = Mathf.Min(1f, fallProgress + dt * 2.4f);
            float ease = 1f - (1f - fallProgress) * (1f - fallProgress);
            int facingDir = body != null ? body.Facing : 1;

            visualT.localRotation = Quaternion.Euler(0f, 0f, -facingDir * 88f * ease);
            visualT.localPosition = new Vector3(-facingDir * 0.10f * ease, -0.04f * ease, 0f);
            visualT.localScale = Vector3.one;
            return;
        }
        float move = body != null ? body.MoveAmount : 0f;
        bool grounded = body == null || body.IsGrounded;
        bool stepping = move > 0.05f && grounded && (combat == null || !combat.IsBlocking);

        float bob = 0f;
        float lean = 0f;
        float squash = 0f;
        float recoil = 0f;

        if (stepping)
        {
            phase += dt * stepRate;
            bob = Mathf.Abs(Mathf.Sin(phase)) * bobHeight;
            lean = Mathf.Sin(phase) * leanAngle;
            squash = Mathf.Sin(phase * 2f) * 0.012f;

            dustTimer -= dt;
            if (dustTimer <= 0f)
            {
                dustTimer = dustEvery;
                CombatFX.Dust(new Vector3(transform.position.x, transform.position.y + 0.05f, 0f));

                AudioClip step = AudioManager.Instance?.Library?.sfxPaso;
                AudioManager.Instance?.PlaySFX(step, 0.2f, Random.Range(0.9f, 1.1f));
            }
        }

        // Retroceso al recibir: el sprite se sacude hacia atrás sin cambiar de hoja
        if (hitTimer > 0f)
        {
            hitTimer -= dt;
            float k = Mathf.Clamp01(hitTimer / hitDuration);
            int facing = body != null ? body.Facing : 1;
            recoil = -facing * hitRecoil * k;
            lean += 4f * k;
            squash += 0.03f * k;
        }

        if (!stepping && hitTimer <= 0f)
        {
            // Vuelve suavemente a la pose base
            visualT.localPosition = Vector3.Lerp(visualT.localPosition, Vector3.zero, dt * 16f);
            visualT.localRotation = Quaternion.Slerp(visualT.localRotation, Quaternion.identity, dt * 16f);
            visualT.localScale = Vector3.Lerp(visualT.localScale, Vector3.one, dt * 16f);
        }
        else
        {
            visualT.localPosition = new Vector3(recoil, bob, 0f);
            visualT.localRotation = Quaternion.Euler(0f, 0f, lean);
            visualT.localScale = new Vector3(1f + squash, 1f - squash, 1f);
        }

        // El ataque siempre a velocidad normal; caminar acelera un poco el idle
        if (animator != null)
        {
            bool attacking = combat != null && combat.IsAttacking;
            animator.speed = attacking ? 1f : (stepping ? 1.2f : 1f);
        }

        // Destello blanco al recibir daño + brillo dorado con MAXIMUM lleno
        Color baseColor = source.color;
        if (power != null && power.IsFull)
        {
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
            baseColor = Color.Lerp(baseColor, new Color(1f, 0.85f, 0.25f), 0.18f * wave);
        }

        if (flashTimer > 0f)
        {
            flashTimer -= dt;
            visual.color = Color.Lerp(baseColor, Color.white, 0.7f);
        }
        else
        {
            visual.color = baseColor;
        }
    }

    private void OnDisable()
    {
        if (animator != null) animator.speed = 1f;
    }
}
