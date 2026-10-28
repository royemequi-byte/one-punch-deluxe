using UnityEngine;

/// <summary>
/// Estadísticas base de un peleador.
/// Se crea en ScriptableObjects/Fighters/ y se asigna al prefab del personaje.
/// Cambiar estos valores cambia el balance sin tocar código.
/// </summary>
[CreateAssetMenu(fileName = "NewFighter", menuName = "OnePunchDeluxe/Fighter Data")]
public class FighterData : ScriptableObject
{
    [Header("Identificación")]
    public string fighterName = "Peleador";

    [Tooltip("Retrato que aparece en el HUD junto a las barras de vida.")]
    public Sprite portrait;

    [Header("Estadísticas base")]
    [Tooltip("Vida máxima del peleador.")]
    [Range(50, 300)]
    public int maxHealth = 100;

    [Tooltip("Stamina máxima. Sin stamina no puede atacar ni bloquear.")]
    [Range(50, 200)]
    public int maxStamina = 100;

    [Tooltip("Multiplicador de daño aplicado a todos sus ataques.")]
    [Range(0.5f, 3f)]
    public float attackPower = 1f;

    [Tooltip("Reduce el daño recibido. 0.25 = absorbe 25% del golpe al bloquear.")]
    [Range(0f, 1f)]
    public float defense = 0.25f;

    [Header("Animaciones")]
    [Tooltip("Animator Controller con todos los clips de este personaje.")]
    public RuntimeAnimatorController animatorController;
}
