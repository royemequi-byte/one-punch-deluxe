using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Recoloca el HUD al estilo KOF XV: barras finas arriba, 99 al centro,
/// etiquetas PLAYER 1/2, GUARD y medidor MAXIMUM abajo con 5 segmentos.
/// No crea barras nuevas: reutiliza las del HUD y solo las viste.
/// </summary>
public class KofHudSkin : MonoBehaviour
{
    private Image wallyBar;
    private Image rivalBar;
    private Image staminaBar;
    private TMP_Text timer;
    private Stamina staminaSource;
    private Stamina rivalStaminaSource;
    private PowerMeter wallyPower;
    private PowerMeter rivalPower;

    private readonly Image[] leftSegments = new Image[5];
    private readonly Image[] rightSegments = new Image[5];
    private TMP_Text leftMaximum;
    private TMP_Text rightMaximum;
    private Image leftGlow;
    private Image rightGlow;

    private static readonly Color SegmentOn = new Color(1f, 0.82f, 0.15f);
    private static readonly Color SegmentOff = new Color(0.16f, 0.16f, 0.16f);
    private static readonly Color FrameColor = new Color(0.03f, 0.03f, 0.03f, 0.9f);

    /// <summary>HUDController llama esto al terminar su Awake.</summary>
    public void Init(Image wally, Image rival, Image stamina, TMP_Text timerText,
                     Stamina staminaRef, Stamina rivalStaminaRef,
                     Image[] wallyGemas, Image[] rivalGemas)
    {
        wallyBar = wally;
        rivalBar = rival;
        staminaBar = stamina;
        timer = timerText;
        staminaSource = staminaRef;
        rivalStaminaSource = rivalStaminaRef;

        if (wallyBar == null) return;
        Canvas canvas = wallyBar.canvas;
        if (canvas == null) return;
        Transform parent = canvas.transform;

        LayoutBars();
        CreateFrame(wallyBar);
        CreateFrame(rivalBar);
        if (staminaBar != null) CreateFrame(staminaBar);
        CreateTimerBox();
        CreateLabels(parent);
        BuildGauge(parent, true);
        BuildGauge(parent, false);
        PlaceGems(wallyGemas, true);
        PlaceGems(rivalGemas, false);
    }

    private void LayoutBars()
    {
        SetAnchors(wallyBar, new Vector2(0.115f, 0.905f), new Vector2(0.455f, 0.945f));
        SetAnchors(rivalBar, new Vector2(0.545f, 0.905f), new Vector2(0.885f, 0.945f));

        if (wallyBar != null)
        {
            wallyBar.type = Image.Type.Filled;
            wallyBar.fillMethod = Image.FillMethod.Horizontal;
            wallyBar.fillOrigin = 0;
            wallyBar.color = new Color(0.10f, 0.85f, 0.25f);
        }
        if (rivalBar != null)
        {
            rivalBar.type = Image.Type.Filled;
            rivalBar.fillMethod = Image.FillMethod.Horizontal;
            rivalBar.fillOrigin = 1;
            rivalBar.color = new Color(0.10f, 0.85f, 0.25f);
        }
        if (staminaBar != null)
        {
            SetAnchors(staminaBar, new Vector2(0.115f, 0.884f), new Vector2(0.335f, 0.898f));
            staminaBar.color = new Color(0.25f, 0.65f, 1f);
        }
        if (timer != null)
        {
            SetAnchors(timer.rectTransform, new Vector2(0.465f, 0.902f), new Vector2(0.535f, 0.962f));
            timer.fontSize = 54f;
            timer.fontStyle = FontStyles.Bold;
            timer.alignment = TextAlignmentOptions.Center;
            timer.color = Color.white;
        }
    }

    private void CreateTimerBox()
    {
        if (timer == null) return;
        Image box = Panel(timer.transform.parent, "TimerBox",
            new Vector2(0.458f, 0.898f), new Vector2(0.542f, 0.966f), FrameColor);
        box.transform.SetSiblingIndex(timer.transform.GetSiblingIndex());
    }

    private void CreateFrame(Image bar)
    {
        RectTransform rt = bar.rectTransform;
        Vector2 pad = new Vector2(0.004f, 0.006f);
        Image frame = Panel(rt.parent, bar.name + "Frame",
            rt.anchorMin - pad, rt.anchorMax + pad, FrameColor);
        frame.transform.SetSiblingIndex(bar.transform.GetSiblingIndex());
    }

    private void CreateLabels(Transform parent)
    {
        Label(parent, "PLAYER 1", new Vector2(0.115f, 0.948f), new Vector2(0.27f, 0.982f),
            18f, Color.white, TextAlignmentOptions.Left);
        Label(parent, "PLAYER 2", new Vector2(0.73f, 0.948f), new Vector2(0.885f, 0.982f),
            18f, Color.white, TextAlignmentOptions.Right);

        Label(parent, "GUARD", new Vector2(0.115f, 0.866f), new Vector2(0.175f, 0.883f),
            12f, new Color(0.4f, 0.9f, 1f), TextAlignmentOptions.Left);
        Label(parent, "GUARD", new Vector2(0.825f, 0.866f), new Vector2(0.885f, 0.883f),
            12f, new Color(0.4f, 0.9f, 1f), TextAlignmentOptions.Right);
    }

