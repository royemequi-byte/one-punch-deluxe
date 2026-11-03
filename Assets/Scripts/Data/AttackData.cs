using UnityEngine;

/// <summary>
/// Define los datos de un golpe: daño, costo de stamina y tiempos de cada fase.
/// Se crea como asset en ScriptableObjects/Attacks/ y se edita desde Unity.
/// </summary>
[CreateAssetMenu(fileName = "NewAttack", menuName = "OnePunchDeluxe/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Identificación")]
    public string attackName;

    [Header("Daño y costo")]
    public int damage = 10;
    public float staminaCost = 20f;

    [Header("Tiempos del ciclo (segundos)")]
    public float windupTime = 0.1f;     // preparación antes de golpear
    public float activeTime = 0.1f;     // ventana donde puede impactar
    public float recoveryTime = 0.15f;  // vulnerable después de golpear

    [Header("Alcance e impacto")]
    [Tooltip("Distancia máxima en unidades a la que el golpe conecta.")]
    public float reach = 1.4f;
    [Tooltip("Tiempo que el rival queda aturdido al recibir el golpe.")]
    public float hitstun = 0.18f;
    [Tooltip("Tiempo que el rival queda bloqueado.")]
    public float blockstun = 0.1f;
    [Tooltip("Empuje hacia atrás al impactar.")]
    public float knockback = 0.25f;

    [Header("Animación y audio")]
    public string animationTrigger;     // nombre del trigger en el Animator
    public AudioClip hitSound;

    [Header("Esquives que anulan este golpe")]
    public DodgePosition[] dodgesThatAvoid;  // posiciones que lo esquivan
}
