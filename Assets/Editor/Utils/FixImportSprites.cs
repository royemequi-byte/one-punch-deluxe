#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Reimporta todos los sprites de Wally y Kirikú como Multiple (spritesheet)
/// con la configuración correcta: PPU, pivote bottom-center, sin compresión.
/// Hay que correr esto ANTES de Fix Ataques para que los frames existan.
/// Uso: Tools → 19 Fix Import Sprites
/// </summary>
public class FixImportSprites : EditorWindow
{
    [MenuItem("Tools/19 Fix Import Sprites")]
    public static void Fix()
    {
        int total = 0;

        // Wally: frames de ~420px de ancho — siempre 5 por fila, PPU=100
        total += ImportarCarpeta(
            "Assets/Art/Personajes/Wally",
            ppu: 100f,
            columnas: 5,
            excluir: null);

        // Kirikú: frames de ~254px de ancho — siempre 5 por fila, PPU=200
        total += ImportarCarpeta(
            "Assets/Art/Personajes/Kiriku",
            ppu: 200f,
            columnas: 5,
            excluir: null);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Sprites reimportados",
            $"Total reimportados: {total}\n\n" +
            "Ahora corre:\n" +
            "  17 Fix Ataques  →  asigna los frames a los clips\n" +
            "  18 Fix Datos    →  ajusta los timings\n\n" +
            "Ctrl+S → Play.", "OK");
    }

    private static int ImportarCarpeta(string carpeta, float ppu, int columnas, string excluir)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png")) continue;
            if (excluir != null && path.Contains(excluir)) continue;

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            // Leer dimensiones reales cargando la textura
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue;
            int texW = tex.width;
            int texH = tex.height;

            // Calcular tamaño de celda — dividir en `columnas` columnas iguales
            int cellW = texW / columnas;
            int cellH = texH;

            // Configuración base del importer
            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode          = FilterMode.Point;     // sin blur en pixel art
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;

            // Construir las celdas manualmente
            string baseName = Path.GetFileNameWithoutExtension(path);
            SpriteMetaData[] sprites = new SpriteMetaData[columnas];
            for (int i = 0; i < columnas; i++)
            {
                sprites[i] = new SpriteMetaData
                {
                    name      = $"{baseName}_{i}",
                    // Unity coord: y=0 es abajo, pero rect se cuenta desde abajo
                    rect      = new Rect(i * cellW, 0, cellW, cellH),
                    alignment = (int)SpriteAlignment.BottomCenter,
                    pivot     = new Vector2(0.5f, 0f)
                };
            }

            ti.spritesheet = sprites;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();

            Debug.Log($"[FixImport] {Path.GetFileName(path)}: {texW}x{texH} → {columnas} frames de {cellW}x{cellH}px  PPU={ppu}");
            count++;
        }

        return count;
    }
}
#endif
