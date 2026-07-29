using System;

/// <summary>
/// Datos que se guardan en disco. Una clase plana serializable por JsonUtility.
/// No hereda de MonoBehaviour — no es un componente de Unity.
/// </summary>
[Serializable]
public class SaveData
{
    // Índice del próximo rival a enfrentar: 0=Rival Común, 1=Kirikú, 2=Juego completado
    public int nivelActual = 0;

    // Historial de la partida actual
    public int victorias = 0;
    public int derrotas = 0;
}
