#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Importa los nuevos sprite sheets de Wally (5 frames c/u) y
/// regenera cada AnimationClip con keyframes reales por frame.
/// Uso: Tools → 13 Slicear Wally Nuevo
/// </summary>
public class SlicearWallyNuevo : EditorWindow
{
    private const string SPRITES = "Assets/Art/Personajes/Wally";
    private const string ANIMS   = "Assets/es/Wally";
    private const int    FPS_ATAQUE  = 12;
    private const int    FPS_DEFENSA = 8;
    private const int    PPU         = 100;

    // (sprite, anim, fps, loop)
    private static readonly (string s, string a, int fps, bool loop)[] TABLA = new[]
    {
        ("wally_idle",       "WallyIdle",       FPS_DEFENSA, true ),
        ("wally_straight",   "WallyStraight",   FPS_ATAQUE,  false),
        ("wally_jab",        "WallyJab",        FPS_ATAQUE,  false),
        ("wally_hook",       "WallyHook",       FPS_ATAQUE,  false),
        ("wally_hit",        "WallyHit",        FPS_ATAQUE,  false),
        ("wally_knockdown",  "WallyKnockdown",  FPS_DEFENSA, false),
        ("wally_slip",       "WallySlip",       FPS_DEFENSA, false),
        ("wally_block",      "WallyBlock",      FPS_DEFENSA, true ),
        ("wally_duck",       "WallyDuck",       FPS_DEFENSA, false),
        ("wally_backstep",   "WallyBackstep",   FPS_DEFENSA, false),
        ("wally_weaveleft",  "WallyWeaveLeft",  FPS_DEFENSA, false),
        ("wally_weaveright", "WallyWeaveRight", FPS_DEFENSA, false),
        ("wally_victory",    "WallyVictory",    FPS_DEFENSA, false),
        ("wally_defeat",     "WallyDefeat",     FPS_DEFENSA, false),
    };

    [MenuItem("Tools/13 Slicear Wally Nuevo")]
    public static void Ejecutar()
    {
        // ── 1. Configurar todos los sprite sheets como Multiple 5 frames ──
        int importados = 0;
        foreach (var (spriteName, _, _, _) in TABLA)
        {
            string path = $"{SPRITES}/{spriteName}.png";
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[SlicearWally] No encontrado: {path}"); continue; }

            // Forzar reimport como spritesheet Multiple
            ti.textureType        = TextureImporterType.Sprite;
            ti.spriteImportMode   = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = PPU;
            ti.filterMode         = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;

            // Obtener dimensiones reales de la textura
            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);

            // Leer el tamaño sin importar (via preview)
            int w = 0, h = 0;
            GetTextureSize(path, out w, out h);
            if (w == 0) { Debug.LogWarning($"[SlicearWally] No se pudo leer tamaño: {path}"); continue; }

            int frameW = w / 5;

            // Crear 5 SpriteMetaData
            var metas = new SpriteMetaData[5];
            for (int i = 0; i < 5; i++)
            {
                metas[i] = new SpriteMetaData
                {
                    name   = $"{spriteName}_{i}",
                    rect   = new Rect(i * frameW, 0, frameW, h),
                    pivot  = new Vector2(0.5f, 0f),   // pivot abajo-centro
                    alignment = (int)SpriteAlignment.BottomCenter
                };
            }
            ti.spritesheet = metas;
            ti.SaveAndReimport();
            importados++;
        }

        AssetDatabase.Refresh();

        // ── 2. Regenerar AnimationClips con 5 keyframes reales ────────────
        int animados = 0;
        foreach (var (spriteName, animName, fps, loop) in TABLA)
        {
            string spritePath = $"{SPRITES}/{spriteName}.png";
            string animPath   = $"{ANIMS}/{animName}.anim";

            // Cargar todos los sub-sprites del sheet
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            var sprites   = allAssets.OfType<Sprite>()
                                     .OrderBy(sp => sp.name)
                                     .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[SlicearWally] Sin sprites en {spritePath}");
                continue;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (clip == null)
            {
                Debug.LogWarning($"[SlicearWally] Clip no encontrado: {animPath}");
                continue;
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };

            int frameCount = sprites.Length;
            float frameDur = 1f / fps;

            // Un keyframe por sprite + keyframe final repitiendo el último
            var keys = new ObjectReferenceKeyframe[frameCount + 1];
            for (int i = 0; i < frameCount; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * frameDur, value = sprites[i] };
            keys[frameCount] = new ObjectReferenceKeyframe
            {
                time  = frameCount * frameDur,
                value = sprites[frameCount - 1]
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, s);

            EditorUtility.SetDirty(clip);
            animados++;
            Debug.Log($"[SlicearWally] {animName}: {frameCount} frames @ {fps}fps, loop={loop}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Wally Actualizado",
            $"Sprites reimportados: {importados}/14\n" +
            $"Animaciones actualizadas: {animados}/14\n\n" +
            "Wally ahora tiene animaciones reales de 5 frames.\n" +
            "Guarda con Ctrl+S y dale Play para probar.", "OK");
    }

    // Lee el tamaño de una textura usando su metadata en el disco
    private static void GetTextureSize(string assetPath, out int w, out int h)
    {
        w = 0; h = 0;
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex == null) return;
        w = tex.width;
        h = tex.height;
    }
}
#endif
