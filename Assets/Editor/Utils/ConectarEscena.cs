#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Conecta toda la escena de combate: agrega componentes a Wally,
/// crea AttackData si no existen, y cableña todas las referencias del FightManager.
/// Uso: Tools → 10 Conectar Escena
/// </summary>
public class ConectarEscena : EditorWindow
{
    private const string ATTACKS_PATH = "Assets/ScriptableObjects/Attacks";

    [MenuItem("Tools/10 Conectar Escena")]
    public static void Conectar()
    {
        // ── Buscar objetos en la escena ──────────────────────────────
        GameObject wallyGO   = GameObject.Find("Wally");
        GameObject kirikuGO  = GameObject.Find("Kiriku");
        GameObject fightMgrGO = GameObject.Find("FightManager");
        GameObject hudGO     = GameObject.Find("HUD");

        if (wallyGO == null || kirikuGO == null || fightMgrGO == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No se encontraron Wally, Kiriku o FightManager en la escena.\n" +
                "Asegúrate de tener abierta la escena 'dia 1' y corre primero el Tool 7.", "OK");
            return;
        }

        // ── Paso 1: Agregar componentes a Wally ─────────────────────
        PrepararWally(wallyGO, kirikuGO);

        // ── Paso 2: Crear AttackData si no existen ───────────────────
        EnsureAttacksFolder();
        AttackData jabData      = CrearOCargar("JabData",      "Jab",      15, 15f,  0.10f, 0.08f, 0.12f, "Jab",      new[]{ DodgePosition.WeaveRight });
        AttackData straightData = CrearOCargar("StraightData", "Recto",    20, 20f,  0.15f, 0.10f, 0.20f, "Straight", new[]{ DodgePosition.Backstep, DodgePosition.WeaveLeft, DodgePosition.WeaveRight });
        AttackData hookData     = CrearOCargar("HookData",     "Gancho",   18, 18f,  0.12f, 0.10f, 0.18f, "Hook",     new[]{ DodgePosition.Duck, DodgePosition.WeaveLeft });

        // ── Paso 3: Conectar componentes del rival (Kiriku) ──────────
        ConectarKiriku(kirikuGO, wallyGO);

        // ── Paso 4: Conectar PlayerInputHandler de Wally ────────────
        ConectarPlayerInput(wallyGO, kirikuGO, jabData, straightData, hookData);

        // ── Paso 5: Conectar FightManager ────────────────────────────
        ConectarFightManager(fightMgrGO, wallyGO, kirikuGO, hudGO);

