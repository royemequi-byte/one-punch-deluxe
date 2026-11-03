#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Reposiciona a Kirikú en la escena sin tocar a Wally.
/// Uso: Tools → 7 Posicionar Kiriku
/// </summary>
public class PosicionarKiriku : EditorWindow
{
    [MenuItem("Tools/20 Posicionar Kiriku")]
    public static void Posicionar()
    {
        GameObject kiriku = GameObject.Find("Kiriku");
        if (kiriku == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró 'Kiriku' en la escena.", "OK");
            return;
        }

        // Rival al frente, centrado, escala grande para que domine la pantalla
        kiriku.transform.position   = new Vector3(0f, 2f, 0f);
        kiriku.transform.localScale = new Vector3(4f, 4f, 1f);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Debug.Log("[Fix] Kirikú → posición (0, 2, 0) escala 4x");
        EditorUtility.DisplayDialog("Kirikú reposicionado",
            "Posición: (0, 2, 0)\nEscala: 4x\n\nGuarda con Ctrl+S y dale Play.\nSi queda muy arriba o muy abajo avísame.", "OK");
    }
}
#endif
