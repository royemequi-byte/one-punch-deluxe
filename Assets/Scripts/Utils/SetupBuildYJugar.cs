#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

/// <summary>
/// Configura las escenas en Build Settings en el orden correcto y abre el Splash.
/// Uso: Tools → 21 Jugar Desde el Inicio
/// </summary>
public class SetupBuildYJugar : EditorWindow
{
    [MenuItem("Tools/21 Jugar Desde el Inicio")]
    public static void Ejecutar()
    {
        string[] escenas = new[]
        {
            "Assets/splash.unity",
            "Assets/menu.unity",
            "Assets/historia.unity",
            "Assets/dia 1.unity",
        };

        // Verificar que existen
        foreach (string s in escenas)
        {
            if (!System.IO.File.Exists(s))
            {
                EditorUtility.DisplayDialog("Error",
                    $"No se encontró la escena:\n{s}\n\nVerifica la ruta.", "OK");
                return;
            }
        }

        // Agregar al Build Settings en orden
        var builds = new EditorBuildSettingsScene[escenas.Length];
        for (int i = 0; i < escenas.Length; i++)
            builds[i] = new EditorBuildSettingsScene(escenas[i], true);

        EditorBuildSettings.scenes = builds;
        Debug.Log("[Setup] Build Settings configurado con 4 escenas.");

        // Guardar la escena actual y abrir el Splash
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/splash.unity");

        EditorUtility.DisplayDialog("Listo",
            "Build Settings configurado:\n" +
            "0 - splash\n1 - menu\n2 - historia\n3 - dia 1\n\n" +
            "La escena Splash está abierta.\nDale Play para jugar desde el inicio.",
            "Dale Play!");
    }
}
#endif
