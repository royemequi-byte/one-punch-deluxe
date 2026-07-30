using UnityEngine;

/// <summary>
/// Datos de un panel de historia: imagen de fondo y texto narrado.
/// Se crea como asset y se asigna al StoryPlayer en el Inspector.
/// </summary>
[CreateAssetMenu(fileName = "NewPanel", menuName = "OnePunchDeluxe/Story Panel")]
public class StoryPanel : ScriptableObject
{
    [Header("Contenido")]
    public Sprite imagen;

    [TextArea(3, 6)]
    public string texto;
}
