// Crea clips de animación desde los sprites nuevos de Wally (5 frames)
// y un AnimatorController completo que los combina con los clips viejos.
// Ejecutar: Tools → 3 Crear Animaciones de Wally (Nuevas)
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class WallyNuevosSetup : EditorWindow
{
    private const string SPRITES_NUEVOS = "Assets/Art/Personajes/Wally_Nuevos";
    private const string SPRITES_VIEJOS = "Assets/es/Wally";   // clips originales ya existentes
    private const string ANIMS_PATH     = "Assets/Animations/Wally";

    [MenuItem("Tools/3 Crear Animaciones de Wally (Nuevas)")]
    public static void CrearAnimaciones()
    {
        // Crear carpeta
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(ANIMS_PATH))
            AssetDatabase.CreateFolder("Assets/Animations", "Wally");

        // Nuevos clips desde sprites 5-frame
        var nuevos = new (string sprite, string clipName, bool loop, int fps)[]
        {
            ("wally_idle",     "Idle",     true,  8),
            ("wally_hit",      "Hit",      false, 12),
            ("wally_hit2",     "Knockdown",false, 8),
            ("wally_block",    "Block",    true,  8),
            ("wally_straight", "Straight", false, 12),
        };

        var clips = new Dictionary<string, AnimationClip>();

        foreach (var def in nuevos)
        {
            string texPath = $"{SPRITES_NUEVOS}/{def.sprite}.png";
            Sprite[] sprites = CargarSprites(texPath);

            if (sprites.Length == 0)
            {
                Debug.LogWarning(
                    $"[Wally] Sin sprites en {texPath}. " +
                    $"Ejecuta primero Tools → 1 Configurar Sprite Sheets."
                );
                continue;
            }

            AnimationClip clip = CrearClip(def.clipName, sprites, def.loop, def.fps);
            string clipPath = $"{ANIMS_PATH}/Wally_{def.clipName}.anim";

            AnimationClip existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (existente != null)
            {
                EditorUtility.CopySerialized(clip, existente);
                AssetDatabase.SaveAssets();
                clips[def.clipName] = existente;
            }
            else
            {
                AssetDatabase.CreateAsset(clip, clipPath);
                clips[def.clipName] = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            }

            Debug.Log($"[Wally] Clip nuevo OK: {def.clipName} ({sprites.Length} frames @ {def.fps}fps)");
        }

        // Cargar clips viejos para animaciones sin sprites nuevos
        var viejosPaths = new Dictionary<string, string>
        {
            { "Jab",        $"{SPRITES_VIEJOS}/WallyJab.anim"        },
            { "Hook",       $"{SPRITES_VIEJOS}/WallyHook.anim"       },
            { "WeaveLeft",  $"{SPRITES_VIEJOS}/WallyWeaveLeft.anim"  },
            { "WeaveRight", $"{SPRITES_VIEJOS}/WallyWeaveRight.anim" },
            { "Duck",       $"{SPRITES_VIEJOS}/WallyDuck.anim"       },
            { "Backstep",   $"{SPRITES_VIEJOS}/WallyBackstep.anim"   },
            { "Slip",       $"{SPRITES_VIEJOS}/WallySlip.anim"       },
            { "Victory",    $"{SPRITES_VIEJOS}/WallyVictory.anim"    },
            { "Defeat",     $"{SPRITES_VIEJOS}/WallyDefeat.anim"     },
        };

        foreach (var kv in viejosPaths)
        {
            AnimationClip c = AssetDatabase.LoadAssetAtPath<AnimationClip>(kv.Value);
            if (c != null)
                clips[kv.Key] = c;
            else
                Debug.LogWarning($"[Wally] Clip viejo no encontrado: {kv.Value}");
        }

        if (clips.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Sin sprites",
                "No se encontraron sprites cortados.\nPrimero ejecuta:\nTools → 1 Configurar Sprite Sheets",
                "OK"
            );
            return;
        }

        // Crear AnimatorController completo
        string controllerPath = $"{ANIMS_PATH}/WallyNuevosAnimator.controller";
        CrearController(controllerPath, clips);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Wally listo",
            $"Creados {clips.Count} clips/referencias.\n\n" +
            $"AnimatorController guardado en:\n{controllerPath}\n\n" +
            $"Asígnalo al componente Animator de Wally en la escena.",
            "OK"
        );
    }

    private static Sprite[] CargarSprites(string path)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        return all.OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    private static AnimationClip CrearClip(string name, Sprite[] sprites, bool loop, int fps)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = fps;
        clip.name = name;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time  = i / (float)fps,
                value = sprites[i]
            };
        }
        keys[sprites.Length] = new ObjectReferenceKeyframe
        {
            time  = sprites.Length / (float)fps,
            value = sprites[sprites.Length - 1]
        };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);

        return clip;
    }

    private static void CrearController(string path, Dictionary<string, AnimationClip> clips)
    {
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);

        AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm = ac.layers[0].stateMachine;

        // Parámetros (mismos que el WallyAnimator original)
        string[] triggers = { "Jab", "Hook", "Straight", "Hit", "Knockdown",
                               "Victory", "Defeat", "WeaveLeft", "WeaveRight",
                               "Duck", "Backstep", "Slip" };
        foreach (string t in triggers)
            ac.AddParameter(t, AnimatorControllerParameterType.Trigger);
        ac.AddParameter("IsBlocking", AnimatorControllerParameterType.Bool);

        // Estados
        var states = new Dictionary<string, AnimatorState>();

        if (clips.TryGetValue("Idle", out AnimationClip idleClip))
        {
            AnimatorState idle = sm.AddState("Idle");
            idle.motion = idleClip;
            sm.defaultState = idle;
            states["Idle"] = idle;
        }

        string[] nombres = { "Jab", "Hook", "Straight", "Block", "Hit", "Knockdown",
                              "Victory", "Defeat", "WeaveLeft", "WeaveRight",
                              "Duck", "Backstep", "Slip" };
        foreach (string n in nombres)
        {
            if (!clips.TryGetValue(n, out AnimationClip c)) continue;
            AnimatorState st = sm.AddState(n);
            st.motion = c;
            states[n] = st;
        }

        if (!states.ContainsKey("Idle")) { AssetDatabase.SaveAssets(); return; }

        AnimatorState idleState = states["Idle"];

        // Idle → estados con trigger
        foreach (string t in triggers)
        {
            if (!states.ContainsKey(t)) continue;
            var tr = idleState.AddTransition(states[t]);
            tr.AddCondition(AnimatorConditionMode.If, 0, t);
            tr.hasExitTime = false;
            tr.duration    = 0f;
        }

        // Idle ↔ Block
        if (states.ContainsKey("Block"))
        {
            var toBlock = idleState.AddTransition(states["Block"]);
            toBlock.AddCondition(AnimatorConditionMode.If, 0, "IsBlocking");
            toBlock.hasExitTime = false;
            toBlock.duration    = 0f;

            var fromBlock = states["Block"].AddTransition(idleState);
            fromBlock.AddCondition(AnimatorConditionMode.IfNot, 0, "IsBlocking");
            fromBlock.hasExitTime = false;
            fromBlock.duration    = 0f;
        }

        // Todos los demás estados vuelven a Idle al terminar
        foreach (string n in nombres)
        {
            if (n == "Block" || !states.ContainsKey(n)) continue;
            var back = states[n].AddTransition(idleState);
            back.hasExitTime = true;
            back.exitTime    = 1f;
            back.duration    = 0f;
        }

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Wally] AnimatorController creado en: {path}");
    }
}
#endif
