#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;

/// <summary>
/// Verifica y corrige los AnimatorControllers de Wally y Kirikú.
/// Garantiza que cada trigger tenga su transición AnyState→Estado correspondiente.
/// Uso: Tools → 15 Fix Animators
/// </summary>
public class FixAnimators : EditorWindow
{
    private const string WALLY_CTRL  = "Assets/es/Wally/Characters/Wally/WallyAnimator.controller";
    private const string KIRIKU_CTRL = "Assets/Animations/Kiriku/KirikuAnimator.controller";

    // (trigger/bool, nombre estado en el controller, es Bool no Trigger)
    private static readonly (string param, string state, bool isBool)[] WALLY_PARAMS = new[]
    {
        ("Jab",        "WallyJab",       false),
        ("Straight",   "WallyStraight",  false),
        ("Hook",       "WallyHook",      false),
        ("Hit",        "WallyHit",       false),
        ("Knockdown",  "WallyKnockdown", false),
        ("Victory",    "WallyVictory",   false),
        ("Defeat",     "WallyDefeat",    false),
        ("WeaveLeft",  "WallyWeaveLeft", false),
        ("WeaveRight", "WallyWeaveRight",false),
        ("Duck",       "WallyDuck",      false),
        ("Backstep",   "WallyBackstep",  false),
        ("Slip",       "WallySlip",      false),
        ("IsBlocking", "WallyBlock",     true ),
    };

    private static readonly (string param, string state, bool isBool)[] KIRIKU_PARAMS = new[]
    {
        ("Hit",        "Hit",       false),
        ("Knockdown",  "Knockdown", false),
        ("Victory",    "Victory",   false),
        ("Defeat",     "Defeat",    false),
        ("Jab",        "Jab",       false),
        ("Straight",   "Straight",  false),
        ("Hook",       "Hook",      false),
        ("IsBlocking", "Block",     true ),
    };

    [MenuItem("Tools/15 Fix Animators")]
    public static void Fix()
    {
        int totalFix = 0;

        totalFix += CorregirController(WALLY_CTRL,  WALLY_PARAMS,  "Wally");
        totalFix += CorregirController(KIRIKU_CTRL, KIRIKU_PARAMS, "Kirikú");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Animators corregidos",
            $"Se aplicaron {totalFix} correcciones.\n\n" +
            "- Todos los triggers tienen transición AnyState→Estado\n" +
            "- Las transiciones de ataque/defensa no esperan ExitTime\n" +
            "- Velocidad de cada clip restaurada a 1.0\n\n" +
            "Guarda con Ctrl+S y dale Play para probar con J/K/L.", "OK");
    }

    private static int CorregirController(
        string path,
        (string param, string state, bool isBool)[] tabla,
        string nombreLog)
    {
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (ctrl == null)
        {
            Debug.LogWarning($"[FixAnimators] No encontrado: {path}");
            return 0;
        }

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        int fixes = 0;

        foreach (var (param, stateName, isBool) in tabla)
        {
            // ── 1. Asegurar que el parámetro existe ────────────────
            bool tieneParam = ctrl.parameters.Any(p => p.name == param);
            if (!tieneParam)
            {
                var tipo = isBool ? AnimatorControllerParameterType.Bool
                                  : AnimatorControllerParameterType.Trigger;
                ctrl.AddParameter(param, tipo);
                Debug.Log($"[FixAnimators] {nombreLog}: añadido parámetro '{param}'");
                fixes++;
            }

            // ── 2. Buscar el estado destino ────────────────────────
            ChildAnimatorState? childState = sm.states
                .Cast<ChildAnimatorState?>()
                .FirstOrDefault(cs => cs.HasValue && cs.Value.state.name == stateName);

            if (!childState.HasValue)
            {
                Debug.LogWarning($"[FixAnimators] {nombreLog}: estado '{stateName}' no encontrado — omitido.");
                continue;
            }

            AnimatorState estado = childState.Value.state;

            // Corregir velocidad si está distorsionada
            if (!Mathf.Approximately(estado.speed, 1f))
            {
                estado.speed = 1f;
                EditorUtility.SetDirty(estado);
                fixes++;
            }

            // ── 3. ¿Ya existe una transición AnyState → este estado? ─
            bool yaExiste = sm.anyStateTransitions
                .Any(t => t.destinationState == estado &&
                          t.conditions.Any(c => c.parameter == param));

            if (yaExiste) continue;

            // ── 4. Crear la transición con la condición correcta ───
            AnimatorStateTransition tr = sm.AddAnyStateTransition(estado);
            tr.hasExitTime    = false;
            tr.duration       = 0f;
            tr.canTransitionToSelf = false;

            if (isBool)
                tr.AddCondition(AnimatorConditionMode.If, 0, param);
            else
                tr.AddCondition(AnimatorConditionMode.If, 0, param);

            EditorUtility.SetDirty(ctrl);
            Debug.Log($"[FixAnimators] {nombreLog}: AnyState → {stateName} via '{param}'");
            fixes++;
        }

        // Asegurar que la transición de regreso a Idle usa ExitTime en estados de ataque
        AsegurarRetornoIdle(sm, ctrl);

        return fixes;
    }

    private static void AsegurarRetornoIdle(AnimatorStateMachine sm, AnimatorController ctrl)
    {
        AnimatorState idle = sm.states
            .Select(cs => cs.state)
            .FirstOrDefault(s => s.name.Contains("Idle"));

        if (idle == null) return;

        foreach (ChildAnimatorState child in sm.states)
        {
            AnimatorState estado = child.state;
            if (estado == idle) continue;

            bool esBloqueo = estado.name.Contains("Block");

            // Estados de bloqueo: salir cuando IsBlocking = false
            if (esBloqueo)
            {
                bool tieneIsBlockingFalse = estado.transitions
                    .Any(t => t.conditions.Any(c => c.parameter == "IsBlocking" &&
                                                    c.mode == AnimatorConditionMode.IfNot));
                if (!tieneIsBlockingFalse)
                {
                    bool tieneParam = ctrl.parameters.Any(p => p.name == "IsBlocking");
                    if (tieneParam)
                    {
                        AnimatorStateTransition tr = estado.AddTransition(idle);
                        tr.hasExitTime = false;
                        tr.duration    = 0.05f;
                        tr.AddCondition(AnimatorConditionMode.IfNot, 0, "IsBlocking");
                        EditorUtility.SetDirty(estado);
                    }
                }
                continue;
            }

            // Resto de estados: retorno por ExitTime si no tienen ninguna salida
            bool tieneSalida = estado.transitions.Length > 0;
            if (!tieneSalida)
            {
                AnimatorStateTransition tr = estado.AddTransition(idle);
                tr.hasExitTime = true;
                tr.exitTime    = 0.9f;
                tr.duration    = 0.1f;
                EditorUtility.SetDirty(estado);
            }
        }
    }
}
#endif
