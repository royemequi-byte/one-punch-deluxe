using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Herramienta de editor: Tools → 22 Crear HUD KOF
/// Borra el Canvas de HUD anterior y crea uno nuevo con layout estilo King of Fighters:
/// barras de vida arriba con ghost amarillo, gemas de ronda, timer central y panel de anuncios.
/// Al terminar conecta las referencias en FightManager automáticamente.
/// </summary>
public static class CrearHUDKOF
{
    [MenuItem("Tools/22 Crear HUD KOF")]
    public static void Crear()
    {
        // 1. Eliminar TODOS los Canvas existentes en la escena (el viejo y cualquier duplicado)
        var todosLosCanvas = Object.FindObjectsOfType<Canvas>(true);
        foreach (var c in todosLosCanvas)
        {
            // Solo borrar los que son raíz (no sub-canvas dentro de prefabs de TMP, etc.)
            if (c.transform.parent == null)
            {
                Debug.Log($"[HUD KOF] Eliminando Canvas: {c.gameObject.name}");
                Undo.DestroyObjectImmediate(c.gameObject);
            }
        }

        // Eliminar también el objeto HUD separado si existe (tenía HUDController suelto)
        var hudViejo = GameObject.Find("HUD");
        if (hudViejo != null)
        {
            Undo.DestroyObjectImmediate(hudViejo);
            Debug.Log("[HUD KOF] Objeto HUD viejo eliminado.");
        }

        // 2. Canvas principal
        GameObject canvasGO = new GameObject("HUD_Canvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear HUD KOF");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── FRANJA TOP (fondo oscuro, 120px) ─────────────────────────
        // Layout vertical (y=0 abajo, y=1 arriba del panel):
        //   0.00 - 0.14 → gemas de ronda
        //   0.14 - 0.28 → stamina (solo P1)
        //   0.28 - 0.62 → barra de vida
        //   0.62 - 1.00 → nombre del personaje
        GameObject topBG = Panel("TopBackground", canvasGO.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -120), new Vector2(0, 0),
            new Color(0f, 0f, 0f, 0.80f));

        // ── LADO P1 (Wally — izquierda) ───────────────────────────
        TMP_Text wallyName = Texto("P1Name", topBG.transform,
            new Vector2(0f, 0.62f), new Vector2(0.42f, 1f), new Vector2(12, 0), new Vector2(-4, -3),
            "WALLY", 26, Color.white, TextAlignmentOptions.Left);
        wallyName.fontStyle = FontStyles.Bold;

        GameObject wallyHealthBG = Panel("P1HealthBG", topBG.transform,
            new Vector2(0f, 0.28f), new Vector2(0.42f, 0.60f), new Vector2(10, 2), new Vector2(-6, -2),
            new Color(0.06f, 0.06f, 0.06f, 1f));
        Image wallyGhost = BarraVida("P1HealthGhost", wallyHealthBG.transform,
            new Color(0.95f, 0.85f, 0.10f), 0);
        Image wallyBar   = BarraVida("P1Health",      wallyHealthBG.transform,
            new Color(0.08f, 0.90f, 0.12f), 1);

        GameObject wallyStaminaBG = Panel("P1StaminaBG", topBG.transform,
            new Vector2(0f, 0.14f), new Vector2(0.32f, 0.26f), new Vector2(10, 2), new Vector2(-6, -2),
            new Color(0.04f, 0.04f, 0.04f, 1f));
        Image wallyStaminaBar = BarraVida("P1Stamina", wallyStaminaBG.transform,
            new Color(0.0f, 0.75f, 1f), 0);

