using UnityEngine;

/// <summary>
/// Movimiento del peleador: caminar, dash, salto y límites del ring.
/// No usa Rigidbody: mueve el Transform para que el control sea exacto,
/// como en los juegos de pelea 2D clásicos.
/// </summary>
public class FighterBody : MonoBehaviour
{
    [Header("Caminar")]
    [SerializeField] private float walkSpeed = 2.4f;
    [SerializeField] private float backSpeed = 1.9f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 7f;
    [SerializeField] private float dashTime = 0.18f;
    [SerializeField] private float dashCooldown = 0.35f;

    [Header("Salto")]
    [SerializeField] private float jumpSpeed = 7.4f;
    [SerializeField] private float gravity = 22f;

    [Header("Límites del ring")]
    [SerializeField] private float minX = -6.1f;
    [SerializeField] private float maxX = 6.1f;
    [SerializeField] private float groundY = -3f;

    private float baseScaleX;
    private float verticalSpeed;
    private float dashTimer;
    private float dashCooldownTimer;
    private float moveInput;
    private int facing = 1;

    public bool IsGrounded => transform.position.y <= groundY + 0.001f;
    public bool IsDashing => dashTimer > 0f;
    public int Facing => facing;
    public float X => transform.position.x;

    /// <summary>Cuánto se está moviendo (0 a 1). Lo usa FighterVisual para el paso.</summary>
    public float MoveAmount => Mathf.Abs(moveInput);

    /// <summary>Dirección de movimiento (-1, 0, 1). Sirve para caminar hacia atrás.</summary>
    public float MoveInput => moveInput;

    private void Awake()
    {
        baseScaleX = Mathf.Abs(transform.localScale.x);
        facing = transform.localScale.x < 0f ? -1 : 1;

        // El acabado visual (paso, inclinación, polvo) se añade solo
        if (GetComponent<FighterVisual>() == null) gameObject.AddComponent<FighterVisual>();
    }

    /// <summary>Orientación visual. 1 = derecha, -1 = izquierda.</summary>
    public void SetFacing(int dir)
    {
        if (dir == 0 || dir == facing) return;
        facing = dir;
        transform.localScale = new Vector3(baseScaleX * facing, transform.localScale.y, transform.localScale.z);
    }

    /// <summary>Dirección de caminar de este frame (-1, 0 o 1).</summary>
    public void Move(float dir)
    {
        moveInput = Mathf.Clamp(dir, -1f, 1f);
    }

    /// <summary>Salta si está en el suelo.</summary>
    public void Jump()
    {
        if (!IsGrounded) return;
        verticalSpeed = jumpSpeed;
    }

    /// <summary>Arranque corto hacia un lado. Doble toque en el mando.</summary>
    public void Dash(int dir)
    {
        if (dashCooldownTimer > 0f || !IsGrounded) return;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;
        moveInput = dir;
        SetFacing(dir);
    }

    /// <summary>Empuje por bloqueo o golpe. Respeta los límites del ring.</summary>
    public void Push(float deltaX)
    {
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x + deltaX, minX, maxX);
        transform.position = p;
    }

    /// <summary>Vuelve al suelo y a una posición concreta (inicio de round).</summary>
    public void ResetTo(float x)
    {
        transform.position = new Vector3(x, groundY, transform.position.z);
        verticalSpeed = 0f;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
        moveInput = 0f;
        transform.localScale = new Vector3(baseScaleX * facing, transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dashCooldownTimer > 0f) dashCooldownTimer -= dt;

        float speed = moveInput < 0f ? backSpeed : walkSpeed;
        if (dashTimer > 0f)
        {
            dashTimer -= dt;
            speed = dashSpeed;
        }

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x + moveInput * speed * dt, minX, maxX);

        if (!IsGrounded || verticalSpeed > 0f)
        {
            verticalSpeed -= gravity * dt;
            p.y += verticalSpeed * dt;
            if (p.y <= groundY)
            {
                p.y = groundY;
                verticalSpeed = 0f;
            }
        }

        transform.position = p;
    }
}
