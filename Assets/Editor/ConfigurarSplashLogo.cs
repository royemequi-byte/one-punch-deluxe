#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Herramienta de Editor para configurar automáticamente el logo en la escena Splash.
/// Importa la imagen como Sprite, la asigna al componente Image y guarda la escena.
/// </summary>
public static class ConfigurarSplashLogo
{
    private const string RUTA_LOGO = "Assets/Art/UI/Logo_OnePunchDeluxe.png";
    private const string RUTA_ESCENA_SPLASH = "Assets/splash.unity";

    [MenuItem("OnePunch/Configurar Logo en Splash")]
    [MenuItem("Tools/Configurar Logo en Splash")]
    public static void AplicarLogo()
    {
        // 1. Asegurar que la textura esté configurada como Sprite (2D y UI)
        TextureImporter importer = AssetImporter.GetAtPath(RUTA_LOGO) as TextureImporter;
        if (importer != null)
        {
            bool necesitaReimportar = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                necesitaReimportar = true;
            }
            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                necesitaReimportar = true;
            }
            if (necesitaReimportar)
            {
                importer.SaveAndReimport();
                Debug.Log("[OnePunch] La imagen del logo fue reconfigurada con canal alfa transparente.");
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

        // 3. Forzar refresco y cargar el Sprite procesado
        AssetDatabase.ImportAsset(RUTA_LOGO, ImportAssetOptions.ForceUpdate);
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_LOGO);
        if (logoSprite == null)
        {
            Debug.LogError("[OnePunch] No se pudo cargar el Sprite del logo. Revisa la importación en Unity.");
            return;
        }

        // 4. Buscar el objeto LogoImage en la jerarquía
        GameObject logoGO = GameObject.Find("LogoImage");
        if (logoGO == null)
        {
            Debug.LogError("[OnePunch] No se encontró el GameObject 'LogoImage' en la escena Splash.");
            return;
        }

        // 5. Asignar el sprite y configurar proporciones
        Image imagen = logoGO.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.sprite = logoSprite;
            imagen.preserveAspect = true;
            imagen.color = Color.white;
        }

        // 6. Ajustar dimensiones para que se vea nítido, imponente y centrado
        RectTransform rt = logoGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(1100f, 825f);
        }

        // 7. Marcar la escena como modificada y guardarla
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        Debug.Log("[OnePunch] ¡Logo recortado configurado exitosamente en la escena Splash!");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("One Punch Deluxe", "¡El logo recortado fue asignado y centrado exitosamente en la escena Splash!", "Excelente");
        }
    }
}
#endif
