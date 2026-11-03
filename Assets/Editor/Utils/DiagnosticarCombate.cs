#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Text;
using System.Linq;

/// <summary>
/// Diagnóstico completo del combate: revisa referencias, animadores, sprites y datos.
/// Uso: Tools → 00 Diagnosticar Combate
/// </summary>
public class DiagnosticarCombate : EditorWindow
{
    [MenuItem("Tools/00 Diagnosticar Combate")]
    public static void Diagnosticar()
    {
        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════════");
        sb.AppendLine("   DIAGNÓSTICO ONE PUNCH DELUXE");
        sb.AppendLine("══════════════════════════════════════\n");

        bool hayErrores = false;

        // ── 1. GameObjects en escena ───────────────────────────────────
        sb.AppendLine("── OBJETOS EN ESCENA ──");
        string[] requeridos = { "Wally", "Kiriku", "FightManager", "HUD" };
        foreach (string nombre in requeridos)
        {
            GameObject go = GameObject.Find(nombre);
            string estado = go != null ? "✓" : "✗ FALTA";
            if (go == null) hayErrores = true;
            sb.AppendLine($"  {estado}  {nombre}");
        }

        GameObject wally  = GameObject.Find("Wally");
        GameObject kiriku = GameObject.Find("Kiriku");

        // ── 2. Componentes de Wally ────────────────────────────────────
        sb.AppendLine("\n── COMPONENTES WALLY ──");
        if (wally != null)
        {
            hayErrores |= CheckComp<Health>(wally,              sb, "Health");
            hayErrores |= CheckComp<Stamina>(wally,             sb, "Stamina");
            hayErrores |= CheckComp<CombatController>(wally,    sb, "CombatController");
            hayErrores |= CheckComp<PlayerInputHandler>(wally,  sb, "PlayerInputHandler");
            hayErrores |= CheckComp<Animator>(wally,            sb, "Animator");

            Animator wa = wally.GetComponent<Animator>();
            if (wa != null)
                InspeccionarAnimator(wa, sb, ref hayErrores,
                    new[]{ "Jab","Straight","Hook","Hit","Victory","Knockdown","Defeat",
                           "WeaveLeft","WeaveRight","Duck","Backstep" },
                    new[]{ "IsBlocking" });
        }
        else sb.AppendLine("  (Wally no encontrado)");

        // ── 3. Componentes de Kirikú ───────────────────────────────────
        sb.AppendLine("\n── COMPONENTES KIRIKÚ ──");
        if (kiriku != null)
        {
            hayErrores |= CheckComp<Health>(kiriku,           sb, "Health");
            hayErrores |= CheckComp<Stamina>(kiriku,          sb, "Stamina");
            hayErrores |= CheckComp<CombatController>(kiriku, sb, "CombatController");
            hayErrores |= CheckComp<RivalAI>(kiriku,          sb, "RivalAI");
            hayErrores |= CheckComp<Animator>(kiriku,         sb, "Animator");

            Animator ka = kiriku.GetComponent<Animator>();
            if (ka != null)
                InspeccionarAnimator(ka, sb, ref hayErrores,
                    new[]{ "Jab","Straight","Hook","Hit","Victory","Knockdown","Defeat",
                           "Telegraph","WeaveLeft","WeaveRight","Duck","Backstep" },
                    new string[]{});

            // Verificar que RivalAI tiene perfil y ataques
            RivalAI ai = kiriku.GetComponent<RivalAI>();
            if (ai != null)
            {
                SerializedObject so = new SerializedObject(ai);
                object perfil = so.FindProperty("profile")?.objectReferenceValue;
                object combate = so.FindProperty("combatController")?.objectReferenceValue;
                sb.AppendLine($"  {(perfil  != null ? "✓" : "✗ NULL")}  RivalAI.profile");
                sb.AppendLine($"  {(combate != null ? "✓" : "✗ NULL")}  RivalAI.combatController");
                if (perfil == null || combate == null) hayErrores = true;

                AIProfile profile = so.FindProperty("profile")?.objectReferenceValue as AIProfile;
                if (profile != null)
                {
                    int numAtaques = profile.attacks?.Length ?? 0;
                    string tag = numAtaques > 0 ? "✓" : "✗ 0 ATAQUES";
                    if (numAtaques == 0) hayErrores = true;
                    sb.AppendLine($"  {tag}  AIProfile.attacks ({numAtaques} asignados)");
                    sb.AppendLine($"       telegraph={profile.telegraphDuration}s  cooldown={profile.minCooldown}-{profile.maxCooldown}s");
                }
            }
        }
        else sb.AppendLine("  (Kirikú no encontrado)");

        // ── 4. AttackData assets ───────────────────────────────────────
        sb.AppendLine("\n── ATTACK DATA ──");
        string[] ataques = { "JabData", "StraightData", "HookData" };
        foreach (string a in ataques)
        {
            string path = $"Assets/ScriptableObjects/Attacks/{a}.asset";
            AttackData data = AssetDatabase.LoadAssetAtPath<AttackData>(path);
            if (data == null) { sb.AppendLine($"  ✗ NO EXISTE  {a}"); hayErrores = true; continue; }
            sb.AppendLine($"  ✓  {a}: dmg={data.damage} stamina={data.staminaCost} " +
                          $"total={(data.windupTime+data.activeTime+data.recoveryTime):F2}s trigger='{data.animationTrigger}'");
        }

        // ── 5. Sprites cargados correctamente ─────────────────────────
        sb.AppendLine("\n── SPRITES (frames por sheet) ──");
        string[,] sheets = {
            { "Assets/Art/Personajes/Wally/wally_idle.png",     "wally_idle"     },
            { "Assets/Art/Personajes/Wally/wally_jab.png",      "wally_jab"      },
            { "Assets/Art/Personajes/Wally/wally_straight.png", "wally_straight" },
            { "Assets/Art/Personajes/Wally/wally_hook.png",     "wally_hook"     },
            { "Assets/Art/Personajes/Wally/wally_hit.png",      "wally_hit"      },
            { "Assets/Art/Personajes/Kiriku/kiriku_idle.png",   "kiriku_idle"    },
            { "Assets/Art/Personajes/Kiriku/kiriku_jab.png",    "kiriku_jab"     },
            { "Assets/Art/Personajes/Kiriku/kiriku_hook.png",   "kiriku_hook"    },
        };
        for (int i = 0; i < sheets.GetLength(0); i++)
        {
            string spPath = sheets[i, 0];
            var sprites = AssetDatabase.LoadAllAssetsAtPath(spPath)
                          .OfType<UnityEngine.Sprite>().ToArray();
            string ok = sprites.Length > 1 ? "✓" : "✗ 1 FRAME (sin slicing)";
            if (sprites.Length <= 1) hayErrores = true;
            sb.AppendLine($"  {ok}  {sheets[i,1]}: {sprites.Length} frames");
        }

        // ── 6. Clips de animación ──────────────────────────────────────
        sb.AppendLine("\n── CLIPS DE ANIMACIÓN (frames asignados) ──");
        string[,] clips = {
            { "Assets/es/Wally/WallyIdle.anim",      "WallyIdle"      },
            { "Assets/es/Wally/WallyJab.anim",       "WallyJab"       },
            { "Assets/es/Wally/WallyStraight.anim",  "WallyStraight"  },
            { "Assets/es/Wally/WallyHit.anim",       "WallyHit"       },
            { "Assets/Animations/Kiriku/Kiriku_Idle.anim",     "Kiriku_Idle"     },
            { "Assets/Animations/Kiriku/Kiriku_Jab.anim",      "Kiriku_Jab"      },
            { "Assets/Animations/Kiriku/Kiriku_Straight.anim", "Kiriku_Straight" },
            { "Assets/Animations/Kiriku/Kiriku_Hook.anim",     "Kiriku_Hook"     },
        };
        for (int i = 0; i < clips.GetLength(0); i++)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clips[i, 0]);
            if (clip == null) { sb.AppendLine($"  ✗  {clips[i,1]}: NO EXISTE"); hayErrores = true; continue; }
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            int frames = bindings.Length > 0
                ? AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]).Length
                : 0;
            string ok = frames > 1 ? "✓" : "✗ VACÍO";
            if (frames <= 1) hayErrores = true;
            sb.AppendLine($"  {ok}  {clips[i,1]}: {frames} keyframes  {clip.length:F2}s");
        }

        // ── Resumen ────────────────────────────────────────────────────
        sb.AppendLine("\n══════════════════════════════════════");
        sb.AppendLine(hayErrores
            ? "⚠  HAY PROBLEMAS — revisa los ✗ arriba"
            : "✓  TODO CONECTADO — el juego debería funcionar");
        sb.AppendLine("══════════════════════════════════════");

        // Mostrar en consola (más legible que un dialog largo)
        Debug.Log(sb.ToString());

        // Dialog corto
        EditorUtility.DisplayDialog("Diagnóstico",
            hayErrores
                ? "HAY ERRORES.\nRevisa la Consola de Unity (Ctrl+Shift+C)\npara ver exactamente qué falta."
                : "¡Todo conectado!\nSi el juego se siente trabado es un problema\ndel Animator (transitions).",
            "OK");
    }

    private static bool CheckComp<T>(GameObject go, StringBuilder sb, string nombre) where T : Component
    {
        bool falta = go.GetComponent<T>() == null;
        sb.AppendLine($"  {(falta ? "✗ FALTA" : "✓")}  {nombre}");
        return falta;
    }

    private static void InspeccionarAnimator(Animator anim, StringBuilder sb,
        ref bool hayErrores, string[] triggersEsperados, string[] booleansEsperados)
    {
        RuntimeAnimatorController rac = anim.runtimeAnimatorController;
        if (rac == null)
        {
            sb.AppendLine("  ✗  Animator sin controller asignado");
            hayErrores = true;
            return;
        }
        sb.AppendLine($"  ✓  Animator controller: {rac.name}");

        AnimatorController ac = rac as AnimatorController;
        if (ac == null) return;

        // Verificar parámetros
        var paramNames = ac.parameters.Select(p => p.name).ToHashSet();
        foreach (string t in triggersEsperados)
        {
            bool tiene = paramNames.Contains(t);
            if (!tiene) hayErrores = true;
            sb.AppendLine($"    {(tiene ? "✓" : "✗ FALTA TRIGGER")}  {t}");
        }
        foreach (string b in booleansEsperados)
        {
            bool tiene = paramNames.Contains(b);
            if (!tiene) hayErrores = true;
            sb.AppendLine($"    {(tiene ? "✓" : "✗ FALTA BOOL")}  {b}");
        }
    }
}
#endif
