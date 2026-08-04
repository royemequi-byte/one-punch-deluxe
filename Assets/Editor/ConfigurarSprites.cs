using UnityEngine;
using UnityEditor;

/// <summary>
/// Configura todos los sprites de Art/Personajes con los ajustes correctos.
/// Ejecutar desde el menu: Tools → Configurar Sprites del Proyecto
/// </summary>
public class ConfigurarSprites
{
    [MenuItem("Tools/Configurar Sprites del Proyecto")]
    public static void Configurar()
    {
        string[] carpetas = new[]
        {
            "Assets/Art/Personajes/Wally",
            "Assets/Art/Personajes/Rival",
            "Assets/Art/Personajes/UI"
        };

        int total = 0;

        foreach (string carpeta in carpetas)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta });

            foreach (string guid in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;

                if (importer == null) continue;

                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.alphaSource         = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.filterMode          = FilterMode.Bilinear;
                importer.mipmapEnabled       = false;

                TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
                settings.maxTextureSize  = 2048;
                settings.format          = TextureImporterFormat.Automatic;
                settings.compressionQuality = 100;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);

                importer.SaveAndReimport();
                total++;
                Debug.Log($"Configurado: {ruta}");
            }
        }

        Debug.Log($"Listo. {total} sprites configurados.");
        EditorUtility.DisplayDialog("Sprites configurados", $"{total} sprites importados correctamente.", "OK");
    }
}
