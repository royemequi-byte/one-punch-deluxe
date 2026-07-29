using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lee el teclado y traduce las teclas en acciones de combate.
/// También informa al rival la posición de esquive y bloqueo de Wally.
/// Teclas: J=Jab, K=Recto, L=Gancho, WASD=Esquive, Space=Bloqueo.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Combate de Wally")]
    [SerializeField] private CombatController wallyCombat;
    [SerializeField] private Stamina wallyStamina;

    [Header("Ataques")]
    [SerializeField] private AttackData jabData;       // tecla J
    [SerializeField] private AttackData straightData;  // tecla K
    [SerializeField] private AttackData hookData;      // tecla L

    [Header("Rival (recibe info de esquive y bloqueo)")]
    [SerializeField] private CombatController rivalCombat;

    [Header("Bloqueo")]
    [Tooltip("Stamina que consume mantener el bloqueo por segundo.")]
    [SerializeField] private float blockStaminaDrain = 10f;

    private DodgePosition currentDodge = DodgePosition.Neutral;

    private void Update()
    {
        LeerAtaques();
        LeerEsquives();
        LeerBloqueo();
    }

    private void LeerAtaques()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
            wallyCombat.TryAttack(jabData);

        if (Keyboard.current.kKey.wasPressedThisFrame)
            wallyCombat.TryAttack(straightData);

        if (Keyboard.current.lKey.wasPressedThisFrame)
            wallyCombat.TryAttack(hookData);
    }

    private void LeerEsquives()
    {
        if (Keyboard.current == null) return;

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

    private void LeerBloqueo()
    {
        if (Keyboard.current == null) return;

        bool quiereBloquear = Keyboard.current.spaceKey.isPressed;

        // Solo puede bloquear si tiene stamina
        bool bloqueando = quiereBloquear && wallyStamina.HasStamina(1f);

        if (bloqueando)
        {
            // Drenar stamina mientras mantiene el bloqueo
            wallyStamina.UseStamina(blockStaminaDrain * Time.deltaTime);
        }

        // Informar al rival si Wally está bloqueando
        rivalCombat?.SetTargetBlocking(bloqueando);
    }

    private void SetDodge(DodgePosition dodge)
    {
        if (currentDodge == dodge) return;
        currentDodge = dodge;

        // Informar al rival la posición actual de Wally
        rivalCombat?.SetTargetDodge(currentDodge);

        if (dodge != DodgePosition.Neutral)
            Debug.Log($"[Esquive] Wally → {dodge}");
    }

    /// <summary>
    /// Devuelve la posición de esquive actual. La consulta la IA del rival.
    /// </summary>
    public DodgePosition GetCurrentDodge() => currentDodge;
}
