#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;

public class DiagnosticarEscena : EditorWindow
{
    [MenuItem("Tools/00 Abrir Escena Combate")]
    public static void AbrirCombate()
    {
        EditorSceneManager.OpenScene("Assets/dia 1.unity");
    }

    [MenuItem("Tools/8 Diagnosticar Escena")]
    public static void Diagnosticar()
    {
        var todos = GameObject.FindObjectsByType<SpriteRenderer>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        var sb = new StringBuilder();
        sb.AppendLine($"=== SPRITE RENDERERS EN ESCENA ({todos.Length}) ===\n");

        foreach (var sr in todos)
        {
            string path = GetPath(sr.gameObject);
            string sprite = sr.sprite != null ? sr.sprite.name : "NULL";
            string activo = sr.gameObject.activeInHierarchy ? "ON" : "OFF";
            sb.AppendLine($"[{activo}] {path}");
            sb.AppendLine($"       Sprite: {sprite}  |  Order: {sr.sortingOrder}  |  Layer: {sr.sortingLayerName}");
        }

        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Diagnóstico", sb.ToString(), "OK");
    }

    private static string GetPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
#endif
