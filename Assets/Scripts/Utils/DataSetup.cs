// Crea los ScriptableObjects de datos que faltan: FighterData y AIProfile de Kirikú.
// Uso: Tools → 4 Crear Datos del Juego
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DataSetup : EditorWindow
{
    private const string FIGHTERS_PATH  = "Assets/ScriptableObjects/Fighters";
    private const string AIPROFILES_PATH = "Assets/ScriptableObjects/AIProfiles";

    [MenuItem("Tools/4 Crear Datos del Juego")]
    public static void CrearDatos()
    {
        int creados = 0;

        // ── FighterData: Wally ───────────────────────────────────────
        string wallyPath = $"{FIGHTERS_PATH}/WallyData.asset";
        if (AssetDatabase.LoadAssetAtPath<FighterData>(wallyPath) == null)
        {
            FighterData wally = ScriptableObject.CreateInstance<FighterData>();
            wally.fighterName = "Wally Dempsey";
            wally.maxHealth   = 100;
            wally.maxStamina  = 100;
            wally.attackPower = 1f;
            wally.defense     = 0.25f;
            AssetDatabase.CreateAsset(wally, wallyPath);
            Debug.Log("[Datos] WallyData creado");
            creados++;
        }

        // ── FighterData: Rival Común ─────────────────────────────────
        string rivalPath = $"{FIGHTERS_PATH}/RivalComunData.asset";
        if (AssetDatabase.LoadAssetAtPath<FighterData>(rivalPath) == null)
        {
            FighterData rival = ScriptableObject.CreateInstance<FighterData>();
            rival.fighterName = "Rival Común";
            rival.maxHealth   = 80;
            rival.maxStamina  = 100;
            rival.attackPower = 0.8f;
            rival.defense     = 0.2f;
            AssetDatabase.CreateAsset(rival, rivalPath);
            Debug.Log("[Datos] RivalComunData creado");
            creados++;
        }

        // ── FighterData: Kirikú ──────────────────────────────────────
        string kirikuFighterPath = $"{FIGHTERS_PATH}/KirikuData.asset";
        if (AssetDatabase.LoadAssetAtPath<FighterData>(kirikuFighterPath) == null)
        {
            FighterData kiriku = ScriptableObject.CreateInstance<FighterData>();
            kiriku.fighterName = "Kirikú";
            kiriku.maxHealth   = 120;
            kiriku.maxStamina  = 100;
            kiriku.attackPower = 1.5f;
            kiriku.defense     = 0.4f;
            AssetDatabase.CreateAsset(kiriku, kirikuFighterPath);
            Debug.Log("[Datos] KirikuData (FighterData) creado");
            creados++;
        }

        // ── AIProfile: Kirikú ────────────────────────────────────────
        // (RivalComunProfile ya existe; solo crear el de Kirikú)
        string kirikuAIPath = $"{AIPROFILES_PATH}/KirikuProfile.asset";
        if (AssetDatabase.LoadAssetAtPath<AIProfile>(kirikuAIPath) == null)
        {
            AIProfile kiriku = ScriptableObject.CreateInstance<AIProfile>();
            kiriku.rivalName         = "Kirikú";
            kiriku.telegraphDuration = 0.25f;  // jefe final: muy rápido
            kiriku.minCooldown       = 0.4f;
            kiriku.maxCooldown       = 1.2f;

            // Asignar los 3 ataques disponibles (ya deben existir en Attacks/)
            AttackData jab      = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/JabData.asset");
            AttackData straight = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/StraightData.asset");
            AttackData hook     = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/HookData.asset");

            var lista = new System.Collections.Generic.List<AttackData>();
            if (jab      != null) lista.Add(jab);
            if (straight != null) lista.Add(straight);
            if (hook     != null) lista.Add(hook);
            kiriku.attacks = lista.ToArray();

            AssetDatabase.CreateAsset(kiriku, kirikuAIPath);
            Debug.Log($"[Datos] KirikuProfile creado con {lista.Count} ataques");
            creados++;
        }

        // ── Verificar que RivalComunProfile tenga ataques asignados ──
        AIProfile rivalProfile = AssetDatabase.LoadAssetAtPath<AIProfile>(
            $"{AIPROFILES_PATH}/RivalComunProfile.asset"
        );
        if (rivalProfile != null && (rivalProfile.attacks == null || rivalProfile.attacks.Length == 0))
        {
            AttackData jab      = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/JabData.asset");
            AttackData straight = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/StraightData.asset");
            AttackData hook     = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/ScriptableObjects/Attacks/HookData.asset");

            var lista = new System.Collections.Generic.List<AttackData>();
            if (jab      != null) lista.Add(jab);
            if (straight != null) lista.Add(straight);
            if (hook     != null) lista.Add(hook);
            rivalProfile.attacks = lista.ToArray();

            EditorUtility.SetDirty(rivalProfile);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Datos] RivalComunProfile actualizado con {lista.Count} ataques");
            creados++;
        }

        AssetDatabase.Refresh();

        string mensaje = creados > 0
            ? $"Creados/actualizados {creados} assets en ScriptableObjects/.\n\n" +
              "PENDIENTE: Asigna el AnimatorController a cada FighterData en el Inspector."
            : "Todo ya existía — no se creó nada nuevo.";

        EditorUtility.DisplayDialog("Datos del juego", mensaje, "OK");
    }
}
#endif
