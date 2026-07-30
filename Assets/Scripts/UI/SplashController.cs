using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Muestra el logo del juego con fade in/out y carga el menú principal.
/// </summary>
public class SplashController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Configuración")]
    [SerializeField] private float tiempoFadeIn = 1f;
    [SerializeField] private float tiempoVisible = 2.5f;
    [SerializeField] private float tiempoFadeOut = 1f;
    [SerializeField] private string escenaMenu = "menu";

    private void Start()
    {
        StartCoroutine(SecuenciaSplash());
    }

    private IEnumerator SecuenciaSplash()
    {
        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, tiempoFadeIn));

        // Esperar visible
        yield return new WaitForSeconds(tiempoVisible);

        // Fade out
        yield return StartCoroutine(Fade(1f, 0f, tiempoFadeOut));

        SceneManager.LoadScene(escenaMenu);
    }

    private IEnumerator Fade(float desde, float hasta, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(desde, hasta, tiempo / duracion);
            yield return null;
        }
        canvasGroup.alpha = hasta;
    }
}
