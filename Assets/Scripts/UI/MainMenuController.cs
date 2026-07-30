using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el menú principal: Nueva Partida, Continuar y Salir.
/// Continuar solo está activo si existe una partida guardada con progreso.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button botonNuevaPartida;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonSalir;

    [Header("Escenas")]
    [SerializeField] private string escenaHistoria = "historia";
    [SerializeField] private string escenaPelea = "dia 1";

    private void Start()
    {
        // Continuar solo disponible si hay progreso guardado
        bool hayProgreso = SaveManager.Instance != null && SaveManager.Instance.Data.nivelActual > 0;
        if (botonContinuar != null)
            botonContinuar.interactable = hayProgreso;

        // Conectar botones
        botonNuevaPartida?.onClick.AddListener(NuevaPartida);
        botonContinuar?.onClick.AddListener(Continuar);
        botonSalir?.onClick.AddListener(Salir);

        // Reproducir música de menú
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.Library?.musicaMenu);
    }

    private void NuevaPartida()
    {
        SaveManager.Instance?.ReiniciarPartida();
        SceneManager.LoadScene(escenaHistoria);
    }

    private void Continuar()
    {
        // Ir directo a la pelea del nivel guardado
        SceneManager.LoadScene(escenaPelea);
    }

    private void Salir()
    {
        Application.Quit();
    }
}
