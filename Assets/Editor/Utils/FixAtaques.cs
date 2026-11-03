#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Diferencia visualmente los 3 ataques de Wally aunque vengan del mismo sprite sheet.
/// Jab  = 3 frames primeros, 15fps  → golpe rápido y corto
/// Recto = 5 frames completos, 12fps → extensión total
/// Gancho = 5 frames en reversa, 10fps → swing de vuelta
/// Uso: Tools → 17 Fix Ataques
/// </summary>
public class FixAtaques : EditorWindow
{
    private const string SPRITES_WALLY  = "Assets/Art/Personajes/Wally";
    private const string SPRITES_KIRIKU = "Assets/Art/Personajes/Kiriku";
    private const string ANIMS_WALLY    = "Assets/es/Wally";
    private const string ANIMS_KIRIKU   = "Assets/Animations/Kiriku";

    [MenuItem("Tools/17 Fix Ataques")]
    public static void Fix()
    {
        int ok = 0;

        // ── IDLE ESTÁTICO (1 solo frame, personajes completamente quietos) ──
        ok += AplicarIdleEstatico(
            $"{ANIMS_WALLY}/WallyIdle.anim",
            $"{SPRITES_WALLY}/wally_idle.png",
            frameIndex: 0);

        ok += AplicarIdleEstatico(
            $"{ANIMS_KIRIKU}/Kiriku_Idle.anim",
            $"{SPRITES_KIRIKU}/kiriku_idle.png",
            frameIndex: 0);

        // ── ATAQUES KIRIKÚ — cada golpe tiene su propio sprite ────────────
        // kiriku_jab: golpe corto y rápido
        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Jab.anim",
            $"{SPRITES_KIRIKU}/kiriku_jab.png",    new[] {0,1,2,1,0},    15, loop: false);

