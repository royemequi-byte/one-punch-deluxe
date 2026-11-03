using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Efectos visuales de impacto: screen shake, flash blanco y micro-pausa.
/// Se auto-inicializa al arrancar la escena. No requiere configuración manual.
/// </summary>
public class VisualJuice : MonoBehaviour
{
    public static VisualJuice Instance { get; private set; }

    [Header("Shake")]
    [SerializeField] private float shakeGrandeMagnitud  = 0.18f;
    [SerializeField] private float shakeGrandeDuracion  = 0.25f;
    [SerializeField] private float shakeChicoMagnitud   = 0.07f;
    [SerializeField] private float shakeChicoDuracion   = 0.12f;

    [Header("Impact Freeze")]
    [SerializeField] private float freezeDuracion = 0.05f;

    [Header("Hit Flash")]
    [SerializeField] private float flashDuracion = 0.12f;

    [Header("Zoom de impacto")]
    [SerializeField] private float zoomFuerza = 0.1f;
    [SerializeField] private float zoomDuracion = 0.12f;

    private Camera cam;
    private Vector3 camBasePos;
    private Coroutine shakeCoroutine;
    private Coroutine zoomCoroutine;
    private float baseOrthoSize = 5f;

    // Overlay de UI para el flash blanco
    private Image flashImage;

    private void Awake()
    {
        // Singleton: si ya existe una instancia válida, destruimos esta copia
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        cam = Camera.main;
        if (cam != null)
        {
            camBasePos = cam.transform.position;
            baseOrthoSize = cam.orthographicSize;
        }

        CrearFlashOverlay();
    }

    private void OnDestroy()
    {
        // Limpiar la instancia al destruirse (fin de Play mode o cambio de escena)
        if (Instance == this) Instance = null;
    }

    // ── MÉTODOS PÚBLICOS ──────────────────────────────────────

    /// <summary>Golpe que recibe Wally — shake fuerte.</summary>
    public void JugadorRecibioGolpe()
    {
        Shake(shakeGrandeMagnitud, shakeGrandeDuracion);
        StartCoroutine(ImpactFreeze());
        StartCoroutine(FlashCoroutine());
    }

    /// <summary>Golpe que recibe el rival — shake leve.</summary>
    public void RivalRecibioGolpe()
    {
        Shake(shakeChicoMagnitud, shakeChicoDuracion);
        StartCoroutine(FlashCoroutine());
    }

    /// <summary>KO — shake máximo.</summary>
    public void KnockOut()
    {
        Shake(shakeGrandeMagnitud * 2f, 0.45f);
        StartCoroutine(SlowMotion());
    }

    // Cámara lenta breve al caer: da peso al K.O. sin congelar el juego
    private IEnumerator SlowMotion()
    {
        float previous = Time.timeScale;
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.55f);
        Time.timeScale = previous <= 0f ? 1f : previous;
    }

    /// <summary>Zoom corto hacia dentro al conectar un golpe. Da sensación de impacto.</summary>
    public void ZoomPunch(float amount)
    {
        if (cam == null || !cam.orthographic) return;
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomCoroutine(amount <= 0f ? zoomFuerza : amount, zoomDuracion));
    }

    private IEnumerator ZoomCoroutine(float amount, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(elapsed / duration);
            cam.orthographicSize = baseOrthoSize - amount * k;
            yield return null;
        }
        cam.orthographicSize = baseOrthoSize;
        zoomCoroutine = null;
    }

    // ── SHAKE ─────────────────────────────────────────────────

    private void Shake(float magnitud, float duracion)
    {
        if (cam == null) return;
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(magnitud, duracion));
    }

    private IEnumerator ShakeCoroutine(float magnitud, float duracion)
    {
        float elapsed = 0f;
        while (elapsed < duracion)
        {
            elapsed += Time.unscaledDeltaTime;
            float fuerza = (1f - elapsed / duracion) * magnitud;
            cam.transform.position = camBasePos + (Vector3)Random.insideUnitCircle * fuerza;
            yield return null;
        }
        cam.transform.position = camBasePos;
        shakeCoroutine = null;
    }

    // ── IMPACT FREEZE ─────────────────────────────────────────

    private IEnumerator ImpactFreeze()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeDuracion);
        Time.timeScale = 1f;
    }

    // ── HIT FLASH ─────────────────────────────────────────────

    private void CrearFlashOverlay()
    {
        // Crear Canvas en modo ScreenSpaceOverlay para el flash
        GameObject canvasGO = new GameObject("_JuiceFlashCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        // No usamos DontDestroyOnLoad — el canvas vive con la escena de combate

        // Imagen blanca semitransparente que cubre toda la pantalla
        GameObject imgGO = new GameObject("FlashImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        flashImage = imgGO.AddComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0f);

        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // El fondo del ring vive en la escena (BackgroundRing).
    // Este script NO lo crea ni lo mueve: así el escenario no desaparece
    // ni se duplica al dar Play.

    private IEnumerator FlashCoroutine()
    {
        if (flashImage == null) yield break;

        // Flash rápido: aparece en 1 frame y desaparece en flashDuracion
        flashImage.color = new Color(1f, 1f, 1f, 0.55f);
        float elapsed = 0f;
        while (elapsed < flashDuracion)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.55f, 0f, elapsed / flashDuracion);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        flashImage.color = new Color(1f, 1f, 1f, 0f);
    }
}