        // ── Paso 6: Conectar CombatController de Wally ───────────────
        ConectarCombatControllerWally(wallyGO, kirikuGO);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("¡Escena conectada!",
            "Todo conectado:\n" +
            "✓ Wally tiene todos sus componentes\n" +
            "✓ AttackData creados (Jab, Recto, Gancho)\n" +
            "✓ FightManager cableado\n" +
            "✓ Kiriku y Wally listos\n\n" +
            "Guarda con Ctrl+S y dale PLAY para probar.", "OK");
    }

    // ── WALLY: agregar lo que falta ──────────────────────────────────

    private static void PrepararWally(GameObject wally, GameObject kiriku)
    {
        // Animator con WallyAnimator.controller
        Animator anim = wally.GetComponent<Animator>();
        if (anim == null) anim = wally.AddComponent<Animator>();
        if (anim.runtimeAnimatorController == null)
        {
            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/es/Wally/Characters/Wally/WallyAnimator.controller");
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;
        }

        // Health
        if (wally.GetComponent<Health>() == null) wally.AddComponent<Health>();

        // Stamina
        if (wally.GetComponent<Stamina>() == null) wally.AddComponent<Stamina>();

        // CombatController
        if (wally.GetComponent<CombatController>() == null) wally.AddComponent<CombatController>();

        // PlayerInputHandler
        if (wally.GetComponent<PlayerInputHandler>() == null) wally.AddComponent<PlayerInputHandler>();

        Debug.Log("[Conectar] Wally preparado con todos sus componentes");
    }

    // ── KIRIKU: conectar CombatController y RivalAI ──────────────────

    private static void ConectarKiriku(GameObject kiriku, GameObject wally)
    {
        Stamina staminaKiriku   = kiriku.GetComponent<Stamina>();
        Animator animKiriku     = kiriku.GetComponent<Animator>();
        Health healthWally      = wally.GetComponent<Health>();
        CombatController combatK = kiriku.GetComponent<CombatController>();

        if (combatK != null)
        {
            SerializedObject so = new SerializedObject(combatK);
            so.FindProperty("stamina").objectReferenceValue  = staminaKiriku;
            so.FindProperty("animator").objectReferenceValue = animKiriku;
            so.FindProperty("targetHealth").objectReferenceValue = healthWally;
            so.ApplyModifiedProperties();
        }

        RivalAI ai = kiriku.GetComponent<RivalAI>();
        if (ai != null)
        {
            AIProfile profile = AssetDatabase.LoadAssetAtPath<AIProfile>(
                "Assets/ScriptableObjects/AIProfiles/KirikuProfile.asset");

            // TelegraphIndicator puede estar en raíz o como hijo (FixKiriku lo pone en raíz)
            Transform telegraph = kiriku.transform.Find("TelegraphIndicator");
            GameObject telegraphGO = telegraph != null
                ? telegraph.gameObject
                : GameObject.Find("TelegraphIndicator");

            SerializedObject so = new SerializedObject(ai);
            so.FindProperty("combatController").objectReferenceValue  = combatK;
            so.FindProperty("kirikuAnimator").objectReferenceValue    = animKiriku;
            if (profile != null)
                so.FindProperty("profile").objectReferenceValue = profile;
            if (telegraphGO != null)
                so.FindProperty("telegraphIndicator").objectReferenceValue = telegraphGO;
            so.ApplyModifiedProperties();
        }

        Debug.Log("[Conectar] Kiriku conectado");
    }

    // ── WALLY: conectar CombatController ─────────────────────────────

    private static void ConectarCombatControllerWally(GameObject wally, GameObject kiriku)
    {
        CombatController combatW = wally.GetComponent<CombatController>();
        if (combatW == null) return;

        Stamina staminaW = wally.GetComponent<Stamina>();
        Animator animW   = wally.GetComponent<Animator>();
        Health healthK   = kiriku.GetComponent<Health>();

        SerializedObject so = new SerializedObject(combatW);
        so.FindProperty("stamina").objectReferenceValue      = staminaW;
        so.FindProperty("animator").objectReferenceValue     = animW;
        so.FindProperty("targetHealth").objectReferenceValue = healthK;
        so.ApplyModifiedProperties();

        Debug.Log("[Conectar] CombatController de Wally conectado");
    }

    // ── PLAYER INPUT HANDLER ─────────────────────────────────────────

    private static void ConectarPlayerInput(GameObject wally, GameObject kiriku,
        AttackData jab, AttackData straight, AttackData hook)
    {
        PlayerInputHandler input = wally.GetComponent<PlayerInputHandler>();
        if (input == null) return;

        CombatController combatW = wally.GetComponent<CombatController>();
        CombatController combatK = kiriku.GetComponent<CombatController>();
        Stamina staminaW         = wally.GetComponent<Stamina>();
        Animator animW           = wally.GetComponent<Animator>();

        SerializedObject so = new SerializedObject(input);
        so.FindProperty("wallyCombat").objectReferenceValue   = combatW;
        so.FindProperty("wallyStamina").objectReferenceValue  = staminaW;
        so.FindProperty("wallyAnimator").objectReferenceValue = animW;
        so.FindProperty("jabData").objectReferenceValue       = jab;
        so.FindProperty("straightData").objectReferenceValue  = straight;
        so.FindProperty("hookData").objectReferenceValue      = hook;
        so.FindProperty("rivalCombat").objectReferenceValue   = combatK;
        so.ApplyModifiedProperties();

        Debug.Log("[Conectar] PlayerInputHandler conectado");
    }

    // ── FIGHT MANAGER ────────────────────────────────────────────────

    private static void ConectarFightManager(GameObject fmGO, GameObject wally, GameObject kiriku, GameObject hud)
    {
        FightManager fm = fmGO.GetComponent<FightManager>();
        if (fm == null) return;

        HUDController hudCtrl = hud != null ? hud.GetComponent<HUDController>() : null;
        if (hudCtrl == null && hud != null)
            hudCtrl = hud.GetComponentInChildren<HUDController>();

        SerializedObject so = new SerializedObject(fm);
        so.FindProperty("wallyHealth").objectReferenceValue    = wally.GetComponent<Health>();
        so.FindProperty("rivalHealth").objectReferenceValue    = kiriku.GetComponent<Health>();
        so.FindProperty("wallyStamina").objectReferenceValue   = wally.GetComponent<Stamina>();
        so.FindProperty("rivalStamina").objectReferenceValue   = kiriku.GetComponent<Stamina>();
        so.FindProperty("rivalAI").objectReferenceValue        = kiriku.GetComponent<RivalAI>();
        so.FindProperty("wallyAnimator").objectReferenceValue  = wally.GetComponent<Animator>();
        so.FindProperty("rivalAnimator").objectReferenceValue  = kiriku.GetComponent<Animator>();
        so.FindProperty("playerInput").objectReferenceValue    = wally.GetComponent<PlayerInputHandler>();
        if (hudCtrl != null)
            so.FindProperty("hud").objectReferenceValue = hudCtrl;
        so.ApplyModifiedProperties();

        // ── Paso 7: Conectar referencias faltantes de HUDController ─
        if (hudCtrl != null)
            ConectarHUD(hudCtrl, wally, kiriku);

        Debug.Log("[Conectar] FightManager cableado");
    }

    // ── HUD CONTROLLER ───────────────────────────────────────────────

    private static void ConectarHUD(HUDController hud, GameObject wally, GameObject kiriku)
    {
        SerializedObject so = new SerializedObject(hud);

        // rivalHealth puede estar null — conectarlo
        var rivalHealthProp = so.FindProperty("rivalHealth");
        if (rivalHealthProp != null && rivalHealthProp.objectReferenceValue == null)
        {
            rivalHealthProp.objectReferenceValue = kiriku.GetComponent<Health>();
            Debug.Log("[Conectar] HUDController.rivalHealth conectado");
        }

        // wallyHealth por si acaso
        var wallyHealthProp = so.FindProperty("wallyHealth");
        if (wallyHealthProp != null && wallyHealthProp.objectReferenceValue == null)
            wallyHealthProp.objectReferenceValue = wally.GetComponent<Health>();

        // wallyStamina por si acaso
        var wallyStaminaProp = so.FindProperty("wallyStamina");
        if (wallyStaminaProp != null && wallyStaminaProp.objectReferenceValue == null)
            wallyStaminaProp.objectReferenceValue = wally.GetComponent<Stamina>();

        so.ApplyModifiedProperties();
    }

    // ── ATTACK DATA ──────────────────────────────────────────────────

    private static void EnsureAttacksFolder()
    {
        if (!AssetDatabase.IsValidFolder(ATTACKS_PATH))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath,
                "ScriptableObjects/Attacks"));
            AssetDatabase.Refresh();
        }
    }

    private static AttackData CrearOCargar(string fileName, string attackName,
        int damage, float staminaCost,
        float windup, float active, float recovery,
        string trigger, DodgePosition[] avoids)
    {
        string path = $"{ATTACKS_PATH}/{fileName}.asset";
        AttackData data = AssetDatabase.LoadAssetAtPath<AttackData>(path);
        if (data != null) return data;

        data = ScriptableObject.CreateInstance<AttackData>();
        data.attackName       = attackName;
        data.damage           = damage;
        data.staminaCost      = staminaCost;
        data.windupTime       = windup;
        data.activeTime       = active;
        data.recoveryTime     = recovery;
        data.animationTrigger = trigger;
        data.dodgesThatAvoid  = avoids;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"[Conectar] AttackData creado: {fileName}");
        return data;
    }
}
#endif