        // kiriku_attack → recto/straight
        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Straight.anim",
            $"{SPRITES_KIRIKU}/kiriku_attack.png", new[] {0,1,2,2},      12, loop: false);

        // kiriku_hook → gancho
        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Hook.anim",
            $"{SPRITES_KIRIKU}/kiriku_hook.png",   new[] {0,1,2,3,4,4},  10, loop: false);

        // ── REACCIONES KIRIKÚ ─────────────────────────────────────────────
        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Hit.anim",
            $"{SPRITES_KIRIKU}/kiriku_hit.png",    new[] {0,1,2,3,4,4},  14, loop: false);

        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Block.anim",
            $"{SPRITES_KIRIKU}/kiriku_block.png",  new[] {0,1,2,1},       8, loop: true);

        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Knockdown.anim",
            $"{SPRITES_KIRIKU}/kiriku_knockdown.png", new[] {0,1,2,3,4,4}, 10, loop: false);

        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Weave.anim",
            $"{SPRITES_KIRIKU}/kiriku_weave.png",  new[] {0,1,2,3,4},    12, loop: false);

        // ── TELEGRAPH: sprite dedicado para cuando Kirikú se prepara ─────
        // La IA lo activa justo antes de golpear (se ve su pose de ataque)
        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Telegraph.anim",
            $"{SPRITES_KIRIKU}/kiriku_telegraph.png", new[] {0,1,2,2},    8, loop: true);

        // ── VICTORIA / DERROTA KIRIKÚ ─────────────────────────────────────
        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Victory.anim",
            $"{SPRITES_KIRIKU}/kiriku_idle2.png",  new[] {0,1,2,3,4,3,2,1}, 8, loop: true);

        ok += AplicarAtaque($"{ANIMS_KIRIKU}/Kiriku_Defeat.anim",
            $"{SPRITES_KIRIKU}/kiriku_knockdown.png", new[] {0,1,2,3,4,4},  8, loop: false);

        // ── ATAQUES WALLY ─────────────────────────────────────────────────
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyJab.anim",
            $"{SPRITES_WALLY}/wally_jab.png",      new[] {0,1,2,1,0},    15, loop: false);

        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyStraight.anim",
            $"{SPRITES_WALLY}/wally_straight.png", new[] {0,1,2,3,4,4},  12, loop: false);

        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyHook.anim",
            $"{SPRITES_WALLY}/wally_hook.png",     new[] {4,3,2,1,0,0},  10, loop: false);

        // ── ESQUIVES más rápidos (12fps en vez de 8) ─────────────────────
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyWeaveLeft.anim",
            $"{SPRITES_WALLY}/wally_weaveleft.png",  new[] {0,1,2,3,4},  12, loop: false);
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyWeaveRight.anim",
            $"{SPRITES_WALLY}/wally_weaveright.png", new[] {0,1,2,3,4},  12, loop: false);
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyDuck.anim",
            $"{SPRITES_WALLY}/wally_duck.png",       new[] {0,1,2,3,4},  12, loop: false);
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyBackstep.anim",
            $"{SPRITES_WALLY}/wally_backstep.png",   new[] {0,1,2,1,0},  12, loop: false);

        // ── REACCIÓN a golpe: empieza desde el frame 0 ───────────────────
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyHit.anim",
            $"{SPRITES_WALLY}/wally_hit.png",        new[] {0,1,2,3,4,4},  14, loop: false);

        // ── VICTORIA: animación en bucle celebrando ───────────────────────
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyVictory.anim",
            $"{SPRITES_WALLY}/wally_victory.png",    new[] {0,1,2,3,4,3,2,1}, 8, loop: true);

        // ── DERROTA y KNOCKDOWN ───────────────────────────────────────────
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyKnockdown.anim",
            $"{SPRITES_WALLY}/wally_knockdown.png",  new[] {0,1,2,3,4,4}, 10, loop: false);
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyDefeat.anim",
            $"{SPRITES_WALLY}/wally_defeat.png",     new[] {0,1,2,3,4,4}, 8, loop: false);

        // ── BLOQUEO en loop mientras se mantiene Espacio ─────────────────
        ok += AplicarAtaque($"{ANIMS_WALLY}/WallyBlock.anim",
            $"{SPRITES_WALLY}/wally_block.png",      new[] {0,1,2,3,4},   8, loop: true);

        // ── LIMPIAR Rigidbody2D si existe ──────────────────────────────────
        int rb = EliminarRigidbody("Wally") + EliminarRigidbody("Kiriku");

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Animaciones corregidas",
            $"Clips actualizados: {ok}/27\n" +
            $"Rigidbody2D eliminados: {rb}\n\n" +
            "WALLY:\n" +
            "  Idle    → frame fijo\n" +
            "  Jab/Recto/Gancho → sprites propios\n" +
            "  Esquives, Hit, Bloqueo, Victoria, KO\n\n" +
            "KIRIKÚ:\n" +
            "  Idle    → frame fijo\n" +
            "  Jab/Recto/Gancho → cada uno su sprite\n" +
            "  Hit, Bloqueo, Weave, Knockdown\n" +
            "  Telegraph → pose de ataque dedicada\n" +
            "  Victoria, Derrota\n\n" +
            "Guarda Ctrl+S y dale Play.", "OK");
    }

    // Un único keyframe = sprite completamente quieto
    private static int AplicarIdleEstatico(string clipPath, string spritePath, int frameIndex)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) { Debug.LogWarning($"[FixAtaques] No encontrado: {clipPath}"); return 0; }

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length == 0) { Debug.LogWarning($"[FixAtaques] Sin sprites: {spritePath}"); return 0; }

        int idx = Mathf.Clamp(frameIndex, 0, sprites.Length - 1);
        EditorCurveBinding binding = new EditorCurveBinding
            { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };

        // Dos keyframes iguales = imagen fija, sin transición visual
        var keys = new ObjectReferenceKeyframe[]
        {
            new ObjectReferenceKeyframe { time = 0f,    value = sprites[idx] },
            new ObjectReferenceKeyframe { time = 0.25f, value = sprites[idx] },
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings cfg = AnimationUtility.GetAnimationClipSettings(clip);
        cfg.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, cfg);
        EditorUtility.SetDirty(clip);

        Debug.Log($"[FixAtaques] Idle estático: {System.IO.Path.GetFileName(clipPath)} → {sprites[idx].name}");
        return 1;
    }

    private static int EliminarRigidbody(string nombreGO)
    {
        GameObject go = GameObject.Find(nombreGO);
        if (go == null) return 0;

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) return 0;

        Object.DestroyImmediate(rb);
        Debug.Log($"[FixAtaques] Rigidbody2D eliminado de {nombreGO}");
        return 1;
    }

    private static int AplicarAtaque(string clipPath, string spritePath, int[] frameOrder, int fps, bool loop = false)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) { Debug.LogWarning($"[FixAtaques] Clip no encontrado: {clipPath}"); return 0; }

        Sprite[] subSprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (subSprites.Length == 0)
        {
            Debug.LogWarning($"[FixAtaques] Sin sub-sprites en {spritePath}");
            return 0;
        }

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        float dur = 1f / fps;
        var keys = new ObjectReferenceKeyframe[frameOrder.Length];
        for (int i = 0; i < frameOrder.Length; i++)
        {
            int idx = Mathf.Clamp(frameOrder[i], 0, subSprites.Length - 1);
            keys[i] = new ObjectReferenceKeyframe { time = i * dur, value = subSprites[idx] };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings cfg = AnimationUtility.GetAnimationClipSettings(clip);
        cfg.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, cfg);

        EditorUtility.SetDirty(clip);
        Debug.Log($"[FixAtaques] {System.IO.Path.GetFileName(clipPath)}: {frameOrder.Length} frames a {fps}fps, loop={loop}");
        return 1;
    }
}
#endif
