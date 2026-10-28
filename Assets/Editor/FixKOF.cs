#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public class FixKOF
{
    // Layout KOF XV: mismo suelo, escala igual para ambos
    // Wally nuevo (1.565x1.95u/frame) escala 3 → 4.7x5.85u, cabeza y=2.85
    static readonly Vector3 WALLY_POS   = new Vector3(-2f, -3f, 0f);
    static readonly Vector3 WALLY_SCALE = new Vector3(3f, 3f, 1f);

    // Kiriku (1.27x1.765u/frame) escala -3 → flipX, lado derecho
    static readonly Vector3 RIVAL_POS   = new Vector3(2f, -3f, 0f);
    static readonly Vector3 RIVAL_SCALE = new Vector3(-3f, 3f, 1f);

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
                Object[] wallyAssets = AssetDatabase.LoadAllAssetsAtPath(
                    "Assets/Art/Personajes/Wally_Nuevos/wally_idle.png");
                foreach (Object a in wallyAssets)
                    if (a is Sprite s) { sr.sprite = s; break; }
            }
            RuntimeAnimatorController wallyCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/es/Wally/Characters/Wally/WallyAnimator.controller");
            Animator anim = wally.GetComponent<Animator>();
            if (anim != null && wallyCtrl != null) anim.runtimeAnimatorController = wallyCtrl;

            log += "Wally: sprite lado, pos (-2,-3) escala 3\n";
        }

        // ── KIRIKU (RIVAL) ─────────────────────────────────────────
        if (rival != null)
        {
            rival.transform.position   = RIVAL_POS;
            rival.transform.localScale = RIVAL_SCALE;
            SpriteRenderer sr = rival.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sortingOrder = 1; sr.enabled = true; }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Art/Personajes/Kiriku/kiriku_idle.png");
            foreach (Object a in assets)
                if (a is Sprite s && sr != null) { sr.sprite = s; break; }

            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/Kiriku/KirikuAnimator.controller");
            Animator anim = rival.GetComponent<Animator>();
            if (anim != null && ctrl != null) anim.runtimeAnimatorController = ctrl;

            log += "Kiriku: pos (2,-3) escala -3 flipX\n";
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
        log += FixHUDLayout(todos);

        // ── FIGHTMANAGER: conectar staminas ────────────────────────
        FightManager fm = Object.FindFirstObjectByType<FightManager>();
        if (fm != null && wally != null && rival != null)
        {
            SerializedObject soFm = new SerializedObject(fm);
            var ws = soFm.FindProperty("wallyStamina");
            if (ws != null && ws.objectReferenceValue == null)
            {
                Stamina st = wally.GetComponent<Stamina>();
                if (st != null) ws.objectReferenceValue = st;
            }
            var rs = soFm.FindProperty("rivalStamina");
            if (rs != null && rs.objectReferenceValue == null)
            {
                Stamina st = rival.GetComponent<Stamina>();
                if (st != null) rs.objectReferenceValue = st;
            }
            soFm.ApplyModifiedProperties();
            log += "FightManager: staminas conectadas\n";
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Fix KOF listo",
            log + "\nCtrl+S para guardar, luego Play.", "OK");
    }

    // ── LAYOUT KOF XV: barras arriba, timer centro, stamina debajo ─
    static string FixHUDLayout(GameObject[] todos)
    {
        string log = "";

        GameObject wallyBarGO  = null;
        GameObject rivalBarGO  = null;
        GameObject staminaGO   = null;
        GameObject timerGO     = null;

        // Busca por nombre exacto O con espacio al inicio (bug del Inspector)
        foreach (var go in todos)
        {
            string n = go.name.Trim();
            if (n == "WallyHealthBar")  { wallyBarGO = go;  go.name = "WallyHealthBar"; }
            if (n == "RivalHealthBar")  { rivalBarGO = go; }
            if (n == "WallyStaminaBar") { staminaGO  = go; }
            if (n == "TimerText")       { timerGO    = go; }
        }

        // ── Barra vida Wally: esquina superior izquierda → centro ──
        if (wallyBarGO != null)
        {
            SetRect(wallyBarGO, new Vector2(0.03f, 0.87f), new Vector2(0.47f, 0.95f));
            Image img = wallyBarGO.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.type        = Image.Type.Filled;
                img.fillMethod  = Image.FillMethod.Horizontal;
                img.fillOrigin  = 0;   // rellena de izquierda a derecha
                img.fillAmount  = 1f;
                img.color       = new Color(0.08f, 0.90f, 0.12f);
            }
            log += "WallyHealthBar: top-left KOF\n";
        }
        else log += "⚠ WallyHealthBar no encontrado\n";

        // ── Barra vida Rival: esquina superior derecha → centro ────
        if (rivalBarGO != null)
        {
            SetRect(rivalBarGO, new Vector2(0.53f, 0.87f), new Vector2(0.97f, 0.95f));
            Image img = rivalBarGO.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.type        = Image.Type.Filled;
                img.fillMethod  = Image.FillMethod.Horizontal;
                img.fillOrigin  = 1;   // rellena de derecha a izquierda
                img.fillAmount  = 1f;
                img.color       = new Color(0.08f, 0.90f, 0.12f);
            }
            log += "RivalHealthBar: top-right KOF\n";
        }
        else log += "⚠ RivalHealthBar no encontrado\n";

        // ── Barra stamina Wally: debajo de vida, más estrecha ──────
        if (staminaGO != null)
        {
            SetRect(staminaGO, new Vector2(0.03f, 0.82f), new Vector2(0.38f, 0.87f));
            Image img = staminaGO.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.type        = Image.Type.Filled;
                img.fillMethod  = Image.FillMethod.Horizontal;
                img.fillOrigin  = 0;
                img.fillAmount  = 1f;
                img.color       = new Color(0.10f, 0.50f, 1.00f);
            }
            log += "WallyStaminaBar: below health KOF\n";
        }

        // ── Timer: centrado en la parte superior ───────────────────
        if (timerGO != null)
        {
            SetRect(timerGO, new Vector2(0.44f, 0.89f), new Vector2(0.56f, 0.97f));
            TMP_Text txt = timerGO.GetComponent<TMP_Text>();
            if (txt != null)
            {
                txt.alignment  = TextAlignmentOptions.Center;
                txt.fontSize   = 48;
                txt.fontStyle  = FontStyles.Bold;
                txt.color      = Color.white;
            }
            log += "TimerText: top-center KOF\n";
        }
        else log += "⚠ TimerText no encontrado\n";

        // ── HUDController: reconectar barras ───────────────────────
        HUDController hud = Object.FindFirstObjectByType<HUDController>();
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            BindImgChildren(so, "wallyBar",        "WallyHealthBar");
            BindImgChildren(so, "rivalBar",        "RivalHealthBar");
            BindImgChildren(so, "wallyStaminaBar", "WallyStaminaBar");
            var ep = so.FindProperty("escenaMenu");
            if (ep != null && string.IsNullOrEmpty(ep.stringValue)) ep.stringValue = "menu";
            so.ApplyModifiedProperties();
            log += "HUDController: barras reconectadas\n";
        }

        return log;
    }

    // Ancla con modo stretch (anclaje en porcentaje de pantalla)
    static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
    }

    // Busca por nombre (incluyendo el bug del espacio) y asigna el Image hijo
    static void BindImgChildren(SerializedObject so, string field, string goName)
    {
        var p = so.FindProperty(field);
        if (p == null) return;

        // Busca el GO por nombre exacto o con espacio
        GameObject go = GameObject.Find(goName) ?? GameObject.Find(" " + goName);
        if (go == null)
        {
            // Último recurso: buscar por nombre trimado
            foreach (var candidate in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (candidate.name.Trim() == goName) { go = candidate; break; }
        }
        if (go == null) return;

        // Usa GetComponentInChildren en caso de que Image esté en un hijo
        Image img = go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>();
        if (img != null) p.objectReferenceValue = img;
    }
}
#endif
