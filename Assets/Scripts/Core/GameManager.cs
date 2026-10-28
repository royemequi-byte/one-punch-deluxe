using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton central del juego. Gestiona el flujo entre escenas y el nivel actual.
/// No se destruye al cambiar de escena — vive toda la partida.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Datos de peleadores")]
    [SerializeField] private FighterData wallyData;
    [SerializeField] private FighterData[] rivales;   // índice = nivel (0 = rival común, 1 = Kirikú)

    [Header("Nombres de escenas")]
    [SerializeField] private string escenaMenu     = "menu";
    [SerializeField] private string escenaHistoria  = "historia";
    [SerializeField] private string escenaCombate  = "dia 1";

    // Lee siempre desde SaveManager para no quedar desactualizado entre escenas
    public int NivelActual => SaveManager.Instance != null
        ? Mathf.Clamp(SaveManager.Instance.Data.nivelActual, 0, rivales != null ? rivales.Length - 1 : 0)
        : 0;

    // Datos del rival que está a punto de pelear (los usa la escena de combate)
    public FighterData WallyData => wallyData;
    public FighterData RivalActual => (rivales != null && NivelActual < rivales.Length)
        ? rivales[NivelActual]
        : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── NAVEGACIÓN ─────────────────────────────────────────────────

    public void IrAlMenu()     => SceneManager.LoadScene(escenaMenu);
    public void IrAHistoria()  => SceneManager.LoadScene(escenaHistoria);
    public void IrACombate()   => SceneManager.LoadScene(escenaCombate);
    public void ReiniciarJuego()
    {
        SaveManager.Instance?.ReiniciarPartida(); // esto resetea nivelActual a 0 en el save
        IrAlMenu();
    }

    // NivelActual se actualiza automáticamente porque lee de SaveManager.
    // FightManager llama SaveManager.RegistrarResultado() al terminar la pelea.
}
