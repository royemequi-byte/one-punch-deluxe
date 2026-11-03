using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Atajos de arranque para probar rápido. No afecta al juego normal:
///   OnePunchDeluxe.exe            → menú (lo normal)
///   OnePunchDeluxe.exe -pelea     → entra directo a la pelea
///   OnePunchDeluxe.exe -diag      → abre el panel de diagnóstico del mando
/// </summary>
public static class ArranqueDebug
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RevisarArgumentos()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLower();

            if (arg == "-pelea" || arg == "--pelea")
            {
                Debug.Log("[Arranque] Entrando directo a la pelea");
                SceneManager.LoadScene("dia 1");
                return;
            }
        }
    }
}
