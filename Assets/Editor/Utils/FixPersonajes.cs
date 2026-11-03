// Corrige el SpriteRenderer y Animator de Wally y Kirikú en la escena.
// Uso: Tools → 6 Corregir Personajes
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class FixPersonajes : EditorWindow
{
    [MenuItem("Tools/6 Corregir Personajes")]
    public static void Corregir()
    {
        int fixes = 0;

        // ── WALLY ────────────────────────────────────────────────────
        GameObject wally = GameObject.Find("Wally");
        if (wally != null)
        {
            // Sprite inicial = idle original (sprite único, no sheet)
            Sprite wallyIdleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Art/Personajes/Wally/wally_idle.png"
            );
            SpriteRenderer wallySR = wally.GetComponent<SpriteRenderer>();
            if (wallySR == null) wallySR = wally.AddComponent<SpriteRenderer>();
            if (wallyIdleSprite != null) wallySR.sprite = wallyIdleSprite;
            wallySR.sortingOrder = 2;

            // Usar el Animator original de Wally (ya tenía todas las animaciones)
            Animator wallyAnim = wally.GetComponent<Animator>();
            if (wallyAnim == null) wallyAnim = wally.AddComponent<Animator>();
            RuntimeAnimatorController wallyCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/es/Wally/Characters/Wally/WallyAnimator.controller"
            );
            if (wallyCtrl != null) wallyAnim.runtimeAnimatorController = wallyCtrl;

            // Posición abajo al centro (espalda del jugador)
            wally.transform.position   = new Vector3(0f, -1.5f, 0.5f);
            wally.transform.localScale = new Vector3(2f, 2f, 1f);

            fixes++;
            Debug.Log($"[Fix] Wally corregido — sprite: {wallyIdleSprite?.name}, controller: {wallyCtrl?.name}");
        }
        else Debug.LogWarning("[Fix] No se encontró 'Wally' en la escena.");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        AssetDatabase.Refresh();

        string log = fixes > 0
            ? $"Corregidos {fixes} personajes.\n\nGuarda con Ctrl+S."
            : "No se encontraron objetos Wally o Kiriku en la escena.";

        EditorUtility.DisplayDialog("Fix Personajes", log, "OK");
    }

    // Carga el primer frame cortado de un sprite sheet
    private static Sprite CargarFrame(string path)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        // Intentar primer sub-sprite cortado (kiriku_idle_0, wally_idle_0, etc.)
        Sprite subSprite = all.OfType<Sprite>().OrderBy(s => s.name).FirstOrDefault();
        return subSprite;
    }
}
#endif
