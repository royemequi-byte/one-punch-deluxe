using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pone sonido a todos los botones del juego (clic y hover) sin configurar nada.
/// Se engancha a cada escena al cargar.
/// </summary>
public static class UISoundBinder
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Iniciar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene scene, LoadSceneMode mode)
    {
        Button[] botones = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button boton in botones)
        {
            boton.onClick.AddListener(() => Click());

            EventTrigger trigger = boton.GetComponent<EventTrigger>();
            if (trigger == null) trigger = boton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entrada = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entrada.callback.AddListener(_ => Hover());
            trigger.triggers.Add(entrada);
        }
    }

    private static void Click()
    {
        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlaySFX(lib?.sfxBotonClick, 0.7f, 1f);
    }

    private static void Hover()
    {
        AudioLibrary lib = AudioManager.Instance?.Library;
        AudioManager.Instance?.PlaySFX(lib?.sfxBotonHover, 0.35f, 1f);
    }
}
