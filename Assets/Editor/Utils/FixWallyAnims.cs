#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// Regenera los clips de Wally para que nunca quede con sprite null.
/// Cada clip recibe un keyframe en tiempo 0 Y otro al final con el mismo sprite.
/// Uso: Tools → 11 Fix Animaciones Wally
/// </summary>
public class FixWallyAnims : EditorWindow
{
    private const string SPRITES  = "Assets/Art/Personajes/Wally";
    private const string ANIMS    = "Assets/es/Wally";

    // (archivo de sprite, archivo .anim, fps, ¿loop?)
    private static readonly (string sprite, string anim, int fps, bool loop)[] TABLA = new[]
    {
        ("wally_idle",       "WallyIdle",      8,  true),
        ("wally_straight",   "WallyStraight",  12, false),
        ("wally_jab",        "WallyJab",       12, false),
        ("wally_hook",       "WallyHook",      12, false),
        ("wally_hit",        "WallyHit",       12, false),
        ("wally_block",      "WallyBlock",     8,  true),
        ("wally_knockdown",  "WallyKnockdown", 8,  false),
        ("wally_duck",       "WallyDuck",      8,  false),
        ("wally_weaveleft",  "WallyWeaveLeft", 8,  false),
        ("wally_weaveright", "WallyWeaveRight",8,  false),
        ("wally_backstep",   "WallyBackstep",  8,  false),
        ("wally_victory",    "WallyVictory",   8,  false),
        ("wally_defeat",     "WallyDefeat",    8,  false),
        ("wally_slip",       "WallySlip",      8,  false),
    };

    [MenuItem("Tools/11 Fix Animaciones Wally")]
    public static void Fix()
    {
        int arreglados = 0;

        foreach (var (spriteName, animName, fps, loop) in TABLA)
        {
            // Cargar sprite (único o primer sub-sprite del sheet)
            Sprite sprite = CargarSprite($"{SPRITES}/{spriteName}.png");
            if (sprite == null)
            {
                Debug.LogWarning($"[FixWally] Sprite no encontrado: {spriteName}.png");
                continue;
            }

            string animPath = $"{ANIMS}/{animName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (clip == null)
            {
                Debug.LogWarning($"[FixWally] Clip no encontrado: {animPath}");
                continue;
            }

            // Reconstruir el clip con keyframe al inicio Y al final
            EditorCurveBinding binding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };

            float duracion = (fps > 0) ? (1f / fps) * 2 : 0.5f; // al menos 2 frames de duración

            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0f,       value = sprite },
                new ObjectReferenceKeyframe { time = duracion, value = sprite }
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, s);

            EditorUtility.SetDirty(clip);
            arreglados++;
            Debug.Log($"[FixWally] {animName} → sprite={sprite.name}, loop={loop}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Fix Wally Anims",
            $"Animaciones corregidas: {arreglados}\n\nGuarda con Ctrl+S y prueba Play.", "OK");
    }

    private static Sprite CargarSprite(string path)
    {
        // Intentar primero como sprite único
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;

        // Si es spritesheet, tomar el primer sub-sprite
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        return all.OfType<Sprite>().OrderBy(x => x.name).FirstOrDefault();
    }
}
#endif
