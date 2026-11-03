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

    [Header("Comportamiento de pelea (KOF)")]
    [Tooltip("Tiempo en segundos que tarda en reaccionar a un ataque del jugador.")]
    public float reactionTime = 0.18f;
    [Tooltip("0 = pasivo, 1 = ataca en cuanto puede.")]
    public float aggressiveness = 0.6f;
    [Tooltip("Probabilidad de bloquear cuando el jugador ataca.")]
    public float blockChance = 0.3f;
    [Tooltip("Probabilidad de esquivar con la pose de weave.")]
    public float dodgeChance = 0.15f;
    [Tooltip("Golpes seguidos que encadena en un combo.")]
    public int comboLength = 2;

    [Header("Ataques disponibles")]
    [Tooltip("El rival elige uno al azar de esta lista en cada turno.")]
    public AttackData[] attacks;
}
