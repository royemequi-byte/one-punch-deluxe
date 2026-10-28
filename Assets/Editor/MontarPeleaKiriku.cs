#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Monta la pelea completa contra Kirikú con aspecto KOF/Punch-Out!!
/// - Wally (espalda) en primer plano abajo
/// - Kirikú (frente) grande al centro-arriba
/// - HUD KOF con barras de vida, timer y gemas
/// Uso: Tools -> 24 Montar Pelea Kiriku
/// </summary>
public class MontarPeleaKiriku : EditorWindow
{
    [MenuItem("Tools/24 Montar Pelea Kiriku")]
    public static void Montar()
    {
        // ── 1. ENCONTRAR PERSONAJES por componente (ignora nombre e inactivos) ──
        GameObject wally = null;
        GameObject rival = null;

        // Wally tiene PlayerInputHandler; Rival tiene RivalAI
        PlayerInputHandler pip = Object.FindFirstObjectByType<PlayerInputHandler>(
            FindObjectsInactive.Include);
        if (pip != null) wally = pip.gameObject;

        RivalAI rai = Object.FindFirstObjectByType<RivalAI>(
            FindObjectsInactive.Include);
        if (rai != null) rival = rai.gameObject;

        // Fallback por nombre si los componentes no están
        if (wally == null)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
                if (go.name == "Wally" && go.scene.IsValid()) { wally = go; break; }
        }
        if (rival == null)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
                if ((go.name == "Rival" || go.name == "Kiriku") && go.scene.IsValid()) { rival = go; break; }
        }

        if (wally == null || rival == null)
        {
            EditorUtility.DisplayDialog("Error",
                $"No se encontraron los personajes.\n" +
                $"Wally: {(wally != null ? "OK" : "FALTA")}\n" +
                $"Rival: {(rival != null ? "OK" : "FALTA")}\n\n" +
                "Verifica que la escena 'dia 1' esté abierta.", "OK");
            return;
        }

        // ── 2. POSICION Y ESCALA — estilo Punch-Out!! ─────────────────────
        // Wally: espalda, abajo, primer plano
        wally.transform.position   = new Vector3(0f, -2.2f, 0f);
        wally.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        SpriteRenderer wallySR = wally.GetComponent<SpriteRenderer>();
        if (wallySR != null)
        {
            wallySR.sortingOrder = 2;
            wallySR.enabled      = true;
        }

        // Rival -> Kirikú: frente, centro-arriba, más grande
        rival.transform.position   = new Vector3(0f, 1.2f, 0f);
        rival.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
        SpriteRenderer rivalSR = rival.GetComponent<SpriteRenderer>();
        if (rivalSR != null)
        {
            rivalSR.sortingOrder = 1;
            rivalSR.enabled      = true;
        }

        // ── 3. CAMBIAR SPRITES DE RIVAL A KIRIKÚ ─────────────────────────
        // Cargar sprite idle de Kirikú (Single mode — el primer sprite)
        Object[] kirikuAssets = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Art/Personajes/Kiriku/kiriku_idle.png");
        Sprite kirikuIdleSprite = null;
        foreach (Object a in kirikuAssets)
        {
            if (a is Sprite s) { kirikuIdleSprite = s; break; }
        }
        if (kirikuIdleSprite != null && rivalSR != null)
            rivalSR.sprite = kirikuIdleSprite;

        // ── 4. CAMBIAR ANIMATOR DE RIVAL A KIRIKÚ ────────────────────────
        Animator rivalAnim = rival.GetComponent<Animator>();
        RuntimeAnimatorController kirikuCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animations/Kiriku/KirikuAnimator.controller");
        if (rivalAnim != null && kirikuCtrl != null)
            rivalAnim.runtimeAnimatorController = kirikuCtrl;

        // ── 5. ACTUALIZAR FIGHTMANAGER ────────────────────────────────────
        FightManager fm = Object.FindFirstObjectByType<FightManager>();
        if (fm != null)
        {
            SerializedObject soFm = new SerializedObject(fm);

            // Conectar wallyStamina y rivalStamina si son null
            var wprop = soFm.FindProperty("wallyStamina");
            if (wprop != null && wprop.objectReferenceValue == null)
            {
                Stamina ws = wally.GetComponent<Stamina>();
                if (ws != null) wprop.objectReferenceValue = ws;
            }
            var rprop = soFm.FindProperty("rivalStamina");
            if (rprop != null && rprop.objectReferenceValue == null)
            {
                Stamina rs = rival.GetComponent<Stamina>();
                if (rs != null) rprop.objectReferenceValue = rs;
            }

            // Posicion y escala de Wally para FightManager
            var posProp = soFm.FindProperty("wallyPosicion");
            if (posProp != null) posProp.vector3Value = new Vector3(0f, -2.2f, 0f);
            var scaleProp = soFm.FindProperty("wallyEscala");
            if (scaleProp != null) scaleProp.vector3Value = new Vector3(2.5f, 2.5f, 1f);

            soFm.ApplyModifiedProperties();
        }

        // ── 6. HUD — reconectar barras al HUDController correcto ─────────
        HUDController hud = Object.FindFirstObjectByType<HUDController>();
        if (hud != null)
        {
            SerializedObject soHud = new SerializedObject(hud);
            ConectarImagen(soHud, "wallyBar",       "WallyHealthBar");
            ConectarImagen(soHud, "rivalBar",       "RivalHealthBar");
            ConectarImagen(soHud, "wallyStaminaBar","WallyStaminaBar");

            var escProp = soHud.FindProperty("escenaMenu");
            if (escProp != null && string.IsNullOrEmpty(escProp.stringValue))
                escProp.stringValue = "menu";

            soHud.ApplyModifiedProperties();
        }

        // ── 7. FONDO — asegurar sorting order correcto ────────────────────
        GameObject bgMenu = GameObject.Find("bg_menu");
        if (bgMenu != null)
        {
            SpriteRenderer bgSR = bgMenu.GetComponent<SpriteRenderer>();
            if (bgSR != null) bgSR.sortingOrder = -2;
        }
        GameObject bgRing = GameObject.Find("BackgroundRing");
        if (bgRing != null)
        {
            SpriteRenderer bgSR = bgRing.GetComponent<SpriteRenderer>();
            if (bgSR != null) bgSR.sortingOrder = -3;
        }

        // ── 8. GUARDAR ────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Listo",
            "Pelea contra Kirikú montada:\n\n" +
            "✓ Wally posicionado abajo (espalda)\n" +
            "✓ Kirikú posicionado arriba (frente)\n" +
            "✓ Escalas estilo Punch-Out!! aplicadas\n" +
            "✓ Sprite e animator de Kirikú conectados\n" +
            "✓ Barras de vida reconectadas\n\n" +
            "Guarda con Ctrl+S y dale Play.", "OK");
    }

    static void ConectarImagen(SerializedObject so, string propName, string goName)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop == null || prop.objectReferenceValue != null) return;
        GameObject go = GameObject.Find(goName);
        if (go == null) return;
        Image img = go.GetComponent<Image>();
        if (img != null) prop.objectReferenceValue = img;
    }
}
#endif
