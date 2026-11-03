using UnityEngine;

/// <summary>
/// Crea el AudioManager automáticamente al arrancar el juego, en cualquier escena.
/// Carga la librería desde Resources y la deja viva con DontDestroyOnLoad.
/// </summary>
public static class GameAudioBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CrearManager()
    {
        if (AudioManager.Instance != null) return;

        AudioLibrary library = Resources.Load<AudioLibrary>("AudioLibrary");
        if (library == null)
        {
            Debug.LogWarning("[Audio] No se encontró Resources/AudioLibrary. El juego quedará sin sonido.");
        }

        GameObject go = new GameObject("AudioManager");
        AudioManager manager = go.AddComponent<AudioManager>();
        manager.Initialize(library);
    }
}
