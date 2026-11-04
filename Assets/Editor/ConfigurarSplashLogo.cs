#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configura el logo en la escena Splash con tamaño proporcionado cinematográfico.
/// Utiliza CanvasScaler (1920x1080) y fija un tamaño de 1150x860 centrado para que
/// el logo luzca imponente pero con margen de respiro en los bordes, sin tocar nunca los extremos.
/// </summary>
public static class ConfigurarSplashLogo
{
    private const string RUTA_LOGO = "Assets/Art/UI/Logo_OnePunchDeluxe.png";
    private const string RUTA_ESCENA_SPLASH = "Assets/splash.unity";

    [MenuItem("OnePunch/Configurar Logo en Splash")]
    [MenuItem("Tools/Configurar Logo en Splash")]
    public static void AplicarLogo()
    {
        // 1. Asegurar que la textura esté como Sprite UI con alfa
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

        // 2. Guardar escena actual y abrir Splash
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

        // 4. Configurar CanvasScaler con resolución de referencia 1920x1080
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

        // 5. Buscar LogoImage
        GameObject logoGO = GameObject.Find("LogoImage");
        if (logoGO == null)
        {
            Debug.LogError("[OnePunch] No se encontró el GameObject 'LogoImage' en la escena Splash.");
            return;
        }

        // 6. Asignar el sprite y color
        Image imagen = logoGO.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.sprite = logoSprite;
            imagen.preserveAspect = true;
            imagen.color = Color.white;
        }

        // 7. Centrado perfecto con ratio 4:3 exacto (1152x864 = 4:3, misma proporción que el PNG 1024x768)
        // Deja ~108px de margen arriba/abajo y ~384px a los lados en base 1920x1080
        RectTransform rt = logoGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(1152f, 864f);
        }

        // 8. Marcar la escena como modificada y guardarla
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        Debug.Log("[OnePunch] ¡Logo centrado y proporcionado exitosamente en la escena Splash!");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("One Punch Deluxe", "¡El logo fue ajustado con márgenes limpios!", "Excelente");
        }
    }
}
#endif
