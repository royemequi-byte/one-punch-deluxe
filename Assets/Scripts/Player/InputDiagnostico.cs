using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Panel de diagnóstico del mando. Se abre y se cierra con F1.
/// Muestra exactamente qué dispositivos ve Unity, si XInput ve algo,
/// y qué está llegando en vivo (ejes y botones). Sirve para saber en 2
/// segundos si el problema es el juego o Windows.
/// </summary>
public class InputDiagnostico : MonoBehaviour
{
    private GameObject panel;
    private TextMeshProUGUI texto;
    private bool visible;
    private readonly StringBuilder sb = new StringBuilder();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Crear()
    {
        if (Object.FindFirstObjectByType<InputDiagnostico>() != null) return;

        GameObject go = new GameObject("InputDiagnostico");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<InputDiagnostico>();
    }

    private void Start()
    {
        PadInput.RegistrarDispositivos(true);

        Debug.Log("[Entrada] Dispositivos del Input System:");
        foreach (InputDevice d in InputSystem.devices)
            Debug.Log("   · " + d.displayName + "  [" + d.layout + "]");

        // Se puede abrir directo con:  OnePunchDeluxe.exe -diag
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i].ToLower();
            if (a == "-diag" || a == "--diag" || a == "-diagnostico")
            {
                Alternar();
                break;
            }
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f1Key.wasPressedThisFrame) Alternar();

        if (visible) Refrescar();
    }

    private void Alternar()
    {
        visible = !visible;

        if (panel == null) Construir();
        panel.SetActive(visible);

        if (visible) Refrescar();
    }

    private void Refrescar()
    {
        if (texto == null) return;

        sb.Clear();
        sb.AppendLine("<b>DIAGNÓSTICO DE MANDO</b>      <size=70%>F1 para cerrar</size>");
        sb.AppendLine();

        PadInput.Via via = PadInput.ViaActiva;
        sb.AppendLine("Vía activa: <b>" + via + "</b>");
        sb.AppendLine("Nombre: " + PadInput.NombreDispositivo);
        sb.AppendLine("¿PlayStation? " + (PadInput.EsPlayStation ? "sí" : "no"));
        sb.AppendLine();

        sb.AppendLine("<b>Unity Input System ve:</b>");
        int cuenta = 0;
        foreach (InputDevice d in InputSystem.devices)
        {
            if (d is Keyboard || d is Mouse || d is Touchscreen) continue;
            sb.AppendLine("  · " + d.displayName + "  [" + d.layout + "]");
            cuenta++;
        }
        if (cuenta == 0) sb.AppendLine("  · (ningún mando)");

        sb.AppendLine();
        sb.AppendLine("<b>XInput (Windows) ve:</b> " +
                      (XInputPad.Conectado ? ("conectado en el puerto #" + XInputPad.Indice) :
                       (XInputPad.Disponible ? "ningún mando" : "dll no disponible")));

        sb.AppendLine();
        sb.AppendLine("<b>En vivo:</b>");
        sb.AppendLine("  Eje X: " + PadInput.EjeHorizontal.ToString("0.00") +
                      "   Eje Y: " + PadInput.EjeVertical.ToString("0.00"));
        sb.AppendLine("  Jab " + Marca(PadInput.JabPulsado) +
                      "  Recto " + Marca(PadInput.RectoPulsado) +
                      "  Gancho " + Marca(PadInput.GanchoPulsado));
        sb.AppendLine("  Salto " + Marca(PadInput.SaltoPulsado) +
                      "  Súper " + Marca(PadInput.SuperPulsado) +
                      "  Bloqueo " + Marca(PadInput.BloqueoMantenido));
        sb.AppendLine();
        sb.AppendLine("<size=75%>Si aquí no aparece tu mando, Windows no lo está entregando:\n" +
                      "pruébalo en Inicio → \"Configurar mandos USB de juego\" (joy.cpl).</size>");

        texto.text = sb.ToString();
    }

    private static string Marca(bool activo) => activo ? "<color=#66FF66>■</color>" : "<color=#555555>□</color>";

    private void Construir()
    {
        GameObject canvasGO = new GameObject("DiagnosticoCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject fondo = new GameObject("Fondo", typeof(RectTransform));
        fondo.transform.SetParent(canvasGO.transform, false);
        RectTransform frt = (RectTransform)fondo.transform;
        frt.anchorMin = new Vector2(0f, 0.15f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = new Vector2(40f, 0f);
        frt.offsetMax = new Vector2(-40f, -40f);

        Image img = fondo.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.82f);
        img.raycastTarget = false;

        GameObject go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(fondo.transform, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(40f, 30f);
        rt.offsetMax = new Vector2(-40f, -30f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.font == null)
            tmp.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        tmp.fontSize = 26f;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.raycastTarget = false;

        texto = tmp;
        panel = canvasGO;
        panel.SetActive(false);
    }
}
