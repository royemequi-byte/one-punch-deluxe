#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixKOF
{
    // Layout KOF: ambos al mismo nivel de suelo del ring (y=-4)
    // Wally NUEVO (313x390px PPU=200 = 1.565x1.95u) escala 2 -> 3.13x3.9u, izquierda
    static readonly Vector3 WALLY_POS   = new Vector3(-3f, -4f, 0f);
    static readonly Vector3 WALLY_SCALE = new Vector3(2f, 2f, 1f);

    // Kiriku (254x353px PPU=200 = 1.27x1.765u) escala 2.3 -> 2.92x4.06u, derecha, flipX
    static readonly Vector3 RIVAL_POS   = new Vector3(3f, -4f, 0f);
    static readonly Vector3 RIVAL_SCALE = new Vector3(-2.3f, 2.3f, 1f);

    [MenuItem("Tools/25 Fix KOF - Abrir y Montar")]
    public static void AbrirYMontar()
    {
        string scenePath = "Assets/dia 1.unity";
        Scene active = SceneManager.GetActiveScene();
        if (active.name != "dia 1")
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(scenePath);
        }
        Montar();
    }

    [MenuItem("Tools/26 Fix KOF - Solo Montar")]
    public static void Montar()
    {
        string log = "";

        GameObject[] todos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        GameObject wally = null;
        GameObject rival = null;

        foreach (var go in todos)
        {
            if (go.GetComponent<PlayerInputHandler>() != null) wally = go;
            if (go.GetComponent<RivalAI>() != null)            rival = go;
        }

        log += $"Wally: {(wally != null ? wally.name : "NO ENCONTRADO")}\n";
        log += $"Rival: {(rival != null ? rival.name : "NO ENCONTRADO")}\n\n";

        if (wally == null && rival == null)
        {
            string diag = "Objetos en escena:\n";
            foreach (var go in todos) diag += "  " + go.name + "\n";
            EditorUtility.DisplayDialog("Diagnostico", diag, "OK");
            return;
        }

        // ── WALLY ──────────────────────────────────────────────────
        if (wally != null)
        {
            wally.transform.position   = WALLY_POS;
            wally.transform.localScale = WALLY_SCALE;
            SpriteRenderer sr = wally.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 2;
                sr.enabled = true;
                // Sprite nuevo de lado (wally_idle_0 de Wally_Nuevos)
                Object[] wallyAssets = AssetDatabase.LoadAllAssetsAtPath(
                    "Assets/Art/Personajes/Wally_Nuevos/wally_idle.png");
                foreach (Object a in wallyAssets)
                    if (a is Sprite s) { sr.sprite = s; break; }
            }
            // Animator de Wally
            RuntimeAnimatorController wallyCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/Wally/WallyAnimator.controller");
            Animator anim = wally.GetComponent<Animator>();
            if (anim != null && wallyCtrl != null) anim.runtimeAnimatorController = wallyCtrl;

            log += "Wally: sprite nuevo lado, pos (-3,-4) escala 2\n";
        }

        // ── KIRIKU (RIVAL) ─────────────────────────────────────────
        if (rival != null)
        {
            rival.transform.position   = RIVAL_POS;
            rival.transform.localScale = RIVAL_SCALE;  // scale.x negativo = flipX
            SpriteRenderer sr = rival.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sortingOrder = 1; sr.enabled = true; }

            // Asignar sprite idle de Kiriku (primer frame del sheet)
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Art/Personajes/Kiriku/kiriku_idle.png");
            foreach (Object a in assets)
                if (a is Sprite s && sr != null) { sr.sprite = s; break; }

            // Asignar KirikuAnimator.controller
            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/Kiriku/KirikuAnimator.controller");
            Animator anim = rival.GetComponent<Animator>();
            if (anim != null && ctrl != null) anim.runtimeAnimatorController = ctrl;

            log += "Kiriku: posicion (3,-4) escala 2.3 flipX\n";
        }

        // ── FIGHTMANAGER: actualizar wallyPosicion/wallyEscala ─────
        FightManager fm = Object.FindFirstObjectByType<FightManager>();
        if (fm != null && wally != null)
        {
            SerializedObject soFm = new SerializedObject(fm);

            var posProp = soFm.FindProperty("wallyPosicion");
            if (posProp != null) posProp.vector3Value = WALLY_POS;

            var scaleProp = soFm.FindProperty("wallyEscala");
            if (scaleProp != null) scaleProp.vector3Value = WALLY_SCALE;

            // Conectar wallyStamina y rivalStamina si son null
            var ws = soFm.FindProperty("wallyStamina");
            if (ws != null && ws.objectReferenceValue == null)
            {
                Stamina st = wally.GetComponent<Stamina>();
                if (st != null) ws.objectReferenceValue = st;
            }
            var rs = soFm.FindProperty("rivalStamina");
            if (rs != null && rs.objectReferenceValue == null && rival != null)
            {
                Stamina st = rival.GetComponent<Stamina>();
                if (st != null) rs.objectReferenceValue = st;
            }

            soFm.ApplyModifiedProperties();
            log += "FightManager: wallyPosicion y wallyEscala actualizados\n";
        }

        // ── FONDO ──────────────────────────────────────────────────
        foreach (var go in todos)
        {
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            if (go.name == "bg_menu")        { sr.sortingOrder = -2; log += "bg_menu: -2\n"; }
            if (go.name == "BackgroundRing") { sr.sortingOrder = -3; log += "BackgroundRing: -3\n"; }
        }

        // ── HUD ────────────────────────────────────────────────────
        HUDController hud = Object.FindFirstObjectByType<HUDController>();
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            BindImg(so, "wallyBar",        "WallyHealthBar");
            BindImg(so, "rivalBar",        "RivalHealthBar");
            BindImg(so, "wallyStaminaBar", "WallyStaminaBar");
            var ep = so.FindProperty("escenaMenu");
            if (ep != null && string.IsNullOrEmpty(ep.stringValue)) ep.stringValue = "menu";
            so.ApplyModifiedProperties();
            log += "HUD: barras reconectadas\n";
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Fix KOF listo",
            log + "\nCtrl+S para guardar, luego Play.", "OK");
    }

    static void BindImg(SerializedObject so, string field, string goName)
    {
        var p = so.FindProperty(field);
        if (p == null || p.objectReferenceValue != null) return;
        GameObject go = GameObject.Find(goName);
        if (go == null) return;
        Image img = go.GetComponent<Image>();
        if (img != null) p.objectReferenceValue = img;
    }
}
#endif
