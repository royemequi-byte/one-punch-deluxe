#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Conecta los ataques al KirikuProfile y verifica que la escena esté
/// lista para el combate. Complementa Tool 10.
/// Uso: Tools → 16 Fix Combate
/// </summary>
public class FixCombate : EditorWindow
{
    [MenuItem("Tools/16 Fix Combate")]
    public static void Fix()
    {
        int fixes = 0;

        // ── 1. Cargar AttackData ──────────────────────────────────
        AttackData jab      = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/JabData.asset");
        AttackData straight = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/StraightData.asset");
        AttackData hook     = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/HookData.asset");

        if (jab == null || straight == null || hook == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No se encontraron los AttackData.\nCorre primero Tools → 10 Conectar Escena.", "OK");
            return;
        }

        // ── 2. Asignar ataques al KirikuProfile ───────────────────
        AIProfile kirikuProfile = AssetDatabase.LoadAssetAtPath<AIProfile>(
            "Assets/ScriptableObjects/AIProfiles/KirikuProfile.asset");

        if (kirikuProfile != null)
        {
            bool vacio = kirikuProfile.attacks == null || kirikuProfile.attacks.Length == 0;
            if (vacio)
            {
                kirikuProfile.attacks = new AttackData[] { jab, straight, hook };
                EditorUtility.SetDirty(kirikuProfile);
                Debug.Log("[FixCombate] KirikuProfile: ataques asignados (Jab, Straight, Hook)");
                fixes++;
            }
        }
        else
        {
            Debug.LogWarning("[FixCombate] KirikuProfile.asset no encontrado.");
        }

        // ── 3. Asignar ataques al RivalComunProfile ───────────────
        AIProfile rivalProfile = AssetDatabase.LoadAssetAtPath<AIProfile>(
            "Assets/ScriptableObjects/AIProfiles/RivalComunProfile.asset");

        if (rivalProfile != null)
        {
            bool vacio = rivalProfile.attacks == null || rivalProfile.attacks.Length == 0;
            if (vacio)
            {
                rivalProfile.attacks = new AttackData[] { jab, straight };
                EditorUtility.SetDirty(rivalProfile);
                Debug.Log("[FixCombate] RivalComunProfile: ataques asignados");
                fixes++;
            }
        }

        // ── 4. Verificar RivalAI en escena tiene perfil asignado ──
        GameObject kiriku = GameObject.Find("Kiriku");
        if (kiriku != null)
        {
            RivalAI ai = kiriku.GetComponent<RivalAI>();
            if (ai != null)
            {
                SerializedObject so = new SerializedObject(ai);
                var profileProp = so.FindProperty("profile");
                if (profileProp != null && profileProp.objectReferenceValue == null)
                {
                    profileProp.objectReferenceValue = kirikuProfile;
                    so.ApplyModifiedProperties();
                    Debug.Log("[FixCombate] RivalAI.profile conectado a KirikuProfile");
                    fixes++;
                }
            }
        }

        // ── 5. Verificar CombatController de Kiriku tiene targetHealth ─
        if (kiriku != null)
        {
            CombatController combatK = kiriku.GetComponent<CombatController>();
            GameObject wally = GameObject.Find("Wally");
            if (combatK != null && wally != null)
            {
                SerializedObject so = new SerializedObject(combatK);
                var targetProp = so.FindProperty("targetHealth");
                if (targetProp != null && targetProp.objectReferenceValue == null)
                {
                    targetProp.objectReferenceValue = wally.GetComponent<Health>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[FixCombate] CombatController de Kirikú → targetHealth = Wally");
                    fixes++;
                }
            }
        }

        // ── 6. Corregir sprites de Victory/Defeat de Kirikú ──────
        FixKirikuAnimSprites();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Combate listo",
            $"Correcciones aplicadas: {fixes}\n\n" +
            "✓ KirikuProfile tiene ataques (Jab, Straight, Hook)\n" +
            "✓ RivalAI conectado al perfil\n" +
            "✓ CombatController de Kirikú apunta a la vida de Wally\n" +
            "✓ Animaciones Victory/Defeat de Kirikú corregidas\n\n" +
            "Controles:\n" +
            "  J = Jab    K = Recto    L = Gancho\n" +
            "  A = WeaveLeft    D = WeaveRight\n" +
            "  S = Duck    W = Backstep\n" +
            "  Espacio = Bloqueo\n\n" +
            "Guarda Ctrl+S → Play y prueba el combate.", "OK");
    }

    private static void FixKirikuAnimSprites()
    {
        const string KIRIKU_SPRITES = "Assets/Art/Personajes/Kiriku";
        const string KIRIKU_ANIMS   = "Assets/Animations/Kiriku";

        // Victory → usar kiriku_idle2 (postura de pie, mejor que hit2)
        FixAnimSprite(
            $"{KIRIKU_ANIMS}/Kiriku_Victory.anim",
            $"{KIRIKU_SPRITES}/kiriku_idle2.png",
            8, true);

        // Defeat → usar kiriku_knockdown (caída = derrota)
        FixAnimSprite(
            $"{KIRIKU_ANIMS}/Kiriku_Defeat.anim",
            $"{KIRIKU_SPRITES}/kiriku_knockdown.png",
            8, false);

        // Straight → kiriku_attack (ya correcto, reforzar)
        FixAnimSprite(
            $"{KIRIKU_ANIMS}/Kiriku_Straight.anim",
            $"{KIRIKU_SPRITES}/kiriku_attack.png",
            12, false);
    }

    private static void FixAnimSprite(string animPath, string spritePath, int fps, bool loop)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
        if (clip == null) return;

        var sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (sprites.Length == 0) { Debug.LogWarning($"[FixCombate] Sin sprites en {spritePath}"); return; }

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
        keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length * frameDur, value = sprites[sprites.Length - 1] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings cfg = AnimationUtility.GetAnimationClipSettings(clip);
        cfg.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, cfg);
        EditorUtility.SetDirty(clip);

        Debug.Log($"[FixCombate] Anim corregida: {System.IO.Path.GetFileName(animPath)} -> {System.IO.Path.GetFileName(spritePath)}");
    }
}

#endif
