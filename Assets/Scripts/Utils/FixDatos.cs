#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Actualiza los ScriptableObjects de AttackData y AIProfile con valores
/// sincronizados a las animaciones y balanceados para que el juego se sienta bien.
/// Uso: Tools → 18 Fix Datos
/// </summary>
public class FixDatos : EditorWindow
{
    [MenuItem("Tools/18 Fix Datos")]
    public static void Fix()
    {
        int cambios = 0;

        // ── AttackData ────────────────────────────────────────────────────
        // Los tiempos (windup + active + recovery) coinciden con la duración
        // real de cada animación para que el sprite y el daño estén sincronizados.

        // Jab: 5 frames a 15fps = 0.333s total
        cambios += ActualizarAtaque("JabData",
            nombre:      "Jab",
            damage:      12,
            stamina:     12f,
            windup:      0.08f,   // frames 0-1
            active:      0.10f,   // frame 2 (el brazo extendido)
            recovery:    0.15f,   // frames 3-4 retrocede
            trigger:     "Jab",
            esquives:    new[] { DodgePosition.WeaveRight });

        // Recto: 6 frames a 12fps = 0.5s total
        cambios += ActualizarAtaque("StraightData",
            nombre:      "Recto",
            damage:      18,
            stamina:     18f,
            windup:      0.15f,
            active:      0.15f,
            recovery:    0.20f,
            trigger:     "Straight",
            esquives:    new[] { DodgePosition.Backstep, DodgePosition.WeaveLeft, DodgePosition.WeaveRight });

        // Gancho: 6 frames a 10fps = 0.6s total
        cambios += ActualizarAtaque("HookData",
            nombre:      "Gancho",
            damage:      22,
            stamina:     22f,
            windup:      0.20f,
            active:      0.15f,
            recovery:    0.25f,
            trigger:     "Hook",
            esquives:    new[] { DodgePosition.Duck, DodgePosition.WeaveLeft });

        // Cargar los ataques una vez para asignarlos a los perfiles
        AttackData jabAsset = AssetDatabase.LoadAssetAtPath<AttackData>(
            "Assets/ScriptableObjects/Attacks/JabData.asset");
        AttackData straightAsset = AssetDatabase.LoadAssetAtPath<AttackData>(
            "Assets/ScriptableObjects/Attacks/StraightData.asset");
        AttackData hookAsset = AssetDatabase.LoadAssetAtPath<AttackData>(
            "Assets/ScriptableObjects/Attacks/HookData.asset");

        // Armar el array solo con los que existen
        var ataquesDisponibles = new System.Collections.Generic.List<AttackData>();
        if (jabAsset      != null) ataquesDisponibles.Add(jabAsset);
        if (straightAsset != null) ataquesDisponibles.Add(straightAsset);
        if (hookAsset     != null) ataquesDisponibles.Add(hookAsset);

        // ── AIProfile Kirikú ──────────────────────────────────────────────
        AIProfile kiriku = AssetDatabase.LoadAssetAtPath<AIProfile>(
            "Assets/ScriptableObjects/AIProfiles/KirikuProfile.asset");
        if (kiriku != null)
        {
            kiriku.rivalName         = "Kirikú";
            kiriku.telegraphDuration = 0.5f;   // visible pero exigente
            kiriku.minCooldown       = 1.2f;
            kiriku.maxCooldown       = 2.8f;
            // Sin ataques asignados la IA no hace nada — asignar los 3
            if (ataquesDisponibles.Count > 0)
                kiriku.attacks = ataquesDisponibles.ToArray();
            EditorUtility.SetDirty(kiriku);
            cambios++;
            Debug.Log($"[FixDatos] KirikuProfile actualizado ({kiriku.attacks?.Length ?? 0} ataques)");
        }

        // ── AIProfile Rival Común ─────────────────────────────────────────
        AIProfile rival = AssetDatabase.LoadAssetAtPath<AIProfile>(
            "Assets/ScriptableObjects/AIProfiles/RivalComunProfile.asset");
        if (rival != null)
        {
            rival.rivalName          = "Rival";
            rival.telegraphDuration  = 0.8f;
            rival.minCooldown        = 2.0f;
            rival.maxCooldown        = 4.0f;
            if (ataquesDisponibles.Count > 0)
                rival.attacks = ataquesDisponibles.ToArray();
            EditorUtility.SetDirty(rival);
            cambios++;
            Debug.Log("[FixDatos] RivalComunProfile actualizado");
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Datos actualizados",
            $"Cambios aplicados: {cambios}\n\n" +
            "Ataques:\n" +
            "  Jab    → 12 daño, 0.33s  (evita: WeaveRight)\n" +
            "  Recto  → 18 daño, 0.50s  (evita: Backstep/Weave)\n" +
            "  Gancho → 22 daño, 0.60s  (evita: Duck/WeaveLeft)\n\n" +
            "IA Kirikú:\n" +
            "  Telegraph 0.5s  |  Ataca cada 1.2–2.8s\n" +
            "  Ataques asignados al perfil: Jab + Recto + Gancho\n\n" +
            "Ctrl+S y dale Play.", "OK");
    }

    private static int ActualizarAtaque(string asset, string nombre,
        int damage, float stamina,
        float windup, float active, float recovery,
        string trigger, DodgePosition[] esquives)
    {
        string path = $"Assets/ScriptableObjects/Attacks/{asset}.asset";
        AttackData data = AssetDatabase.LoadAssetAtPath<AttackData>(path);
        if (data == null)
        {
            Debug.LogWarning($"[FixDatos] No encontrado: {path}");
            return 0;
        }

        data.attackName       = nombre;
        data.damage           = damage;
        data.staminaCost      = stamina;
        data.windupTime       = windup;
        data.activeTime       = active;
        data.recoveryTime     = recovery;
        data.animationTrigger = trigger;
        data.dodgesThatAvoid  = esquives;
        EditorUtility.SetDirty(data);
        Debug.Log($"[FixDatos] {asset} actualizado");
        return 1;
    }
}
#endif
