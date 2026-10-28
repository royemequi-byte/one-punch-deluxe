#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DiagEscena
{
    [MenuItem("Tools/00 Diagnosticar Objetos")]
    public static void Diagnosticar()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string info = $"Escena activa: '{scene.name}'\n\n";

        // Listar TODOS los GameObjects incluyendo inactivos
        var todos = Resources.FindObjectsOfTypeAll<GameObject>();
        info += $"Total GameObjects (incluye prefabs): {todos.Length}\n\n";
        info += "En escena:\n";

        foreach (var go in todos)
        {
            if (!go.scene.IsValid()) continue; // ignorar prefabs
            string comps = "";
            foreach (var c in go.GetComponents<Component>())
                if (c != null) comps += c.GetType().Name + " ";
            info += $"  [{(go.activeInHierarchy ? "ON" : "off")}] {go.name}  →  {comps}\n";
        }

        Debug.Log(info);
        EditorUtility.DisplayDialog("Objetos en escena", info, "OK");
    }
}
#endif
