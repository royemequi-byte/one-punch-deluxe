using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Compila el juego para Windows. Menú: OnePunch → Construir Juego.
/// También se puede llamar en batch mode con -executeMethod BuildGame.BuildWindows.
/// </summary>
public static class BuildGame
{
    private static readonly string[] Scenes =
    {
        "Assets/menu.unity",
        "Assets/historia.unity",
        "Assets/dia 1.unity"
    };

    [MenuItem("OnePunch/Construir Juego (Windows)")]
    public static void BuildWindows()
    {
        // Antes de compilar, cualquier hoja nueva de Resources/Profile hereda
        // el PPU y el pivote de los pies del sprite base.
        ImportSettingsHojas.Importar();

        const string output = "Builds/OnePunchDeluxe/OnePunchDeluxe.exe";

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        Debug.Log($"[Build] Resultado: {report.summary.result} | Errores: {report.summary.totalErrors} | Salida: {output}");

        if (report.summary.result != BuildResult.Succeeded && Application.isBatchMode)
            EditorApplication.Exit(1);
    }
}
