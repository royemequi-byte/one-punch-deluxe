using UnityEngine;

/// <summary>
/// Datos de personalidad de un rival. Cambiar estos valores cambia la dificultad
/// sin tocar ningún código. Rival común = lento; Kirikú = agresivo y rápido.
/// </summary>
[CreateAssetMenu(fileName = "NewAIProfile", menuName = "OnePunchDeluxe/AI Profile")]
public class AIProfile : ScriptableObject
{
    [Header("Identificación")]
    public string rivalName = "Rival Común";

    [Header("Tiempos")]
    [Tooltip("Cuánto dura el gesto visible antes de golpear. Define la dificultad.")]
    public float telegraphDuration = 0.6f;  // Rival común: 0.6s / Kirikú: 0.25s

    [Tooltip("Mínimo de segundos entre ataques.")]
    public float minCooldown = 1.0f;

    [Tooltip("Máximo de segundos entre ataques.")]
    public float maxCooldown = 2.5f;

    [Header("Ataques disponibles")]
    [Tooltip("El rival elige uno al azar de esta lista en cada turno.")]
    public AttackData[] attacks;
}
