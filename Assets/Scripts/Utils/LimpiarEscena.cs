// Elimina duplicados de Wally y Kiriku, deja solo uno de cada uno bien configurado.
// Uso: Tools → 7 Limpiar Escena
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LimpiarEscena : EditorWindow
{
    [MenuItem("Tools/7 Limpiar Escena")]
    public static void Limpiar()
    {
        // Borrar TODOS los Kiriku existentes
        foreach (GameObject g in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g.name == "Kiriku").ToArray())
        {
            Debug.Log($"[Limpieza] Borrando: {g.name}");
            GameObject.DestroyImmediate(g);
        }

        // Crear Kiriku nuevo y correcto
        GameObject kiriku = new GameObject("Kiriku");
        kiriku.transform.position   = new Vector3(0f, 0.5f, 0f);
        kiriku.transform.localScale = new Vector3(3f, 3f, 1f);

        SpriteRenderer sr = kiriku.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Personajes/Kiriku/kiriku_idle.png");
        Sprite sp = sprites.OfType<Sprite>().OrderBy(s => s.name).FirstOrDefault();
        if (sp != null) sr.sprite = sp;

        Animator anim = kiriku.AddComponent<Animator>();
        RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animations/Kiriku/KirikuAnimator.controller");
        if (ctrl != null) anim.runtimeAnimatorController = ctrl;

        kiriku.AddComponent<Health>();
        kiriku.AddComponent<Stamina>();
        kiriku.AddComponent<CombatController>();
        kiriku.AddComponent<RivalAI>();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = kiriku;

        EditorUtility.DisplayDialog("Kiriku listo",
            "Kiriku reemplazado con las ilustraciones correctas.\n\nGuarda con Ctrl+S.", "OK");
    }

    private static int LimpiarDuplicados(string nombre)
    {
        GameObject[] todos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g.name == nombre)
            .ToArray();

        if (todos.Length <= 1) return 0;

        // Para Kiriku: quedarse con el que usa KirikuAnimator (nuestro nuevo)
        // Para Wally: quedarse con el que usa WallyAnimator original
        string controllerBuscado = nombre == "Kiriku"
            ? "KirikuAnimator"
            : "WallyAnimator";

        GameObject keeper = todos.FirstOrDefault(g =>
        {
            Animator a = g.GetComponent<Animator>();
            return a != null && a.runtimeAnimatorController != null &&
                   a.runtimeAnimatorController.name.Contains(controllerBuscado);
        });

        // Si ninguno tiene el controller correcto, asignar el correcto al primero
        if (keeper == null)
        {
            keeper = todos[0];
            Animator anim = keeper.GetComponent<Animator>();
            if (anim == null) anim = keeper.AddComponent<Animator>();

            string ctrlPath = nombre == "Kiriku"
                ? "Assets/Animations/Kiriku/KirikuAnimator.controller"
                : "Assets/es/Wally/Characters/Wally/WallyAnimator.controller";

            RuntimeAnimatorController ctrl =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;

            // Asignar sprite correcto
            string spritePath = nombre == "Kiriku"
                ? "Assets/Art/Personajes/Kiriku/kiriku_idle.png"
                : "Assets/Art/Personajes/Wally/wally_idle.png";

            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            Sprite sp = sprites.OfType<Sprite>().OrderBy(s => s.name).FirstOrDefault();
            SpriteRenderer sr = keeper.GetComponent<SpriteRenderer>();
            if (sr == null) sr = keeper.AddComponent<SpriteRenderer>();
            if (sp != null) sr.sprite = sp;
        }

        int eliminados = 0;
        foreach (GameObject g in todos)
        {
            if (g == keeper) continue;
            Debug.Log($"[Limpieza] Eliminando: {g.name}");
            GameObject.DestroyImmediate(g);
            eliminados++;
        }
        return eliminados;
    }
}
#endif
