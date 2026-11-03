#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Restaura el sprite por defecto de Wally tras el reimport de los sheets.
/// Uso: Tools → 14 Fix Wally Sprite
/// </summary>
public class FixWallySprite : EditorWindow
{
    private const string SPRITES = "Assets/Art/Personajes/Wally";
    private const string ANIMS   = "Assets/es/Wally";

    private static readonly (string sprite, string anim, int fps, bool loop)[] TABLA = new[]
    {
        ("wally_idle",       "WallyIdle",       8,  true ),
        ("wally_straight",   "WallyStraight",   12, false),
        ("wally_jab",        "WallyJab",        12, false),
        ("wally_hook",       "WallyHook",       12, false),
        ("wally_hit",        "WallyHit",        12, false),
        ("wally_knockdown",  "WallyKnockdown",  8,  false),
        ("wally_slip",       "WallySlip",       8,  false),
        ("wally_block",      "WallyBlock",      8,  true ),
        ("wally_duck",       "WallyDuck",       8,  false),
        ("wally_backstep",   "WallyBackstep",   8,  false),
        ("wally_weaveleft",  "WallyWeaveLeft",  8,  false),
        ("wally_weaveright", "WallyWeaveRight", 8,  false),
        ("wally_victory",    "WallyVictory",    8,  false),
        ("wally_defeat",     "WallyDefeat",     8,  false),
    };

    [MenuItem("Tools/14 Fix Wally Sprite")]
    public static void Fix()
    {
        // ── 1. Reasignar sprite por defecto de Wally ──────────────────
        GameObject wally = GameObject.Find("Wally");
        if (wally == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró 'Wally' en la escena.", "OK");
            return;
        }

        SpriteRenderer sr = wally.GetComponent<SpriteRenderer>();
        if (sr == null) sr = wally.AddComponent<SpriteRenderer>();

        // Cargar el primer sub-sprite de wally_idle
        string idlePath = $"{SPRITES}/wally_idle.png";
        Sprite idleSprite = PrimerSubSprite(idlePath);
        if (idleSprite != null)
        {
            sr.sprite       = idleSprite;
            sr.sortingOrder = 2;
            Debug.Log($"[FixWallySprite] Sprite por defecto: {idleSprite.name}");
        }
        else
        {
            Debug.LogWarning("[FixWallySprite] No se encontró sub-sprite en wally_idle.png");
        }

        // ── 2. Regenerar todos los clips con los sub-sprites nuevos ───
        int arreglados = 0;
        foreach (var (spriteName, animName, fps, loop) in TABLA)
        {
            string spritePath = $"{SPRITES}/{spriteName}.png";
            string animPath   = $"{ANIMS}/{animName}.anim";

            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
                                            .OfType<Sprite>()
                                            .OrderBy(s => s.name)
                                            .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[FixWallySprite] Sin sprites: {spritePath}");
                continue;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (clip == null) { Debug.LogWarning($"[FixWallySprite] Clip no encontrado: {animPath}"); continue; }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };

            float frameDur = 1f / fps;
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * frameDur, value = sprites[i] };
            keys[sprites.Length] = new ObjectReferenceKeyframe
            {
                time  = sprites.Length * frameDur,
                value = sprites[sprites.Length - 1]
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings cfg = AnimationUtility.GetAnimationClipSettings(clip);
            cfg.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, cfg);

            EditorUtility.SetDirty(clip);
            arreglados++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Wally restaurado",
            $"Sprite por defecto asignado: {(idleSprite != null ? idleSprite.name : "ERROR")}\n" +
            $"Clips regenerados: {arreglados}/14\n\n" +
            "Guarda con Ctrl+S y dale Play.", "OK");
    }

    private static Sprite PrimerSubSprite(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
                            .OfType<Sprite>()
                            .OrderBy(s => s.name)
                            .FirstOrDefault();
    }
}
#endif
