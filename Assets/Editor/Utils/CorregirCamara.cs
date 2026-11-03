#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class CorregirCamara : EditorWindow
{
    [MenuItem("Tools/9 Corregir Camara")]
    public static void Corregir()
    {
        // Buscar Main Camera
        GameObject camara = GameObject.Find("Main Camera");
        if (camara == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró 'Main Camera'.", "OK");
            return;
        }

        int eliminados = 0;
        int liberados = 0;

        // Revisar todos los hijos de la cámara
        foreach (Transform hijo in camara.transform.Cast<Transform>().ToArray())
        {
            SpriteRenderer sr = hijo.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            string nombre = hijo.name;

            if (nombre == "Wally")
            {
                // Sacar Wally de la cámara y ponerlo en la raíz
                hijo.SetParent(null);
                hijo.position = new Vector3(0f, -3.5f, 0.5f);
                hijo.localScale = new Vector3(2f, 2f, 1f);
                liberados++;
                Debug.Log("[CorregirCamara] Wally liberado de Main Camera");
            }
            else
            {
                // Eliminar Rival u otros sprites que no deben estar en la cámara
                Debug.Log($"[CorregirCamara] Eliminando '{nombre}' (hijo de Main Camera)");
                GameObject.DestroyImmediate(hijo.gameObject);
                eliminados++;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Corrección completa",
            $"Eliminados: {eliminados} objeto(s)\nLiberados: {liberados} objeto(s)\n\nGuarda con Ctrl+S.",
            "OK");
    }
}
#endif
