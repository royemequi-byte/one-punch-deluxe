using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lee el input del jugador y llama al CombatController con el ataque correcto.
/// Es el único script que toca el teclado — el resto del juego no sabe qué tecla se presionó.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CombatController combatController;

    [Header("Ataques asignados a cada tecla")]
    [SerializeField] private AttackData jabData;       // tecla J
    [SerializeField] private AttackData straightData;  // tecla K
    [SerializeField] private AttackData hookData;      // tecla L

    [Header("Posición de esquive actual")]
    // La posición actual se informa al CombatController del rival
    // para que sepa si el jugador está esquivando su golpe
    private DodgePosition currentDodge = DodgePosition.Neutral;

    private void Update()
    {
        LeerAtaques();
        LeerEsquives();
    }

    private void LeerAtaques()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
            combatController.TryAttack(jabData);

        if (Keyboard.current.kKey.wasPressedThisFrame)
            combatController.TryAttack(straightData);

        if (Keyboard.current.lKey.wasPressedThisFrame)
            combatController.TryAttack(hookData);
    }

    private void LeerEsquives()
    {
        // Mientras se mantiene la tecla, Wally está en esa posición de esquive
        if (Keyboard.current.aKey.isPressed)
            SetDodge(DodgePosition.WeaveLeft);
        else if (Keyboard.current.dKey.isPressed)
            SetDodge(DodgePosition.WeaveRight);
        else if (Keyboard.current.sKey.isPressed)
            SetDodge(DodgePosition.Duck);
        else if (Keyboard.current.wKey.isPressed)
            SetDodge(DodgePosition.Backstep);
        else
            SetDodge(DodgePosition.Neutral);
    }

    private void SetDodge(DodgePosition dodge)
    {
        if (currentDodge == dodge) return;
        currentDodge = dodge;

        // Informa al CombatController del rival la posición actual de Wally
        // para que evalúe si sus golpes impactan o no
        // (se conecta en FightManager cuando tengamos los dos peleadores)
    }

    /// <summary>
    /// Devuelve la posición de esquive actual del jugador.
    /// La consulta el CombatController del rival.
    /// </summary>
    public DodgePosition GetCurrentDodge() => currentDodge;
}
