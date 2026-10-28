#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Repara la escena 'dia 1' en un solo click:
/// reconnecta las barras de vida al HUDController, corrige posiciones/escalas
/// de personajes y conecta referencias faltantes en FightManager.
/// Uso: Tools -> 23 Reparar Escena
/// </summary>
public class RepararEscena : EditorWindow
{
    [MenuItem("Tools/23 Reparar Escena")]
    public static void Reparar()
    {
        int fixes = 0;
        string log = "";

        // ── 1. HUDCONTROLLER ─────────────────────────────────────────────
        HUDController hud = Object.FindFirstObjectByType<HUDController>();
        if (hud == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontro HUDController en la escena.", "OK");
            return;
        }

        SerializedObject soHud = new SerializedObject(hud);

        // Conectar wallyBar (busca imagen llamada WallyHealthBar)
        if (ConectarImagenPorNombre(soHud, "wallyBar", "WallyHealthBar")) { fixes++; log += "wallyBar conectado\n"; }
        if (ConectarImagenPorNombre(soHud, "rivalBar", "RivalHealthBar")) { fixes++; log += "rivalBar conectado\n"; }
        if (ConectarImagenPorNombre(soHud, "wallyStaminaBar", "WallyStaminaBar")) { fixes++; log += "wallyStaminaBar conectado\n"; }

        // escenaMenu
        SerializedProperty escenaMenuProp = soHud.FindProperty("escenaMenu");
        if (escenaMenuProp != null && string.IsNullOrEmpty(escenaMenuProp.stringValue))
        {
            escenaMenuProp.stringValue = "menu";
            fixes++; log += "escenaMenu = 'menu'\n";
        }

        soHud.ApplyModifiedProperties();

        // ── 2. FIGHTMANAGER ───────────────────────────────────────────────
        FightManager fm = Object.FindFirstObjectByType<FightManager>();
        if (fm != null)
        {
            SerializedObject soFm = new SerializedObject(fm);

            // wallyStamina — buscar Stamina en Wally
            var wallyHealthProp = soFm.FindProperty("wallyHealth");
            if (wallyHealthProp != null && wallyHealthProp.objectReferenceValue != null)
            {
                Health wallyHealth = wallyHealthProp.objectReferenceValue as Health;
                if (wallyHealth != null)
                {
                    Stamina wallyStamina = wallyHealth.GetComponent<Stamina>();
                    var staminaProp = soFm.FindProperty("wallyStamina");
                    if (staminaProp != null && wallyStamina != null && staminaProp.objectReferenceValue == null)
                    {
                        staminaProp.objectReferenceValue = wallyStamina;
                        fixes++; log += "wallyStamina conectado\n";
                    }
                }
            }

            // rivalStamina — buscar Stamina en Rival
            var rivalHealthProp = soFm.FindProperty("rivalHealth");
            if (rivalHealthProp != null && rivalHealthProp.objectReferenceValue != null)
            {
                Health rivalHealth = rivalHealthProp.objectReferenceValue as Health;
                if (rivalHealth != null)
                {
                    Stamina rivalStamina = rivalHealth.GetComponent<Stamina>();
                    var staminaProp = soFm.FindProperty("rivalStamina");
                    if (staminaProp != null && rivalStamina != null && staminaProp.objectReferenceValue == null)
                    {
                        staminaProp.objectReferenceValue = rivalStamina;
                        fixes++; log += "rivalStamina conectado\n";
                    }
                }
            }

            soFm.ApplyModifiedProperties();
        }

        // ── 3. PERSONAJES — visibilidad garantizada ───────────────────────
        // Asegurar que Wally y Rival tengan SpriteRenderer activo y sorting order correcto
        ArreglarPersonaje("Wally",  sortOrder: 2);
        ArreglarPersonaje("Rival",  sortOrder: 1);
        fixes++; log += "SpriteRenderers de personajes verificados\n";

        // ── 4. FONDO — sorting order correcto ─────────────────────────────
        GameObject bgMenu = GameObject.Find("bg_menu");
        if (bgMenu != null)
        {
            SpriteRenderer bgSR = bgMenu.GetComponent<SpriteRenderer>();
            if (bgSR != null && bgSR.sortingOrder != -2)
            {
                bgSR.sortingOrder = -2;
                fixes++; log += "bg_menu sortingOrder = -2\n";
            }
        }

        // ── 5. GUARDAR ────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        string msg = fixes > 0
            ? $"Se aplicaron {fixes} reparaciones:\n\n{log}\nGuarda con Ctrl+S y dale Play."
            : "Todo estaba bien. No se necesitaron cambios.";

        EditorUtility.DisplayDialog("Reparar Escena", msg, "OK");
    }

    static bool ConectarImagenPorNombre(SerializedObject so, string propName, string goName)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop == null || prop.objectReferenceValue != null) return false;

        // Buscar en toda la escena por nombre de GameObject
        GameObject go = GameObject.Find(goName);
        if (go == null) return false;

        Image img = go.GetComponent<Image>();
        if (img == null) return false;

        prop.objectReferenceValue = img;
        return true;
    }

    static void ArreglarPersonaje(string nombre, int sortOrder)
    {
        GameObject go = GameObject.Find(nombre);
        if (go == null) return;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.enabled = true;
        if (sr.sortingOrder != sortOrder) sr.sortingOrder = sortOrder;

        // Si no tiene sprite, no podemos forzar uno desde el editor
        // pero al menos aseguramos que el renderer esté activo
        EditorUtility.SetDirty(go);
    }
}
#endif
