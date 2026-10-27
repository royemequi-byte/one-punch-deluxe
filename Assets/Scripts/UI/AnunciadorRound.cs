using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Muestra carteles animados estilo KOF: "ROUND 1", "FIGHT!", "K.O.", etc.
/// HUDController lo llama; los carteles aparecen sobre todo el HUD.
/// </summary>
public class AnunciadorRound : MonoBehaviour
{
    [Header("Panel y texto")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   texto;
    [SerializeField] private TMP_Text   subtexto; // opcional, para "FIGHT!" bajo "ROUND 1"

    [Header("Tiempos")]
    [SerializeField] private float tiempoVisible = 1.1f;
    [SerializeField] private float tiempoFade    = 0.22f;

    /// <summary>Duración total de la secuencia ROUND X → FIGHT! para que FightManager espere.</summary>
    public float DuracionTotal => (tiempoVisible + tiempoFade * 2f) * 2f + 0.15f;

    private Coroutine corActual;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    // ── API PÚBLICA ─────────────────────────────────────────────

    /// <summary>Secuencia completa: "ROUND X" → "FIGHT!" Llamado por HUDController.</summary>
    public void MostrarRoundYFight(int round)
    {
        Reiniciar();
        corActual = StartCoroutine(SecuenciaRound(round));
    }

    public void MostrarKO()
    {
        Reiniciar();
        corActual = StartCoroutine(Anuncio("K.O.", Color.white, tiempoVisible * 1.6f));
    }

    public void MostrarWin()
    {
        Reiniciar();
        corActual = StartCoroutine(Anuncio("YOU WIN!", new Color(1f, 0.85f, 0f), tiempoVisible * 1.6f));
    }

    public void MostrarDerrota()
    {
        Reiniciar();
        corActual = StartCoroutine(Anuncio("K.O.!", new Color(0.95f, 0.15f, 0.15f), tiempoVisible * 1.6f));
    }

    public void MostrarTime()
    {
        Reiniciar();
        corActual = StartCoroutine(Anuncio("TIME!", Color.white, tiempoVisible));
    }

    public void Ocultar()
    {
        Reiniciar();
        if (panel != null) panel.SetActive(false);
    }

    // ── PRIVADOS ─────────────────────────────────────────────────

    private void Reiniciar()
    {
        if (corActual != null) StopCoroutine(corActual);
    }

    private IEnumerator SecuenciaRound(int round)
    {
        yield return StartCoroutine(Anuncio($"ROUND  {round}", Color.white, tiempoVisible));
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(Anuncio("FIGHT!", new Color(1f, 0.85f, 0f), tiempoVisible));
    }

    private IEnumerator Anuncio(string msg, Color color, float duracion)
    {
        if (panel == null || texto == null) yield break;

        texto.text  = msg;
        texto.color = new Color(color.r, color.g, color.b, 0f);
        panel.SetActive(true);
        panel.transform.localScale = Vector3.one * 1.35f;

        // Punch-in: escala 1.35→1.0 + fade in
        float t = 0f;
        while (t < tiempoFade)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / tiempoFade);
            texto.color            = new Color(color.r, color.g, color.b, p);
            panel.transform.localScale = Vector3.one * Mathf.Lerp(1.35f, 1f, p);
            yield return null;
        }
        texto.color            = new Color(color.r, color.g, color.b, 1f);
        panel.transform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(duracion);

        // Fade out
        t = 0f;
        while (t < tiempoFade)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / tiempoFade);
            texto.color = new Color(color.r, color.g, color.b, 1f - p);
            yield return null;
        }

        panel.SetActive(false);
    }
}
