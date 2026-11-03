using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Copia los ajustes de importación del sprite base (idle_0 de Wally) a todas
/// las hojas nuevas de Resources/Profile. Así cualquier hoja que se agregue
/// queda con el mismo PPU (300) y el mismo pivote en los pies, sin tocar los
/// .meta a mano.
/// </summary>
public static class ImportSettingsHojas
{
    private const string Modelo = "Assets/Resources/Profile/Wally/idle_0.png";

    // Solo las carpetas de personajes: el fondo del ring tiene su propio PPU
    // (100 y pivote al centro) y no debe heredar los ajustes de los peleadores.
    private static readonly string[] Carpetas =
    {
        "Assets/Resources/Profile/Wally",
        "Assets/Resources/Profile/Kiriku"
    };

    [MenuItem("OnePunch/Reimportar hojas de perfil")]
    public static void Importar()
    {
        TextureImporter modelo = AssetImporter.GetAtPath(Modelo) as TextureImporter;
        if (modelo == null)
        {
            Debug.LogError("[Import] No se encontró el sprite modelo: " + Modelo);
            return;
        }

        TextureImporterSettings ajustes = new TextureImporterSettings();
        modelo.ReadTextureSettings(ajustes);

        int ajustados = 0;

        foreach (string carpeta in Carpetas)
        {
            if (!Directory.Exists(carpeta)) continue;

            string[] archivos = Directory.GetFiles(carpeta, "*.png", SearchOption.AllDirectories);

            foreach (string archivo in archivos)
            {
                string ruta = archivo.Replace('\\', '/');
                TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (ti == null) continue;

                bool yaEsta = ti.textureType == modelo.textureType
                           && Mathf.Approximately(ti.spritePixelsPerUnit, modelo.spritePixelsPerUnit);
                if (yaEsta) continue;

                ti.SetTextureSettings(ajustes);
                ti.SaveAndReimport();
                ajustados++;

                Debug.Log("[Import] Ajustado: " + ruta +
                          " (PPU " + ti.spritePixelsPerUnit + ")");
            }
        }

        Debug.Log("[Import] Hojas ajustadas: " + ajustados);
    }
}
