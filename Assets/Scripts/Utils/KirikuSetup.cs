// Crea automáticamente todos los clips de animación de Kirikú y su AnimatorController.
// Ejecutar DESPUÉS de "Tools → 1 Configurar Sprite Sheets".
// Uso: Tools → 2 Crear Animaciones de Kiriku
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class KirikuSetup : EditorWindow
{
    private const string SPRITES_PATH = "Assets/Art/Personajes/Kiriku";
    private const string ANIMS_PATH   = "Assets/Animations/Kiriku";

    [MenuItem("Tools/2 Crear Animaciones de Kiriku")]
    public static void CrearAnimaciones()
    {
        // Crear carpeta de animaciones
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(ANIMS_PATH))
            AssetDatabase.CreateFolder("Assets/Animations", "Kiriku");

        // Tabla de animaciones: (archivo, nombre del clip, ¿loop?, fps)
        // Los triggers del AnimatorController usan exactamente estos nombres.
        var defs = new (string sprite, string clipName, bool loop, int fps)[]
        {
            ("kiriku_idle",      "Idle",       true,  8),
            ("kiriku_jab",       "Jab",        false, 12),
            ("kiriku_hook",      "Hook",       false, 12),
            ("kiriku_attack",    "Straight",   false, 12),
            ("kiriku_block",     "Block",      true,  8),
            ("kiriku_hit",       "Hit",        false, 12),
            ("kiriku_knockdown", "Knockdown",  false, 8),
            ("kiriku_telegraph", "Telegraph",  false, 8),
            ("kiriku_weave",     "Weave",      false, 12),
            ("kiriku_hit2",      "Victory",    true,  8),   // hit2 como Victory provisional
            ("kiriku_idle3",     "Defeat",     false, 8),   // idle3 como Defeat provisional
        };

        var clips = new Dictionary<string, AnimationClip>();

        foreach (var def in defs)
        {
            string texPath = $"{SPRITES_PATH}/{def.sprite}.png";
            Sprite[] sprites = CargarSprites(texPath);

            if (sprites.Length == 0)
            {
                Debug.LogWarning(
                    $"[Kirikú] Sin sprites en {texPath}. " +
                    $"Ejecuta primero Tools → 1 Configurar Sprite Sheets."
                );
                continue;
            }

            AnimationClip clip = CrearClip(def.clipName, sprites, def.loop, def.fps);
            string clipPath = $"{ANIMS_PATH}/Kiriku_{def.clipName}.anim";

            // Guardar en disco (reemplazar si ya existe)
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

            Debug.Log($"[Kirikú] Clip OK: {def.clipName} ({sprites.Length} frames @ {def.fps}fps)");
        }

        if (clips.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Sin sprites",
                "No se encontraron sprites cortados.\n\nPrimero ejecuta:\nTools → 1 Configurar Sprite Sheets",
                "OK"
            );
            return;
        }

        // Crear el AnimatorController con todos los clips y transiciones
        string controllerPath = $"{ANIMS_PATH}/KirikuAnimator.controller";
        CrearController(controllerPath, clips);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Kirikú listo",
            $"Creados {clips.Count} clips de animación.\n\n" +
            $"AnimatorController guardado en:\n{controllerPath}\n\n" +
            $"Asígnalo al componente Animator del GameObject de Kirikú en la escena.",
            "OK"
        );
    }

    // ── Carga los sub-sprites (frames cortados) de una textura ─────
    private static Sprite[] CargarSprites(string path)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        return all.OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    // ── Crea un AnimationClip con los frames dados ──────────────────
    private static AnimationClip CrearClip(string name, Sprite[] sprites, bool loop, int fps)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = fps;
        clip.name = name;

        // Binding al SpriteRenderer del mismo GameObject
        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        // Un keyframe por frame + uno extra al final para cerrar limpio
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

        // Configurar loop
        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);

        return clip;
    }

    // ── Crea el AnimatorController y arma la máquina de estados ─────
    private static void CrearController(string path, Dictionary<string, AnimationClip> clips)
    {
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);

        AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm = ac.layers[0].stateMachine;

        // Parámetros
        string[] triggers = { "Jab", "Hook", "Straight", "Hit", "Knockdown",
                               "Victory", "Defeat", "Telegraph", "Weave" };
        foreach (string t in triggers)
            ac.AddParameter(t, AnimatorControllerParameterType.Trigger);
        ac.AddParameter("IsBlocking", AnimatorControllerParameterType.Bool);

        // Estados
        var states = new Dictionary<string, AnimatorState>();

        // Idle — estado por defecto
        if (clips.TryGetValue("Idle", out AnimationClip idleClip))
        {
            AnimatorState idle = sm.AddState("Idle");
            idle.motion = idleClip;
            sm.defaultState = idle;
            states["Idle"] = idle;
        }

        // Resto de estados
        string[] nombres = { "Jab", "Hook", "Straight", "Block", "Hit",
                             "Knockdown", "Victory", "Defeat", "Telegraph", "Weave" };
        foreach (string n in nombres)
        {
            if (!clips.TryGetValue(n, out AnimationClip c)) continue;
            AnimatorState st = sm.AddState(n);
            st.motion = c;
            states[n] = st;
        }

        if (!states.ContainsKey("Idle")) { AssetDatabase.SaveAssets(); return; }

        AnimatorState idleState = states["Idle"];

        // Idle → estados con trigger (transición instantánea, sin exit time)
        foreach (string t in triggers)
        {
            if (!states.ContainsKey(t)) continue;
            var tr = idleState.AddTransition(states[t]);
            tr.AddCondition(AnimatorConditionMode.If, 0, t);
            tr.hasExitTime = false;
            tr.duration    = 0f;
        }

        // Idle ↔ Block (bool: entra si IsBlocking=true, sale si es false)
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

        // Estados de acción corta → vuelven a Idle al terminar
        string[] vuelvenAIdle = { "Jab", "Hook", "Straight", "Hit", "Telegraph", "Weave", "Knockdown" };
        foreach (string n in vuelvenAIdle)
        {
            if (!states.ContainsKey(n)) continue;
            var back = states[n].AddTransition(idleState);
            back.hasExitTime = true;
            back.exitTime    = 1f;
            back.duration    = 0f;
        }

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Kirikú] AnimatorController creado en: {path}");
    }
}
#endif
