/// <summary>
/// Posiciones de esquive disponibles para un peleador.
/// Son posiciones corporales, no desplazamientos — el juego es estilo Punch-Out!!
/// </summary>
public enum DodgePosition
{
    Neutral,     // posición normal, puede recibir cualquier golpe
    WeaveLeft,   // inclinado a la izquierda
    WeaveRight,  // inclinado a la derecha
    Duck,        // agachado
    Backstep     // paso atrás
}