    private void BuildGauge(Transform parent, bool left)
    {
        float x0 = left ? 0.030f : 0.700f;
        float x1 = left ? 0.300f : 0.970f;

        Panel(parent, left ? "GaugeL" : "GaugeR",
            new Vector2(x0, 0.045f), new Vector2(x1, 0.078f), FrameColor);

        // Resplandor que late cuando el MAXIMUM está lleno
        Image glow = Panel(parent, left ? "GaugeGlowL" : "GaugeGlowR",
            new Vector2(x0 - 0.012f, 0.036f), new Vector2(x1 + 0.012f, 0.087f),
            new Color(1f, 0.82f, 0.15f, 0f));
        glow.sprite = CombatFX.Glow;
        glow.raycastTarget = false;
        if (left) leftGlow = glow; else rightGlow = glow;

        Image[] segments = left ? leftSegments : rightSegments;
        for (int i = 0; i < 5; i++)
        {
            float a = Mathf.Lerp(x0 + 0.005f, x1 - 0.005f, i / 5f);
            float b = Mathf.Lerp(x0 + 0.005f, x1 - 0.005f, (i + 1) / 5f) - 0.004f;
            segments[i] = Panel(parent, (left ? "SegL" : "SegR") + i,
                new Vector2(a, 0.050f), new Vector2(b, 0.073f), SegmentOff);
        }

        TMP_Text label = Label(parent, "MAXIMUM",
            new Vector2(x0, 0.006f), new Vector2(x1, 0.042f),
            18f, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center);

        if (left) leftMaximum = label; else rightMaximum = label;
    }

    private void PlaceGems(Image[] gems, bool left)
    {
        if (gems == null) return;
        for (int i = 0; i < gems.Length && i < 2; i++)
        {
            if (gems[i] == null) continue;
            RectTransform rt = gems[i].rectTransform;
            float x = left ? 0.448f + i * 0.012f : 0.540f + i * 0.012f;
            rt.anchorMin = new Vector2(x, 0.885f);
            rt.anchorMax = new Vector2(x, 0.885f);
            rt.sizeDelta = new Vector2(14f, 14f);
            rt.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }
    }

    private void Update()
    {
        if (staminaSource == null) return;

        // El medidor de abajo es el PODER (MAXIMUM), no la stamina.
        if (wallyPower == null)
        {
            PlayerInputHandler input = Object.FindFirstObjectByType<PlayerInputHandler>();
            if (input != null) wallyPower = input.GetComponent<PowerMeter>();
        }
        if (rivalPower == null)
        {
            RivalAI ai = Object.FindFirstObjectByType<RivalAI>();
            if (ai != null) rivalPower = ai.GetComponent<PowerMeter>();
        }

        float leftMax = Mathf.Max(1f, staminaSource.MaxStamina);
        float leftFraction = Mathf.Clamp01(staminaSource.CurrentStamina / leftMax);
        if (wallyPower != null)
            leftFraction = Mathf.Clamp01(wallyPower.Current / Mathf.Max(1f, wallyPower.Max));

        float rightFraction = leftFraction;
        if (rivalPower != null)
        {
            rightFraction = Mathf.Clamp01(rivalPower.Current / Mathf.Max(1f, rivalPower.Max));
        }
        else if (rivalStaminaSource != null)
        {
            float rightMax = Mathf.Max(1f, rivalStaminaSource.MaxStamina);
            rightFraction = Mathf.Clamp01(rivalStaminaSource.CurrentStamina / rightMax);
        }

        int leftLit = Mathf.FloorToInt(leftFraction * 5f);
        int rightLit = Mathf.FloorToInt(rightFraction * 5f);

        for (int i = 0; i < 5; i++)
        {
            if (leftSegments[i] != null) leftSegments[i].color = i < leftLit ? SegmentOn : SegmentOff;
            if (rightSegments[i] != null) rightSegments[i].color = i < rightLit ? SegmentOn : SegmentOff;
        }

        bool leftFull = leftFraction >= 0.999f;
        bool rightFull = rightFraction >= 0.999f;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);

        if (leftMaximum != null)
            leftMaximum.color = leftFull ? SegmentOn : new Color(0.55f, 0.5f, 0.35f);
        if (rightMaximum != null)
            rightMaximum.color = rightFull ? SegmentOn : new Color(0.55f, 0.5f, 0.35f);

        if (leftMaximum != null)
            leftMaximum.rectTransform.localScale = Vector3.one * (leftFull ? 1f + 0.06f * pulse : 1f);
        if (rightMaximum != null)
            rightMaximum.rectTransform.localScale = Vector3.one * (rightFull ? 1f + 0.06f * pulse : 1f);

        if (leftGlow != null)
            leftGlow.color = new Color(1f, 0.82f, 0.15f, leftFull ? 0.18f + 0.22f * pulse : 0f);
        if (rightGlow != null)
            rightGlow.color = new Color(1f, 0.82f, 0.15f, rightFull ? 0.18f + 0.22f * pulse : 0f);
    }

    // ── HELPERS ───────────────────────────────────────────────

    private static void SetAnchors(Image img, Vector2 min, Vector2 max)
    {
        if (img == null) return;
        SetAnchors(img.rectTransform, min, max);
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        if (rt == null) return;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Image Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TMP_Text Label(Transform parent, string text, Vector2 min, Vector2 max,
                                  float size, Color color, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Label_" + text);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }
}
