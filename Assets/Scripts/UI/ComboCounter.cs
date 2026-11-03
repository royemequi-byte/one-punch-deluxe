using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contador de combos tipo KOF: aparece "2 HITS!", "3 HITS!" a la izquierda
/// cuando Wally encadena golpes. Se construye solo, sin tocar el Canvas del HUD.
/// </summary>
public class ComboCounter : MonoBehaviour
{
    private CombatController player;
    private TMP_Text text;
    private RectTransform textRect;
    private int lastCount;
    private float hideTimer;
    private float punch;

    private void Awake()
    {
        ConstruirUI();
    }

    private void Update()
    {
        if (player == null)
        {
            PlayerInputHandler input = Object.FindFirstObjectByType<PlayerInputHandler>();
            if (input != null) player = input.GetComponent<CombatController>();
        }

        if (player == null || text == null) return;

        int count = player.ComboHits;

        if (count >= 2)
        {
            if (count != lastCount)
            {
                text.text = count + " HITS!";
                text.gameObject.SetActive(true);
                punch = 1f;
            }
            lastCount = count;
            hideTimer = 0.75f;
        }
        else if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                text.gameObject.SetActive(false);
                lastCount = 0;
            }
        }

        // Pequeño golpe de escala al aparecer
        punch = Mathf.MoveTowards(punch, 0f, Time.unscaledDeltaTime * 7f);
        if (text.gameObject.activeSelf)
            textRect.localScale = Vector3.one * Mathf.Lerp(1f, 1.3f, punch);
    }

    private void ConstruirUI()
    {
        GameObject canvasGO = new GameObject("ComboCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 350;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textGO = new GameObject("ComboTexto");
        textGO.transform.SetParent(canvasGO.transform, false);

        textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.19f, 0.57f);
        textRect.anchorMax = new Vector2(0.55f, 0.66f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        tmp.fontSize = 58f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(1f, 0.88f, 0.15f);
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;
        tmp.raycastTarget = false;

        text = tmp;
        textGO.SetActive(false);
    }
}
