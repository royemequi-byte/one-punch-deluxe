// Herramienta de editor: configura y corta automáticamente los sprite sheets de Kirikú y Wally.
// Uso: menú superior de Unity → Tools → Configurar Sprite Sheets
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class SpriteSheetImporter : EditorWindow
{
    [MenuItem("Tools/1 Configurar Sprite Sheets")]
    public static void ConfigurarTodo()
    {
        string[] carpetas = {
            "Assets/Art/Personajes/Kiriku",
            "Assets/Art/Personajes/Wally_Nuevos"
        };

        int total = 0;

        foreach (string carpeta in carpetas)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png")) continue;
                // gameover_screen es UI, no sprite sheet
                if (path.Contains("gameover_screen")) continue;

                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
                if (ti == null) continue;

                // Configuración base
                ti.textureType         = TextureImporterType.Sprite;
                ti.spriteImportMode    = SpriteImportMode.Multiple;
                ti.spritePixelsPerUnit = 200f;
                ti.filterMode          = FilterMode.Bilinear;  // ilustraciones suavizadas, no pixel art
                ti.textureCompression  = TextureImporterCompression.Uncompressed;
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled       = false;

                // Obtener dimensiones reales del archivo
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                int texW = tex ? tex.width  : 1365;
                int texH = tex ? tex.height : 512;
                int cellW = texW / 5;
                int cellH = texH;

                // Cortar en 5 celdas iguales (1 fila x 5 columnas)
                SpriteMetaData[] sprites = new SpriteMetaData[5];
                string baseName = Path.GetFileNameWithoutExtension(path);

                for (int i = 0; i < 5; i++)
                {
                    sprites[i] = new SpriteMetaData
                    {
                        name      = $"{baseName}_{i}",
                        rect      = new Rect(i * cellW, 0, cellW, cellH),
                        alignment = (int)SpriteAlignment.BottomCenter,
                        pivot     = new Vector2(0.5f, 0f)
                    };
                }

                ti.spritesheet = sprites;
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();

                Debug.Log($"Configurado: {path} ({texW}x{texH}, celda {cellW}x{cellH})");
                total++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Sprite Sheets",
            $"¡Listo! {total} sprites configurados y cortados.",
            "OK"
        );
    }
}
#endif
