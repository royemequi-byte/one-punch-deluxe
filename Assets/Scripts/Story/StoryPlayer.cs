using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Muestra paneles de historia en secuencia.
/// Cada panel tiene una imagen y un texto. Cualquier tecla avanza al siguiente.
/// Al terminar todos los paneles, carga la escena de pelea.
/// </summary>
public class StoryPlayer : MonoBehaviour
{
    [Header("Paneles en orden")]
    [SerializeField] private StoryPanel[] paneles;

    [Header("Referencias UI")]
    [SerializeField] private Image imagenPanel;
    [SerializeField] private TMP_Text textoPanel;
    [SerializeField] private TMP_Text textoAvanzar;   // "Presiona cualquier tecla..."

    [Header("Escena siguiente")]
    [SerializeField] private string nombreEscenaPelea = "dia 1";

    private int indiceActual = 0;

    private void Start()
    {
        MostrarPanel(0);
    }

    private void Update()
    {
        // Cualquier tecla avanza al siguiente panel
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            Avanzar();
    }

    /// <summary>
    /// Avanza al siguiente panel. Si no hay más, carga la escena de pelea.
    /// </summary>
    public void Avanzar()
    {
        indiceActual++;

        if (indiceActual >= paneles.Length)
        {
            SceneManager.LoadScene(nombreEscenaPelea);
            return;
        }

        MostrarPanel(indiceActual);
    }

    private void MostrarPanel(int indice)
    {
        if (indice >= paneles.Length) return;

        StoryPanel panel = paneles[indice];

        if (imagenPanel != null)
            imagenPanel.sprite = panel.imagen;

        if (textoPanel != null)
            textoPanel.text = panel.texto;

        // Mostrar indicador solo en el último panel para no confundir
        if (textoAvanzar != null)
            textoAvanzar.text = indice == paneles.Length - 1
                ? "Presiona cualquier tecla para pelear"
                : "Presiona cualquier tecla para continuar";
    }
}
