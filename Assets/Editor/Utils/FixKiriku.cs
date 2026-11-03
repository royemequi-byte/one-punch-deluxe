#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Crea el TelegraphIndicator de Kirikú y ajusta su posición/escala en escena.
/// Uso: Tools → 12 Fix Kiriku
/// </summary>
public class FixKiriku : EditorWindow
{
    [MenuItem("Tools/12 Fix Kiriku")]
    public static void Fix()
    {
        GameObject kiriku = GameObject.Find("Kiriku");
        if (kiriku == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró 'Kiriku' en la escena.", "OK");
            return;
        }

        // ── 1. Posición y escala estilo Punch-Out!! ──────────────────
        kiriku.transform.position   = new Vector3(-0.7f, -3.8f, 0f);
        kiriku.transform.localScale = new Vector3(4.8f, 4.8f, 1f);

        // SpriteRenderer — asegurarnos de que tenga sorting order correcto
        SpriteRenderer sr = kiriku.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 1;

        // ── 2. Telegraph Indicator ("!!") en raíz — sin heredar escala ──
        // Buscar si ya existe en raíz o como hijo
        GameObject telegraph = GameObject.Find("TelegraphIndicator");
        if (telegraph == null)
        {
            Transform existente = kiriku.transform.Find("TelegraphIndicator");
            if (existente != null) telegraph = existente.gameObject;
        }
        if (telegraph != null) GameObject.DestroyImmediate(telegraph);

        telegraph = new GameObject("TelegraphIndicator");
        // EN LA RAÍZ — no hijo de Kiriku, así no hereda la escala (4.8)
        telegraph.transform.SetParent(null);

        // Posición encima de la cabeza visible de Kirikú (aprox. y=2.5 pantalla)
        telegraph.transform.position  = new Vector3(-0.7f, 2.5f, -0.1f);
        telegraph.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        TextMeshPro tmp = telegraph.AddComponent<TextMeshPro>();
        tmp.text      = "!!";
        tmp.fontSize  = 5f;
        tmp.color     = new Color(1f, 0.85f, 0f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform rt = telegraph.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(3f, 2f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
        }

        telegraph.SetActive(false);

        // ── 3. Conectar TelegraphIndicator al RivalAI ────────────────
        RivalAI ai = kiriku.GetComponent<RivalAI>();
        if (ai != null)
        {
            SerializedObject so = new SerializedObject(ai);
            so.FindProperty("telegraphIndicator").objectReferenceValue = telegraph;
            so.ApplyModifiedProperties();
            Debug.Log("[FixKiriku] TelegraphIndicator conectado al RivalAI");
        }

        // ── 4. Ajustar Wally también (perspectiva Punch-Out!!) ───────
        GameObject wally = GameObject.Find("Wally");
        if (wally != null)
        {
            wally.transform.position   = new Vector3(0.8f, -6.2f, 0.5f);
            wally.transform.localScale = new Vector3(1.55f, 1.55f, 1f);
            SpriteRenderer wallySR = wally.GetComponent<SpriteRenderer>();
            if (wallySR != null) wallySR.sortingOrder = 2;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Kirikú listo",
            "✓ Posición y escala ajustadas\n" +
            "✓ TelegraphIndicator ('!!') creado encima de su cabeza\n" +
            "✓ Wally reposicionado\n\n" +
            "Guarda con Ctrl+S y dale Play.\n" +
            "Cuando Kirikú vaya a atacar verás '!!' amarillo.", "OK");
    }
}
#endif
