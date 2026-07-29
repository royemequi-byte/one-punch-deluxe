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

    [Header("Animación y audio")]
    public string animationTrigger;     // nombre del trigger en el Animator
    public AudioClip hitSound;

    [Header("Esquives que anulan este golpe")]
    public DodgePosition[] dodgesThatAvoid;  // posiciones que lo esquivan
}
