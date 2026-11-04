#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Herramienta de Editor para configurar el logo en la escena Splash a pantalla completa.
/// Ajusta el CanvasScaler a ScaleWithScreenSize (1920x1080) y estira la imagen
/// para que abarque toda la pantalla sin cortarse en ninguna resolución.
/// </summary>
public static class ConfigurarSplashLogo
{
    private const string RUTA_LOGO = "Assets/Art/UI/Logo_OnePunchDeluxe.png";
    private const string RUTA_ESCENA_SPLASH = "Assets/splash.unity";

    [MenuItem("OnePunch/Configurar Logo en Splash")]
    [MenuItem("Tools/Configurar Logo en Splash")]
    public static void AplicarLogo()
    {
        // 1. Asegurar que la textura esté configurada como Sprite (2D y UI) con canal alfa
        TextureImporter importer = AssetImporter.GetAtPath(RUTA_LOGO) as TextureImporter;
        if (importer != null)
        {
            bool necesitaReimportar = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                necesitaReimportar = true;
            }
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                necesitaReimportar = true;
            }
            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                necesitaReimportar = true;
            }
            if (necesitaReimportar)
            {
                importer.SaveAndReimport();
                Debug.Log("[OnePunch] Imagen configurada como Sprite UI Clamp con transparencia.");
            }
        }
        else
        {
            Debug.LogError($"[OnePunch] No se encontró el archivo del logo en: {RUTA_LOGO}");
            return;
        }

        // 2. Guardar escena actual y abrir la escena de Splash
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var escena = EditorSceneManager.OpenScene(RUTA_ESCENA_SPLASH);

        // 3. Forzar refresco y cargar el Sprite
        AssetDatabase.ImportAsset(RUTA_LOGO, ImportAssetOptions.ForceUpdate);
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_LOGO);
        if (logoSprite == null)
        {
            Debug.LogError("[OnePunch] No se pudo cargar el Sprite del logo.");
            return;
        }

        // 4. Configurar el CanvasScaler para que escale con cualquier tamaño de pantalla
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        // 5. Buscar el objeto LogoImage en la jerarquía
        GameObject logoGO = GameObject.Find("LogoImage");
        if (logoGO == null)
        {
            Debug.LogError("[OnePunch] No se encontró el GameObject 'LogoImage' en la escena Splash.");
            return;
        }

        // 6. Asignar el sprite
        Image imagen = logoGO.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.sprite = logoSprite;
            imagen.preserveAspect = true;
            imagen.color = Color.white;
        }

        // 7. Configurar anclajes a PANTALLA COMPLETA (Stretch total)
        // Se ancla de 0 a 1 en X e Y para que use el 100% de la pantalla sin cortarse nunca
        RectTransform rt = logoGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero; // Esquina inferior izquierda (0,0)
            rt.anchorMax = Vector2.one;  // Esquina superior derecha (1,1)
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero; // Sin márgenes que lo encojan
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        // 8. Marcar la escena como modificada y guardarla
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        Debug.Log("[OnePunch] ¡Logo configurado a pantalla completa sin cortes en la escena Splash!");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("One Punch Deluxe", "¡El logo fue configurado a pantalla completa sin cortes!", "Excelente");
        }
    }
}
#endif
