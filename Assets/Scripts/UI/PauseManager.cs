using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Pausa la pelea con Escape (teclado) o Start (mando).
/// Time.timeScale=0 congela física, animadores y el timer del HUD.
/// AudioListener.pause=true silencia música y SFX mientras se está en pausa.
/// FightManager habilita / deshabilita la pausa según el estado del round.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [SerializeField] private string escenaMenu = "menu";

    public static bool EnPausa { get; private set; }

    private Canvas panelPausa;
    private bool   habilitado;

    public void Habilitar()    => habilitado = true;
    public void Deshabilitar() => habilitado = false;

    private void Update()
    {
        bool pedirToggle = false;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) pedirToggle = true;
        if (PadInput.HayMando && PadInput.InicioPulsado)    pedirToggle = true;

        if (!pedirToggle)             return;
        if (GameOverScreen.IsShowing) return;

        if (EnPausa) Reanudar();
        else if (habilitado) Pausar();
    }

    private void Pausar()
    {
        EnPausa              = true;
        Time.timeScale       = 0f;
        AudioListener.pause  = true;
        if (panelPausa == null) CrearPanel();
        panelPausa.gameObject.SetActive(true);
    }

    public void Reanudar()
    {
        EnPausa              = false;
        Time.timeScale       = 1f;
        AudioListener.pause  = false;
        panelPausa?.gameObject.SetActive(false);
    }

    private void IrAlMenu()
    {
        EnPausa              = false;
        Time.timeScale       = 1f;
        AudioListener.pause  = false;
        SceneManager.LoadScene(escenaMenu);
    }

    private void OnDestroy()
    {
        // Garantía: si la escena cambia mientras está en pausa, restaurar estado global
        if (!EnPausa) return;
        Time.timeScale      = 1f;
        AudioListener.pause = false;
        EnPausa             = false;
    }

    // ── Construcción del panel ────────────────────────────────────

    private void CrearPanel()
    {
        var cGO = new GameObject("PauseCanvas");
        cGO.transform.SetParent(transform, false);
        panelPausa = cGO.AddComponent<Canvas>();
        panelPausa.renderMode    = RenderMode.ScreenSpaceOverlay;
        panelPausa.sortingOrder  = 999;

        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        cGO.AddComponent<GraphicRaycaster>();

        AgregarFondo(cGO.transform);
        AgregarTitulo(cGO.transform);
        AgregarBoton(cGO.transform, "CONTINUAR",  new Vector2(0f,  40f), Reanudar);
        AgregarBoton(cGO.transform, "IR AL MENÚ", new Vector2(0f, -80f), IrAlMenu);
    }

    private static void AgregarFondo(Transform p)
    {
        var go = new GameObject("Fondo"); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.70f);
    }

    private static void AgregarTitulo(Transform p)
    {
        var go = new GameObject("TituloPausa"); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600f, 120f);
        rt.anchoredPosition = new Vector2(0f, 180f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = "PAUSA";
        tmp.fontSize  = 90f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.raycastTarget = false;
    }

    private void AgregarBoton(Transform p, string texto, Vector2 pos, System.Action accion)
    {
        var go = new GameObject(texto); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(440f, 90f);
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = new Color(0.85f, 0.60f, 0.05f);
        btn.colors = cols;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance?.Library?.sfxBotonClick);
            accion();
        });

        var lblGO = new GameObject("Texto"); lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = texto;
        lbl.fontSize  = 36f;
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color     = Color.white;
        lbl.raycastTarget = false;
    }
}
