using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Muestra los controles abajo, con letras de mando cuando hay uno conectado
/// y con teclas cuando no. Se actualiza solo al conectar/desconectar el mando.
/// </summary>
public class ControlsHint : MonoBehaviour
{
    private TMP_Text text;
    private string lastKey;

    private void Awake()
    {
        ConstruirUI();
        Refrescar(true);
    }

    private void Update()
    {
        Refrescar(false);
    }

    private void Refrescar(bool force)
    {
        PadInput.RegistrarDispositivos();

        bool hasPad = PadInput.HayMando;
        bool isPlayStation = PadInput.EsPlayStation;

        string key = hasPad ? (isPlayStation ? "ps" : "xbox") : "kb";
        if (!force && key == lastKey) return;
        lastKey = key;

        if (!hasPad)
        {
            text.text = "FLECHAS O A/D MOVER   ·   J JAB   ·   K RECTO   ·   L GANCHO   ·   U SÚPER (MAXIMUM)   ·   ESPACIO BLOQUEAR";
        }
        else if (isPlayStation)
        {
            text.text = "STICK / D-PAD MOVER   ·   CUADRADO JAB   ·   TRIÁNGULO RECTO   ·   CÍRCULO GANCHO   ·   R2 SÚPER   ·   L1 BLOQUEAR";
        }
        else
        {
            text.text = "STICK / D-PAD MOVER   ·   X JAB   ·   Y RECTO   ·   B GANCHO   ·   RT SÚPER   ·   LB BLOQUEAR";
        }

        if (hasPad)
            text.text += "\n<size=70%>MANDO: " + PadInput.NombreDispositivo.ToUpper() + "</size>";
    }

    private void ConstruirUI()
    {
        GameObject canvasGO = new GameObject("ControlsCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 330;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject go = new GameObject("ControlsText");
        go.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.03f, 0.01f);
        rt.anchorMax = new Vector2(0.97f, 0.06f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        tmp.fontSize = 16f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.92f, 0.92f, 0.92f, 0.9f);
        tmp.raycastTarget = false;

        text = tmp;
    }
}
