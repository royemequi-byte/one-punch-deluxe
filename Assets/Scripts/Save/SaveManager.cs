using UnityEngine;
using System.IO;

/// <summary>
/// Singleton que guarda y carga el progreso del jugador en un archivo JSON.
/// Usa Application.persistentDataPath para que funcione en cualquier PC.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Datos actuales en memoria — otros scripts leen y escriben aquí
    public SaveData Data { get; private set; }

    // Ruta del archivo en disco (se construye en Awake)
    private string rutaArchivo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        rutaArchivo = Path.Combine(Application.persistentDataPath, "save.json");
        CargarPartida();
    }

    /// <summary>
    /// Escribe los datos actuales en el archivo JSON.
    /// Llamar después de cada victoria o derrota.
    /// </summary>
    public void GuardarPartida()
    {
        string json = JsonUtility.ToJson(Data, prettyPrint: true);
        File.WriteAllText(rutaArchivo, json);
        Debug.Log($"[Guardado] Partida guardada en {rutaArchivo}");
    }

    /// <summary>
    /// Lee el archivo JSON y carga los datos en memoria.
    /// Si no existe el archivo, crea una partida nueva.
    /// </summary>
    public void CargarPartida()
    {
        if (File.Exists(rutaArchivo))
        {
            string json = File.ReadAllText(rutaArchivo);
            Data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[Guardado] Partida cargada — nivel {Data.nivelActual}");
        }
        else
        {
            Data = new SaveData();
            Debug.Log("[Guardado] No hay partida previa — iniciando nueva");
        }
    }

    /// <summary>
    /// Borra el progreso y empieza desde el principio.
    /// Llamar desde el menú si hay opción de "Nueva partida".
    /// </summary>
    public void ReiniciarPartida()
    {
        Data = new SaveData();
        GuardarPartida();
        Debug.Log("[Guardado] Partida reiniciada");
    }

    /// <summary>
    /// Registra el resultado de una pelea y avanza al siguiente nivel.
    /// </summary>
    public void RegistrarResultado(bool wallyGano)
    {
        if (wallyGano)
        {
            Data.victorias++;
            Data.nivelActual++;  // avanzar al siguiente rival
        }
        else
        {
            Data.derrotas++;
            // En derrota no avanzamos — el jugador repite el mismo rival
        }

        GuardarPartida();
    }
}
