using UnityEditor;
using UnityEditor.SceneManagement;

public static class AbrirEscenas
{
    [MenuItem("Tools/00 Abrir Dia 1 (Pelea)")]
    static void AbrirDia1()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene("Assets/dia 1.unity");
    }

    [MenuItem("Tools/00 Abrir Menu")]
    static void AbrirMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene("Assets/menu.unity");
    }
}
