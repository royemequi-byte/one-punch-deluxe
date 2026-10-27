using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

/// <summary>
/// Muestra paneles de historia con efecto de máquina de escribir y paneo Ken Burns.
/// Primera tecla: completa el texto al instante. Segunda tecla: avanza al panel.
/// Al terminar todos los paneles carga la escena de pelea.
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
    [SerializeField] private float segundosPorLetra = 0.02f;

    [Header("Efecto Ken Burns")]
    [SerializeField] private float panDistance = 60f;
    [SerializeField] private float panDuration = 8f;

    // Señal que llega desde Update a la corrutina
    private bool teclaPulsada = false;
    private RectTransform imagenRect;
    private Coroutine paneoActivo; // referencia para poder detenerlo correctamente

    private void Awake()
    {
        if (imagenPanel != null)
        {
            imagenRect = imagenPanel.GetComponent<RectTransform>();
            imagenRect.localScale = new Vector3(1.12f, 1.12f, 1f);
        }
    }

    private void Start()
    {
        // Asegurar que el texto indicador muestre el mensaje correcto
        if (textoAvanzar != null)
        {
            textoAvanzar.text = "— Presiona cualquier tecla —";
            textoAvanzar.gameObject.SetActive(false);
        }

        // Música de historia
        AudioManager.Instance?.PlayMusic(AudioManager.Instance?.Library?.musicaHistoria);

        StartCoroutine(SecuenciaPaneles());
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            teclaPulsada = true;
    }

    /// <summary>
    /// Corrutina principal: recorre todos los paneles y luego carga la pelea.
    /// </summary>
    private IEnumerator SecuenciaPaneles()
    {
        for (int i = 0; i < paneles.Length; i++)
        {
            StoryPanel panel = paneles[i];

            // Mostrar imagen del panel
            if (imagenPanel != null && panel.imagen != null)
                imagenPanel.sprite = panel.imagen;

            // Ocultar texto y avanzar, iniciar paneo
            if (textoAvanzar != null) textoAvanzar.gameObject.SetActive(false);
            teclaPulsada = false;

            if (paneoActivo != null) StopCoroutine(paneoActivo);
            paneoActivo = StartCoroutine(PaneoKenBurns(i));

            // Escribir texto letra por letra
            yield return StartCoroutine(EscribirTexto(panel.texto));

            // Mostrar indicador y esperar tecla para avanzar
            if (textoAvanzar != null) textoAvanzar.gameObject.SetActive(true);
            teclaPulsada = false;
            yield return new WaitUntil(() => teclaPulsada);
            teclaPulsada = false;

            if (paneoActivo != null) { StopCoroutine(paneoActivo); paneoActivo = null; }
        }

        SceneManager.LoadScene(nombreEscenaPelea);
    }

    private IEnumerator EscribirTexto(string texto)
    {
        if (textoPanel == null) yield break;

        textoPanel.text = texto;
        textoPanel.maxVisibleCharacters = 0;
        teclaPulsada = false;

        for (int i = 0; i <= texto.Length; i++)
        {
            // Si el jugador pulsó tecla, mostrar todo de golpe y salir
            if (teclaPulsada)
            {
                textoPanel.maxVisibleCharacters = int.MaxValue;
                teclaPulsada = false;
                yield break;
            }

            textoPanel.maxVisibleCharacters = i;
            yield return new WaitForSeconds(segundosPorLetra);
        }
    }

    private IEnumerator PaneoKenBurns(int indice)
    {
        if (imagenRect == null) yield break;

        float desde = indice % 2 == 0 ? -panDistance : panDistance;
        float hasta = -desde;
        float tiempo = 0f;

        while (true)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(tiempo / panDuration, 1f));
            imagenRect.anchoredPosition = new Vector2(Mathf.Lerp(desde, hasta, t), imagenRect.anchoredPosition.y);
            yield return null;
        }
    }
}
