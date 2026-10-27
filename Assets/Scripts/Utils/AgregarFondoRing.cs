#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Agrega el fondo del ring a la escena de combate (dia 1).
/// Solo crea un objeto nuevo — no modifica nada existente.
/// </summary>
public class AgregarFondoRing : MonoBehaviour
{
    [MenuItem("Tools/01 Agregar Fondo Ring")]
    public static void Agregar()
    {
        // Verificar que estamos en la escena correcta
        string escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (escena != "dia 1")
        {
            EditorUtility.DisplayDialog("Error", $"Esta herramienta funciona solo en la escena 'dia 1'.\nEscena actual: '{escena}'", "OK");
            return;
        }

        // Verificar si ya existe un fondo
        if (GameObject.Find("BackgroundRing") != null)
        {
            EditorUtility.DisplayDialog("Info", "El fondo del ring ya existe en la escena.", "OK");
            return;
        }

        // Cargar el sprite del fondo
        Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Art/Backgrounds/Ring/ring_bg.png");

        if (ringSprite == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No se encontró el sprite en:\nAssets/Art/Backgrounds/Ring/ring_bg.png\n\n" +
                "Asegúrate de que Unity haya importado la imagen.", "OK");
            return;
        }

        // Crear el GameObject de fondo
        GameObject fondo = new GameObject("BackgroundRing");
        SpriteRenderer sr = fondo.AddComponent<SpriteRenderer>();
        sr.sprite = ringSprite;
        sr.sortingOrder = -10;  // Detrás de todo

        // Posicionar y escalar para cubrir la cámara
        // La cámara ortográfica tiene size=5 y aspect ~16:9
        fondo.transform.position = new Vector3(0f, 0f, 10f);
        fondo.transform.localScale = new Vector3(0.56f, 0.56f, 1f);

        // Marcar la escena como modificada para que Unity la guarde
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[AgregarFondo] BackgroundRing creado exitosamente.");
        EditorUtility.DisplayDialog("Éxito",
            "Fondo del ring agregado.\nGuarda la escena con Ctrl+S.", "OK");
    }
}
#endif