        // Gemas P1 — fila inferior, claramente separadas del nombre y la barra
        Image[] wallyGemas = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject g = new GameObject($"P1Gema{i+1}", typeof(RectTransform));
            g.transform.SetParent(topBG.transform, false);
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.012f + i * 0.030f, 0.04f);
            rt.anchorMax = new Vector2(0.012f + i * 0.030f + 0.022f, 0.13f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = g.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.18f);
            wallyGemas[i] = img;
        }

        // ── TIMER CENTRAL ─────────────────────────────────────────
        TMP_Text timerText = Texto("TimerText", topBG.transform,
            new Vector2(0.42f, 0f), new Vector2(0.58f, 1f), Vector2.zero, Vector2.zero,
            "99", 56, Color.white, TextAlignmentOptions.Center);
        timerText.fontStyle = FontStyles.Bold;

        // ── LADO P2 (Rival — derecha) ──────────────────────────────
        TMP_Text rivalName = Texto("P2Name", topBG.transform,
            new Vector2(0.58f, 0.62f), new Vector2(1f, 1f), new Vector2(4, 0), new Vector2(-12, -3),
            "RIVAL", 26, Color.white, TextAlignmentOptions.Right);
        rivalName.fontStyle = FontStyles.Bold;

        GameObject rivalHealthBG = Panel("P2HealthBG", topBG.transform,
            new Vector2(0.58f, 0.28f), new Vector2(1f, 0.60f), new Vector2(6, 2), new Vector2(-10, -2),
            new Color(0.06f, 0.06f, 0.06f, 1f));
        Image rivalGhost = BarraVida("P2HealthGhost", rivalHealthBG.transform,
            new Color(0.95f, 0.85f, 0.10f), 0, fillFromRight: true);
        Image rivalBar   = BarraVida("P2Health",      rivalHealthBG.transform,
            new Color(0.08f, 0.90f, 0.12f), 1, fillFromRight: true);

        // Gemas P2 — fila inferior lado derecho
        Image[] rivalGemas = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject g = new GameObject($"P2Gema{i+1}", typeof(RectTransform));
            g.transform.SetParent(topBG.transform, false);
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.966f - i * 0.030f - 0.022f, 0.04f);
            rt.anchorMax = new Vector2(0.966f - i * 0.030f,           0.13f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = g.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.18f);
            rivalGemas[i] = img;
        }

        // ── PANEL DE ANUNCIOS (centrado en pantalla) ──────────────
        GameObject anuncioPanel = new GameObject("AnuncioPanel", typeof(RectTransform));
        anuncioPanel.transform.SetParent(canvasGO.transform, false);
        var anuncioRT = anuncioPanel.GetComponent<RectTransform>();
        anuncioRT.anchorMin = new Vector2(0.5f, 0.5f);
        anuncioRT.anchorMax = new Vector2(0.5f, 0.5f);
        anuncioRT.pivot     = new Vector2(0.5f, 0.5f);
        anuncioRT.sizeDelta = new Vector2(900, 130);
        anuncioRT.anchoredPosition = Vector2.zero;
        anuncioPanel.SetActive(false);

        TMP_Text anuncioText = Texto("AnuncioText", anuncioPanel.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "FIGHT!", 80, Color.white, TextAlignmentOptions.Center);
        anuncioText.fontStyle = FontStyles.Bold;
        // Outline estilo KOF
        var outline = anuncioText.gameObject.AddComponent<Outline>();
        outline.effectColor    = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        // Componente AnunciadorRound
        AnunciadorRound anunciador = anuncioPanel.transform.parent.gameObject
            .AddComponent<AnunciadorRound>();
        SetPrivate(anunciador, "panel",   anuncioPanel);
        SetPrivate(anunciador, "texto",   anuncioText);

        // ── PANEL RESULTADO FINAL ─────────────────────────────────
        GameObject resultPanel = Panel("ResultadoPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-260, -160), new Vector2(260, 160),
            new Color(0f, 0f, 0f, 0.88f));
        resultPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        resultPanel.SetActive(false);

        TMP_Text resultText = Texto("ResultadoText", resultPanel.transform,
            new Vector2(0, 0.45f), new Vector2(1, 1f), Vector2.zero, Vector2.zero,
            "¡WALLY GANA!\n2 - 1", 36, Color.white, TextAlignmentOptions.Center);

        Button btnReintentar = CrearBoton("BtnReintentar", resultPanel.transform,
            new Vector2(0.05f, 0.05f), new Vector2(0.48f, 0.40f), "REINTENTAR",
            new Color(0.15f, 0.55f, 0.15f));
        Button btnMenu = CrearBoton("BtnMenu", resultPanel.transform,
            new Vector2(0.52f, 0.05f), new Vector2(0.95f, 0.40f), "MENÚ",
            new Color(0.45f, 0.10f, 0.10f));

        // ── AGREGAR Y CONFIGURAR HUDController ────────────────────
        HUDController hud = canvasGO.AddComponent<HUDController>();
        SetPrivate(hud, "wallyBar",        wallyBar);
        SetPrivate(hud, "wallyBarGhost",   wallyGhost);
        SetPrivate(hud, "rivalBar",        rivalBar);
        SetPrivate(hud, "rivalBarGhost",   rivalGhost);
        SetPrivate(hud, "wallyStaminaBar", wallyStaminaBar);
        SetPrivate(hud, "wallyGemas",      wallyGemas);
        SetPrivate(hud, "rivalGemas",      rivalGemas);
        SetPrivate(hud, "timerText",       timerText);
        SetPrivate(hud, "anunciador",      anunciador);
        SetPrivate(hud, "resultPanel",     resultPanel);
        SetPrivate(hud, "resultText",      resultText);
        SetPrivate(hud, "botonReintentar", btnReintentar);
        SetPrivate(hud, "botonMenu",       btnMenu);

        // ── CONECTAR TODAS LAS REFERENCIAS AUTOMÁTICAMENTE ────────
        ConectarReferencias(hud);
        ConectarFightManager(hud);

        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[HUD KOF] ¡Canvas creado y referencias conectadas automáticamente!");
    }

    // ── HELPERS ───────────────────────────────────────────────────

    static void ConectarReferencias(HUDController hud)
    {
        var hudSO = new SerializedObject(hud);

        // Estrategia 1: leer wallyHealth y rivalHealth del FightManager (ya los tiene conectados)
        var fm = Object.FindObjectOfType<FightManager>();
        if (fm != null)
        {
            var fmSO = new SerializedObject(fm);
            var wh = fmSO.FindProperty("wallyHealth").objectReferenceValue;
            var rh = fmSO.FindProperty("rivalHealth").objectReferenceValue;
            if (wh != null) { hudSO.FindProperty("wallyHealth").objectReferenceValue = wh; Debug.Log($"[HUD KOF] wallyHealth → {wh.name}"); }
            if (rh != null) { hudSO.FindProperty("rivalHealth").objectReferenceValue = rh; Debug.Log($"[HUD KOF] rivalHealth → {rh.name}"); }
        }

        // Estrategia 2: Stamina — buscar la única Stamina en escena (solo Wally la tiene)
        var staminas = Object.FindObjectsOfType<Stamina>(true);
        if (staminas.Length > 0)
        {
            // Preferir la del mismo GameObject que wallyHealth
            var wallyHealthRef = hudSO.FindProperty("wallyHealth").objectReferenceValue as Health;
            Stamina elegida = null;
            if (wallyHealthRef != null)
                elegida = wallyHealthRef.GetComponent<Stamina>();
            if (elegida == null)
                elegida = staminas[0];

            hudSO.FindProperty("wallyStamina").objectReferenceValue = elegida;
            Debug.Log($"[HUD KOF] wallyStamina → {elegida.gameObject.name}");
        }

        hudSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(hud);
    }

    static void ConectarFightManager(HUDController hud)
    {
        var fm = Object.FindObjectOfType<FightManager>();
        if (fm == null) { Debug.LogWarning("[HUD KOF] FightManager no encontrado en la escena."); return; }

        var so = new SerializedObject(fm);
        so.FindProperty("hud").objectReferenceValue = hud;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(fm);
        Debug.Log("[HUD KOF] HUDController conectado al FightManager.");
    }

    static GameObject Panel(string nombre, Transform padre,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static Image BarraVida(string nombre, Transform padre, Color color, int siblingIndex,
        bool fillFromRight = false)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        go.transform.SetSiblingIndex(siblingIndex);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color      = color;
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = fillFromRight ? (int)Image.OriginHorizontal.Right
                                       : (int)Image.OriginHorizontal.Left;
        img.fillAmount = 1f;
        return img;
    }

    static TMP_Text Texto(string nombre, Transform padre,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        string contenido, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = contenido;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = align;
        return tmp;
    }

    static Button CrearBoton(string nombre, Transform padre,
        Vector2 anchorMin, Vector2 anchorMax, string etiqueta, Color colorFondo)
    {
        var go = Panel(nombre, padre, anchorMin, anchorMax, new Vector2(6,4), new Vector2(-6,-4), colorFondo);
        var btn = go.AddComponent<Button>();
        Texto("Label", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
              etiqueta, 24, Color.white, TextAlignmentOptions.Center);
        return btn;
    }

    // Usar SerializedObject para setear campos [SerializeField] private
    static void SetPrivate(Object target, string fieldName, object value)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null) { Debug.LogWarning($"[HUD KOF] Campo '{fieldName}' no encontrado en {target.GetType().Name}"); return; }

        if      (value is Object o)   prop.objectReferenceValue = o;
        else if (value is float f)    prop.floatValue = f;
        else if (value is int i)      prop.intValue   = i;
        else if (value is bool b)     prop.boolValue  = b;
        else if (value is Image[] imgs)
        {
            prop.arraySize = imgs.Length;
            for (int x = 0; x < imgs.Length; x++)
                prop.GetArrayElementAtIndex(x).objectReferenceValue = imgs[x];
        }

        so.ApplyModifiedProperties();
    }
}
