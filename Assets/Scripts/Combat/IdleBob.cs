using UnityEngine;

/// <summary>
/// Agrega un movimiento de vaivén suave al personaje en reposo.
/// Simula la respiración y postura activa de un boxeador.
/// Se adapta automáticamente si FightManager reposiciona el personaje.
/// </summary>
public class IdleBob : MonoBehaviour
{
    [Header("Movimiento vertical")]
    [SerializeField] private float velocidad   = 1.8f;   // oscilaciones por segundo
    [SerializeField] private float amplitud    = 0.06f;  // metros de movimiento total

    [Header("Rotación leve")]
    [SerializeField] private float rotVelocidad = 0.9f;
    [SerializeField] private float rotAmplitud  = 0.8f;  // grados

    [Header("Offset de fase (para que no sean idénticos)")]
    [SerializeField] private float faseOffset = 0f;      // 0=Wally, 1.2=Rival

    private float baseY;
    private float offsetActual;

    private void Start()
    {
        baseY = transform.position.y;
    }

    private void Update()
    {
        float t = Time.time * velocidad + faseOffset;

        // Si FightManager reposicionó al personaje, actualizar la base
        float yReal = transform.position.y - offsetActual;
        if (Mathf.Abs(yReal - baseY) > amplitud * 2f)
            baseY = yReal;

        // Movimiento vertical
        offsetActual = Mathf.Sin(t) * amplitud;
        Vector3 pos = transform.position;
        pos.y = baseY + offsetActual;
        transform.position = pos;

        // Rotación sutil (breathing lean)
        float rot = Mathf.Sin(t * rotVelocidad) * rotAmplitud;
        transform.rotation = Quaternion.Euler(0f, 0f, rot);
    }

    /// <summary>Pausa el bob (durante ataques o knockdown).</summary>
    public void Pausar()  => enabled = false;

    /// <summary>Reanuda el bob.</summary>
    public void Reanudar() => enabled = true;
}
