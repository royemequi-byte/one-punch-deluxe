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
        // 1. Eliminar canvas HUD anterior si existe
        var anterior = GameObject.Find("HUD_Canvas");
        if (anterior != null)
        {
            Undo.DestroyObjectImmediate(anterior);
            Debug.Log("[HUD KOF] Canvas anterior eliminado.");
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

        // ── FRANJA TOP (fondo oscuro) ─────────────────────────────
        GameObject topBG = Panel("TopBackground", canvasGO.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -110), new Vector2(0, 0),
            new Color(0f, 0f, 0f, 0.78f));

        // ── LADO P1 (Wally — izquierda) ───────────────────────────
        // Nombre
        TMP_Text wallyName = Texto("P1Name", topBG.transform,
            new Vector2(0, 0.55f), new Vector2(0.38f, 1f), new Vector2(14, 0), new Vector2(0, -4),
            "WALLY", 28, Color.white, TextAlignmentOptions.Left);
        wallyName.fontStyle = FontStyles.Bold;

        // Barra de vida ghost (amarillo, detrás)
        GameObject wallyHealthBG = Panel("P1HealthBG", topBG.transform,
            new Vector2(0, 0.05f), new Vector2(0.40f, 0.52f), new Vector2(10, 0), new Vector2(-8, 0),
            new Color(0.08f, 0.08f, 0.08f, 1f));

        Image wallyGhost = BarraVida("P1HealthGhost", wallyHealthBG.transform,
            new Color(0.95f, 0.85f, 0.10f), 0);
        Image wallyBar   = BarraVida("P1Health",      wallyHealthBG.transform,
            new Color(0.08f, 0.90f, 0.12f), 1);

        // Stamina (delgada, debajo de la vida)
        GameObject wallyStaminaBG = Panel("P1StaminaBG", topBG.transform,
            new Vector2(0, 0f), new Vector2(0.30f, 0.07f), new Vector2(10, 2), new Vector2(-8, -2),
            new Color(0.05f, 0.05f, 0.05f, 1f));
        Image wallyStaminaBar = BarraVida("P1Stamina", wallyStaminaBG.transform,
            new Color(0.0f, 0.75f, 1f), 0);

        // Gemas de ronda P1 (2 cuadraditos dorados)
        Image[] wallyGemas = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject g = Panel($"P1Gema{i+1}", topBG.transform,
                new Vector2(0.01f + i * 0.022f, 0f),
                new Vector2(0.01f + i * 0.022f + 0.018f, 0.0f),
                new Vector2(0, 3), new Vector2(0, -3),
                new Color(0.18f, 0.18f, 0.18f));
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.01f + i * 0.025f, 0.65f);
            rt.anchorMax = new Vector2(0.01f + i * 0.025f + 0.018f, 0.95f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            wallyGemas[i] = g.GetComponent<Image>();
        }

        // ── TIMER CENTRAL ─────────────────────────────────────────
        TMP_Text timerText = Texto("TimerText", topBG.transform,
            new Vector2(0.40f, 0f), new Vector2(0.60f, 1f), Vector2.zero, Vector2.zero,
            "99", 52, Color.white, TextAlignmentOptions.Center);
        timerText.fontStyle = FontStyles.Bold;

        // ── LADO P2 (Rival — derecha) ──────────────────────────────
        // Nombre
        TMP_Text rivalName = Texto("P2Name", topBG.transform,
            new Vector2(0.62f, 0.55f), new Vector2(1f, 1f), new Vector2(0, 0), new Vector2(-14, -4),
            "RIVAL", 28, Color.white, TextAlignmentOptions.Right);
        rivalName.fontStyle = FontStyles.Bold;

        // Barra de vida rival (fillOrigin=Right para deplecionar desde la izquierda)
        GameObject rivalHealthBG = Panel("P2HealthBG", topBG.transform,
            new Vector2(0.60f, 0.05f), new Vector2(1f, 0.52f), new Vector2(8, 0), new Vector2(-10, 0),
            new Color(0.08f, 0.08f, 0.08f, 1f));

        Image rivalGhost = BarraVida("P2HealthGhost", rivalHealthBG.transform,
            new Color(0.95f, 0.85f, 0.10f), 0, fillFromRight: true);
        Image rivalBar   = BarraVida("P2Health",      rivalHealthBG.transform,
            new Color(0.08f, 0.90f, 0.12f), 1, fillFromRight: true);

        // Gemas de ronda P2
        Image[] rivalGemas = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject g = new GameObject($"P2Gema{i+1}", typeof(RectTransform));
            g.transform.SetParent(topBG.transform, false);
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.985f - (i + 1) * 0.025f, 0.65f);
            rt.anchorMax = new Vector2(0.985f - i * 0.025f - 0.007f, 0.95f);
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

        // ── CONECTAR AL FightManager EN ESCENA ────────────────────
        ConectarFightManager(hud);

        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[HUD KOF] ¡Canvas creado! Recuerda conectar wallyHealth, rivalHealth y wallyStamina en el Inspector de HUDController.");
    }

    // ── HELPERS ───────────────────────────────────────────────────

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
