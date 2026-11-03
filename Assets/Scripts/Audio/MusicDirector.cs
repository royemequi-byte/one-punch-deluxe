using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pone música según la escena, sin configurar nada: solo hay que dejar
/// menu.ogg, historia.ogg, combate.ogg y victoria.ogg en Assets/Resources/Music.
/// </summary>
public class MusicDirector : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Crear()
    {
        if (FindFirstObjectByType<MusicDirector>() != null) return;

        GameObject go = new GameObject("MusicDirector");
        DontDestroyOnLoad(go);
        go.AddComponent<MusicDirector>();
    }

    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string track = null;

        switch (scene.name)
        {
            case "menu":     track = "menu"; break;
            case "historia": track = "historia"; break;
            case "dia 1":    track = "combate"; break;
        }

        if (track == null)
        {
            AudioManager.Instance?.StopMusic();
            AudioManager.Instance?.StopAmbience();
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>("Music/" + track);
        if (clip != null) AudioManager.Instance?.PlayMusic(clip, 0.6f);
        else AudioManager.Instance?.StopMusic();

        // Multitud real de boxeo durante la pelea
        if (scene.name == "dia 1")
        {
            AudioClip crowd = Resources.Load<AudioClip>("Music/crowd");
            AudioManager.Instance?.PlayAmbience(crowd, 0.22f);
        }
        else
        {
            AudioManager.Instance?.StopAmbience();
        }
    }
}
