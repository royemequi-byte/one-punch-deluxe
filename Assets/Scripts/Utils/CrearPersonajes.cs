// Crea los GameObjects de Wally y Kiriku en la escena de combate con todos sus componentes.
// Uso: Tools → 5 Crear Personajes en Escena
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CrearPersonajes : EditorWindow
{
    [MenuItem("Tools/5 Crear Personajes en Escena")]
    public static void Crear()
    {
        // ── KIRIKÚ (rival) ───────────────────────────────────────────
        GameObject kiriku = new GameObject("Kiriku");
        kiriku.transform.position = new Vector3(0f, 1.5f, 0f);
        kiriku.transform.localScale = new Vector3(4f, 4f, 1f);

        SpriteRenderer kirikuSR = kiriku.AddComponent<SpriteRenderer>();
        kirikuSR.sortingOrder = 1;

        // Asignar sprite idle como imagen inicial
        Sprite kirikuSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Art/Personajes/Kiriku/kiriku_idle.png"
        );
        // Intentar cargar el primer frame cortado
        Object[] kirikuAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Personajes/Kiriku/kiriku_idle.png");
        foreach (Object a in kirikuAssets)
        {
            if (a is Sprite s && s.name.EndsWith("_0")) { kirikuSR.sprite = s; break; }
        }

        Animator kirikuAnim = kiriku.AddComponent<Animator>();
        RuntimeAnimatorController kirikuController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animations/Kiriku/KirikuAnimator.controller"
        );
        if (kirikuController != null) kirikuAnim.runtimeAnimatorController = kirikuController;

        kiriku.AddComponent<Health>();
        kiriku.AddComponent<Stamina>();
        CombatController kirikuCombat = kiriku.AddComponent<CombatController>();
        RivalAI kirikuAI = kiriku.AddComponent<RivalAI>();

        // Indicador de telegraph (!) — objeto hijo simple
        GameObject telegraph = new GameObject("TelegraphIndicator");
        telegraph.transform.SetParent(kiriku.transform);
        telegraph.transform.localPosition = new Vector3(0.3f, 0.7f, 0f);
        telegraph.transform.localScale    = new Vector3(0.25f, 0.25f, 1f);
        telegraph.SetActive(false);

        // ── WALLY (jugador) ──────────────────────────────────────────
        GameObject wally = new GameObject("Wally");
        wally.transform.position = new Vector3(0f, -2f, 0.1f);
        wally.transform.localScale = new Vector3(3f, 3f, 1f);

        SpriteRenderer wallySR = wally.AddComponent<SpriteRenderer>();
        wallySR.sortingOrder = 2;

        Object[] wallyAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Personajes/Wally_Nuevos/wally_idle.png");
        foreach (Object a in wallyAssets)
        {
            if (a is Sprite s && s.name.EndsWith("_0")) { wallySR.sprite = s; break; }
        }

        Animator wallyAnim = wally.AddComponent<Animator>();
        RuntimeAnimatorController wallyController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animations/Wally/WallyNuevosAnimator.controller"
        );
        if (wallyController != null) wallyAnim.runtimeAnimatorController = wallyController;

        wally.AddComponent<Health>();
        wally.AddComponent<Stamina>();
        wally.AddComponent<CombatController>();
        wally.AddComponent<PlayerInputHandler>();

        // ── Marcar la escena como modificada ────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        // Seleccionar Kirikú para verlo en la escena
        Selection.activeGameObject = kiriku;

        EditorUtility.DisplayDialog(
            "Personajes creados",
            "Wally y Kirikú fueron agregados a la escena.\n\n" +
            "IMPORTANTE: Guarda la escena con Ctrl+S.\n\n" +
            "Después conecta las referencias en FightManager:\n" +
            "- Arrastra Wally y Kiriku a los campos del Inspector.",
            "OK"
        );
    }
}
#endif
