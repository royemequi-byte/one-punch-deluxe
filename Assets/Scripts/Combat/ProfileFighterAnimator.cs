using UnityEngine;

/// <summary>
/// Animador de perfil: reproduce las hojas nuevas (idle, caminar y golpe) que
/// están en Resources/Profile. Sustituye al Animator clásico durante la pelea.
/// Todo el indexado está acotado a propósito: si una hoja viene incompleta,
/// la animación se queda en el frame válido anterior en vez de romperse.
/// </summary>
public class ProfileFighterAnimator : MonoBehaviour
{
    [Header("Ritmo")]
    [SerializeField] private float idleFps = 10f;
    [SerializeField] private float walkFps = 13f;
    [SerializeField] private float pixelsPerUnit = 300f;

    private Sprite[] idleFrames;
    private Sprite[] walkFrames;
    private Sprite[] punchFrames;
    private Sprite[] hitFrames;
    private Sprite[] blockFrames;
    private Sprite[] knockdownFrames;
    private SpriteRenderer source;
    private CombatController combat;
    private FighterBody body;
    private Health health;

    private float frameTimer;
    private int frameIndex;
    private bool mirrorPunch;

    private bool hayIdle;
    private bool hayWalk;
    private bool hayHit;
    private bool hayBlock;
    private bool hayCaida;
    private float caidaTimer;
    private bool caidaIniciada;

    /// <summary>True si hay hoja de caída: entonces FighterVisual no rota el sprite.</summary>
    public bool TieneCaida => hayCaida;
    private int  blockIndex;
    private bool bloqueandoAntes;

    // Cuánto dura la reacción al golpe (4 poses a 14 fps ≈ 0.29 s)
    private float hitTimer;
    private const float HitDuracion = 0.34f;

    private void Awake()
    {
        source = GetComponent<SpriteRenderer>();
        combat = GetComponent<CombatController>();
        body   = GetComponent<FighterBody>();
        health = GetComponent<Health>();

        string personaje = gameObject.name.ToLower().Contains("wally") ? "Wally" : "Kiriku";

        idleFrames  = LoadFrames("Profile/" + personaje + "/idle", 8);
        walkFrames  = LoadFrames("Profile/" + personaje + "/walk", 8);
        punchFrames = LoadFrames("Profile/" + personaje + "/punch", 6);
        hitFrames   = LoadFrames("Profile/" + personaje + "/hit", 4);
        blockFrames = LoadFrames("Profile/" + personaje + "/block", 4);
        knockdownFrames = LoadFrames("Profile/" + personaje + "/knockdown", 6);

        hayIdle  = TieneFrames(idleFrames);
        hayWalk  = TieneFrames(walkFrames);
        hayHit   = TieneFrames(hitFrames);
        hayBlock = TieneFrames(blockFrames);
        hayCaida = TieneFrames(knockdownFrames);

        // La hoja de golpe de Kirikú viene mirando al lado contrario que su idle.
        mirrorPunch = personaje == "Kiriku";

        // Solo apagamos el Animator si las hojas nuevas cargaron bien.
        if (hayIdle)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (health != null) health.OnDamage += AlRecibirGolpe;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDamage -= AlRecibirGolpe;
    }

    /// <summary>Dispara la reacción de golpe recibido (hoja "hit").</summary>
    private void AlRecibirGolpe()
    {
        if (hayHit) hitTimer = HitDuracion;
    }

    private static bool TieneFrames(Sprite[] frames)
    {
        return frames != null && frames.Length > 0 && frames[0] != null;
    }

    /// <summary>Primer sprite válido de la lista, o null si no hay ninguno.</summary>
    private static Sprite PrimerValido(Sprite[] frames)
    {
        if (frames == null) return null;
        for (int i = 0; i < frames.Length; i++)
            if (frames[i] != null) return frames[i];
        return null;
    }

    private Sprite Buscar(Sprite[] frames, int index)
    {
        if (frames == null || frames.Length == 0) return null;
        int i = Mathf.Clamp(index, 0, frames.Length - 1);
        return frames[i];
    }

