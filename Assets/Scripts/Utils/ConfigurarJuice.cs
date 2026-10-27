#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Agrega VisualJuice e IdleBob a los personajes de la escena de combate.
/// Ejecutar una sola vez después de abrir dia 1.
/// </summary>
public class ConfigurarJuice : MonoBehaviour
{
    [MenuItem("Tools/02 Configurar Efectos Visuales")]
    public static void Configurar()
    {
        string escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (escena != "dia 1")
        {
            EditorUtility.DisplayDialog("Error",
                $"Esta herramienta funciona solo en 'dia 1'.\nEscena actual: '{escena}'", "OK");
            return;
        }

        int cambios = 0;

        // 1. Agregar VisualJuice al FightManager (se auto-inicializa)
        GameObject fmGO = GameObject.Find("FightManager");
        if (fmGO != null && fmGO.GetComponent<VisualJuice>() == null)
        {
            fmGO.AddComponent<VisualJuice>();
            Debug.Log("[Juice] VisualJuice agregado a FightManager");
            cambios++;
        }

        // 2. Agregar IdleBob a Wally
        GameObject wally = GameObject.Find("Wally");
        if (wally != null && wally.GetComponent<IdleBob>() == null)
        {
            IdleBob bob = wally.AddComponent<IdleBob>();
            // Usar reflexión para setear faseOffset a 0 (Wally)
            // (los valores default del script ya son correctos)
            Debug.Log("[Juice] IdleBob agregado a Wally");
            cambios++;
        }

        // 3. Agregar IdleBob al Rival (con fase offset para que no sean idénticos)
        GameObject rival = GameObject.Find("Rival");
        if (rival != null && rival.GetComponent<IdleBob>() == null)
        {
            IdleBob bob = rival.AddComponent<IdleBob>();
            // Serializar con offset de fase diferente
            SerializedObject so = new SerializedObject(bob);
            so.FindProperty("faseOffset").floatValue = 1.2f;
            so.ApplyModifiedProperties();
            Debug.Log("[Juice] IdleBob agregado a Rival");
            cambios++;
        }

        // 4. Marcar esJugador=true en el Health de Wally
        if (wally != null)
        {
            Health wallyHealth = wally.GetComponent<Health>();
            if (wallyHealth != null)
            {
                SerializedObject so = new SerializedObject(wallyHealth);
                so.FindProperty("esJugador").boolValue = true;
                so.ApplyModifiedProperties();
                Debug.Log("[Juice] esJugador=true en Health de Wally");
                cambios++;
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Éxito",
            $"Efectos visuales configurados ({cambios} cambios).\nGuarda la escena con Ctrl+S.", "OK");
    }
}
#endif
