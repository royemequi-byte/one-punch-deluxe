using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

/// <summary>
/// Muestra paneles de historia con efecto de máquina de escribir y paneo Ken Burns.
/// La imagen se mueve suavemente de un lado al otro dando sensación de profundidad.
/// Primera tecla: completa el texto. Segunda tecla: avanza al siguiente panel.
/// </summary>
public class StoryPlayer : MonoBehaviour
{
    [Header("Paneles en orden")]
    [SerializeField] private StoryPanel[] paneles;

    [Header("Referencias UI")]
    [SerializeField] private Image imagenPanel;
    [SerializeField] private TMP_Text textoPanel;
    [SerializeField] private TMP_Text textoAvanzar;

    [Header("Escena siguiente")]
    [SerializeField] private string nombreEscenaPelea = "dia 1";

    [Header("Velocidad de escritura")]
    [SerializeField] private float segundosPorLetra = 0.04f;

    [Header("Efecto Ken Burns")]
    [SerializeField] private float panDistance = 60f;   // píxeles que se desplaza la imagen
    [SerializeField] private float panDuration = 8f;    // segundos de un lado al otro

    private int indiceActual = 0;
    private bool escribiendo = false;
    private bool textoCompleto = false;
    private RectTransform imagenRect;

    private void Start()
    {
        if (imagenPanel != null)
        {
            imagenRect = imagenPanel.GetComponent<RectTransform>();
            // Escalar ligeramente para que el paneo no muestre bordes
            imagenRect.localScale = new Vector3(1.12f, 1.12f, 1f);
        }

        if (textoAvanzar != null) textoAvanzar.gameObject.SetActive(false);
        MostrarPanel(0);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.anyKey.wasPressedThisFrame) return;

        if (escribiendo)
            CompletarTexto();
        else if (textoCompleto)
            Avanzar();
    }

    private void MostrarPanel(int indice)
    {
        if (indice >= paneles.Length) return;

        StoryPanel panel = paneles[indice];

        if (imagenPanel != null && panel.imagen != null)
            imagenPanel.sprite = panel.imagen;

        if (textoAvanzar != null) textoAvanzar.gameObject.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(EscribirTexto(panel.texto));
        StartCoroutine(PaneoKenBurns(indice));
    }

    /// <summary>
    /// Mueve la imagen de izquierda a derecha (o al revés) alternando por panel.
    /// </summary>
    private IEnumerator PaneoKenBurns(int indice)
    {
        if (imagenRect == null) yield break;

        // Alternar dirección según el número de panel
        float desde = indice % 2 == 0 ? -panDistance : panDistance;
        float hasta = -desde;

        float tiempo = 0f;
        while (true)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(tiempo / panDuration, 1f));
            float x = Mathf.Lerp(desde, hasta, t);
            imagenRect.anchoredPosition = new Vector2(x, imagenRect.anchoredPosition.y);
            yield return null;
        }
    }

    private IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        textoCompleto = false;

        if (textoPanel == null) yield break;

        textoPanel.text = texto;
        textoPanel.maxVisibleCharacters = 0;

        for (int i = 0; i <= texto.Length; i++)
        {
            textoPanel.maxVisibleCharacters = i;
            yield return new WaitForSeconds(segundosPorLetra);
            if (!escribiendo) yield break;
        }

        TerminarEscritura();
    }

    private void CompletarTexto()
    {
        escribiendo = false;
        StopCoroutine(nameof(EscribirTexto));

        if (textoPanel != null)
            textoPanel.maxVisibleCharacters = int.MaxValue;

        TerminarEscritura();
    }

    private void TerminarEscritura()
    {
        escribiendo = false;
        textoCompleto = true;
        if (textoAvanzar != null) textoAvanzar.gameObject.SetActive(true);
    }

    private void Avanzar()
    {
        indiceActual++;
        textoCompleto = false;

        if (indiceActual >= paneles.Length)
        {
            SceneManager.LoadScene(nombreEscenaPelea);
            return;
        }

        MostrarPanel(indiceActual);
    }
}
