using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Carteles animados estilo KOF: "ROUND 1", "FIGHT!", "K.O.", "GAME OVER".
/// Si la escena no tiene panel asignado, se construye solo en runtime:
/// así funciona sin tocar el Canvas del juego.
/// </summary>
public class AnunciadorRound : MonoBehaviour
{
    [Header("Panel y texto")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text texto;

    [Header("Tiempos")]
    [SerializeField] private float tiempoVisible = 0.75f;
    [SerializeField] private float tiempoFade = 0.18f;

    /// <summary>Duración total de "ROUND X → FIGHT!" para que FightManager espere.</summary>
    public float DuracionTotal => (tiempoVisible + tiempoFade * 2f) * 2f + 0.15f;

    private Coroutine corActual;

    private void Awake()
    {
        if (panel == null || texto == null) ConstruirUI();
        if (panel != null) panel.SetActive(false);
    }

    // ── API PÚBLICA ─────────────────────────────────────────────

    public void MostrarRoundYFight(int round)
    {
        Reiniciar();
        corActual = StartCoroutine(SecuenciaRound(round));
    }

    /// <summary>K.O. con voz del locutor, temblor de cámara y cámara lenta.</summary>
    public void MostrarKO()
    {
        Reiniciar();
        AnnouncerVoice.Hablar(AnnouncerVoice.KO, 1f);
        if (VisualJuice.Instance != null) VisualJuice.Instance.KnockOut();
        corActual = StartCoroutine(Anuncio("K.O.", new Color(1f, 0.25f, 0.2f), tiempoVisible * 2.2f, 1.55f));
    }

    public void MostrarWin()
    {
        Reiniciar();
        corActual = StartCoroutine(Anuncio("YOU WIN!", new Color(1f, 0.85f, 0f), tiempoVisible * 1.8f));
    }

    public void MostrarDerrota()
    {
        Reiniciar();
        corActual = StartCoroutine(Anuncio("GAME OVER", new Color(0.95f, 0.15f, 0.15f), tiempoVisible * 1.8f));
    }

    public void MostrarMaximum()
    {
        Reiniciar();
        AnnouncerVoice.Hablar(AnnouncerVoice.Maximum, 0.9f);
        corActual = StartCoroutine(Anuncio("MAXIMUM!", new Color(1f, 0.85f, 0.1f), 1.0f));
    }

    public void MostrarTime()
    {
        Reiniciar();
        AnnouncerVoice.Hablar(AnnouncerVoice.TimeUp, 0.9f);
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
        AnnouncerVoice.Hablar(AnnouncerVoice.Round(round), 1f);
        yield return StartCoroutine(Anuncio($"ROUND  {round}", Color.white, tiempoVisible));
        yield return new WaitForSecondsRealtime(0.1f);
        AnnouncerVoice.Hablar(AnnouncerVoice.Fight, 1f);
        yield return StartCoroutine(Anuncio("FIGHT!", new Color(1f, 0.85f, 0f), tiempoVisible));
    }

    private IEnumerator Anuncio(string msg, Color color, float duracion, float escalaInicial = 1.35f)
    {
        if (panel == null || texto == null) yield break;

        texto.text = msg;
        texto.color = new Color(color.r, color.g, color.b, 0f);
        panel.SetActive(true);
        panel.transform.localScale = Vector3.one * escalaInicial;

        float t = 0f;
        while (t < tiempoFade)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / tiempoFade);
            texto.color = new Color(color.r, color.g, color.b, p);
            panel.transform.localScale = Vector3.one * Mathf.Lerp(escalaInicial, 1f, p);
            yield return null;
        }
        texto.color = new Color(color.r, color.g, color.b, 1f);
        panel.transform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(duracion);

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

    // Crea un Canvas propio con un texto grande centrado
    private void ConstruirUI()
    {
        GameObject canvasGO = new GameObject("AnunciadorCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textGO = new GameObject("Texto");
        textGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        tmp.fontSize = 110f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;

        panel = canvasGO;
        texto = tmp;
    }
}