    private Sprite[] LoadFrames(string path, int count)
    {
        Sprite[] result = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            // Preferimos el Sprite ya importado (PPU y pivote vienen del .meta).
            // Funciona en build sin depender de que la textura sea legible.
            Sprite existing = Resources.Load<Sprite>(path + "_" + i);
            if (existing != null) { result[i] = existing; continue; }

            // Respaldo: crear el sprite desde la textura
            Texture2D tex = Resources.Load<Texture2D>(path + "_" + i);
            if (tex == null) continue;

            result[i] = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 20f / 620f),
                pixelsPerUnit);
        }
        return result;
    }

    private void Update()
    {
        if (source == null || !hayIdle) return;

        // K.O.: si hay hoja de caída se reproduce entera y se queda tirado en el suelo
        if (health != null && health.IsDead)
        {
            if (!hayCaida) return;

            if (!caidaIniciada)
            {
                caidaIniciada = true;
                caidaTimer    = 0f;
            }

            caidaTimer += Time.deltaTime;

            const float fpsCaida = 11f;
            int totalCaida = knockdownFrames.Length;
            int indexCaida = Mathf.Clamp(Mathf.FloorToInt(caidaTimer * fpsCaida), 0, totalCaida - 1);

            Sprite caida = Buscar(knockdownFrames, indexCaida) ?? PrimerValido(knockdownFrames);
            if (caida != null)
            {
                source.sprite = caida;
                source.flipX  = false;
            }
            return;
        }

        // Sigue vivo: la próxima caída empieza desde el principio
        caidaIniciada = false;
        caidaTimer    = 0f;

        // 0. Golpe RECIBIDO: tiene prioridad sobre todo lo demás
        if (hitTimer > 0f && hayHit)
        {
            hitTimer -= Time.deltaTime;

            float avance = 1f - Mathf.Clamp01(hitTimer / HitDuracion);
            int total = hitFrames.Length;
            int index = Mathf.Clamp(Mathf.FloorToInt(avance * total), 0, total - 1);

            Sprite reaccion = Buscar(hitFrames, index) ?? PrimerValido(hitFrames);
            if (reaccion != null)
            {
                source.sprite = reaccion;
                source.flipX  = false;
                return;
            }
        }

        // 1. Golpe: recorre las poses según el progreso real del ataque
        if (combat != null && combat.IsAttacking && TieneFrames(punchFrames))
        {
            int total = punchFrames.Length;
            int index = Mathf.Clamp(Mathf.FloorToInt(combat.AttackProgress * total), 0, total - 1);
            Sprite pose = Buscar(punchFrames, index) ?? PrimerValido(punchFrames);

            if (pose != null)
            {
                source.sprite = pose;
                source.flipX  = mirrorPunch;
                return;
            }
        }

        // 2. Guardia: la hoja de bloqueo hace un movimiento defensivo suave
        if (combat != null && combat.IsBlocking)
        {
            if (hayBlock)
            {
                if (!bloqueandoAntes)
                {
                    bloqueandoAntes = true;
                    blockIndex = 0;
                    frameTimer = 0f;
                }

                frameTimer += Time.deltaTime;
                float paso = 1f / 6f;
                if (frameTimer >= paso)
                {
                    frameTimer -= paso;
                    blockIndex = (blockIndex + 1) % blockFrames.Length;
                }

                Sprite guardia = Buscar(blockFrames, blockIndex) ?? PrimerValido(blockFrames);
                if (guardia != null)
                {
                    source.sprite = guardia;
                    source.flipX  = false;
                    return;
                }
            }

            source.sprite = PrimerValido(idleFrames);
            source.flipX  = false;
            return;
        }

        bloqueandoAntes = false;

        // 3. Caminar: frames de paso. Hacia atrás se reproduce al revés.
        bool moving = body != null && body.MoveAmount > 0.05f && body.IsGrounded;
        if (moving && hayWalk)
        {
            bool backwards = body.MoveInput * body.Facing < 0f;
            float fps = backwards ? walkFps * 0.85f : walkFps;

            frameTimer += Time.deltaTime;
            float step = 1f / Mathf.Max(1f, fps);
            if (frameTimer >= step)
            {
                frameTimer -= step;
                frameIndex = (frameIndex + 1) % Mathf.Max(1, walkFrames.Length * 16);
            }

            int index = frameIndex % Mathf.Max(1, walkFrames.Length);
            if (backwards) index = walkFrames.Length - 1 - index;

            Sprite paso = Buscar(walkFrames, index);
            if (paso != null)
            {
                source.sprite = paso;
                source.flipX  = false;
                return;
            }
        }

        // 4. Idle
        if (!TieneFrames(idleFrames)) return;

        frameTimer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, idleFps);
        if (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            frameIndex = (frameIndex + 1) % Mathf.Max(1, idleFrames.Length);
        }

        Sprite reposo = Buscar(idleFrames, frameIndex) ?? PrimerValido(idleFrames);
        if (reposo != null)
        {
            source.sprite = reposo;
            source.flipX  = false;
        }
    }
}
