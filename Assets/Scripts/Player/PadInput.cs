using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Única puerta de entrada para el mando, con TRES vías por si una falla:
///   1. Gamepad  → mandos que Unity reconoce (PS4/PS5/Xbox oficiales).
///   2. Joystick → mandos genéricos / DirectInput que Unity ve pero no como Gamepad.
///   3. XInput   → lectura directa de Windows (xinput1_4.dll). Es la red de
///                 seguridad: si el mando aparece en "joy.cpl", aquí aparece.
/// Usar PadInput en vez de Gamepad.current es lo que hace que el mando
/// funcione también cuando se conecta después de abrir el juego.
/// </summary>
public static class PadInput
{
    public enum Via { Ninguna, UnityGamepad, UnityJoystick, XInput }

    private static int ultimoConteo = -1;

    // ── DETECCIÓN ────────────────────────────────────────────────

    public static Gamepad GamepadActivo
    {
        get
        {
            var todos = Gamepad.all;
            if (todos.Count > 0) return todos[0];
            return Gamepad.current;
        }
    }

    public static Joystick JoystickActivo
    {
        get
        {
            var todos = Joystick.all;
            return todos.Count > 0 ? todos[0] : null;
        }
    }

    public static bool HayMando
    {
        get
        {
            if (GamepadActivo != null) return true;
            if (JoystickActivo != null) return true;
            XInputPad.Actualizar();
            return XInputPad.Conectado;
        }
    }

    public static Via ViaActiva
    {
        get
        {
            if (GamepadActivo != null) return Via.UnityGamepad;
            if (JoystickActivo != null) return Via.UnityJoystick;
            XInputPad.Actualizar();
            if (XInputPad.Conectado) return Via.XInput;
            return Via.Ninguna;
        }
    }

    public static string NombreDispositivo
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return string.IsNullOrEmpty(g.description.product) ? g.displayName : g.description.product;

            Joystick j = JoystickActivo;
            if (j != null) return string.IsNullOrEmpty(j.description.product) ? j.displayName : j.description.product;

            XInputPad.Actualizar();
            if (XInputPad.Conectado) return "Mando XInput (Windows) #" + XInputPad.Indice;

            return "Teclado";
        }
    }

    public static bool EsPlayStation
    {
        get
        {
            string n = NombreDispositivo.ToLower();
            return n.Contains("dual") || n.Contains("playstation") || n.Contains("wireless controller")
                || n.Contains("sony") || n.Contains("ps4") || n.Contains("ps5");
        }
    }

    /// <summary>Resumen de una línea para el log y el panel de diagnóstico.</summary>
    public static string Resumen
    {
        get
        {
            XInputPad.Actualizar();

            string texto = "Via: " + ViaActiva;

            Gamepad g = GamepadActivo;
            if (g != null) texto += "  |  " + g.displayName + " [" + g.layout + "]";

            Joystick j = JoystickActivo;
            if (j != null) texto += "  |  " + j.displayName + " [" + j.layout + "]";

            texto += "  |  XInput: " + (XInputPad.Conectado ? ("conectado #" + XInputPad.Indice) : "nada");

            return texto;
        }
    }

    /// <summary>Escribe en el log qué mandos ve el juego (solo cuando cambia).</summary>
    public static void RegistrarDispositivos(bool forzar = false)
    {
        XInputPad.Actualizar();

        int conteo = Gamepad.all.Count * 100 + Joystick.all.Count * 10 + (XInputPad.Conectado ? 1 : 0);
        if (!forzar && conteo == ultimoConteo) return;
        ultimoConteo = conteo;

        Debug.Log("[Entrada] " + Resumen);
    }

    // ── DIRECCIÓN ────────────────────────────────────────────────

    public static float EjeHorizontal
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null)
            {
                float eje = 0f;
                float x = g.leftStick.x.ReadValue();
                if (Mathf.Abs(x) > 0.35f) eje = Mathf.Sign(x) * Mathf.InverseLerp(0.35f, 0.9f, Mathf.Abs(x));
                if (g.dpad.left.isPressed)  eje = -1f;
                if (g.dpad.right.isPressed) eje =  1f;
                return eje;
            }

            Joystick j = JoystickActivo;
            if (j != null)
            {
                float eje = 0f;
                StickControl s = j.TryGetChildControl<StickControl>("stick");
                if (s != null)
                {
                    float x = s.x.ReadValue();
                    if (Mathf.Abs(x) > 0.35f) eje = Mathf.Sign(x);
                }
                ButtonControl b4 = BotonJoystick(4);
                ButtonControl b6 = BotonJoystick(6);
                if (b4 != null && b4.isPressed) eje = -1f;
                if (b6 != null && b6.isPressed) eje =  1f;
                return eje;
            }

            return XInputPad.EjeX;
        }
    }

    public static float EjeVertical
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null)
            {
                float y = g.leftStick.y.ReadValue();
                if (Mathf.Abs(y) > 0.35f) return Mathf.Sign(y);
                if (g.dpad.up.isPressed)   return  1f;
                if (g.dpad.down.isPressed) return -1f;
                return 0f;
            }

            Joystick j = JoystickActivo;
            if (j != null)
            {
                StickControl s = j.TryGetChildControl<StickControl>("stick");
                if (s != null)
                {
                    float y = s.y.ReadValue();
                    if (Mathf.Abs(y) > 0.35f) return Mathf.Sign(y);
                }
                return 0f;
            }

            return XInputPad.EjeY;
        }
    }

    public static bool AbajoMantenido
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null)
                return g.dpad.down.isPressed || g.leftStick.y.ReadValue() < -0.55f;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                StickControl s = j.TryGetChildControl<StickControl>("stick");
                return s != null && s.y.ReadValue() < -0.55f;
            }

            return XInputPad.Abajo;
        }
    }

    public static bool ArribaPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.dpad.up.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(0);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_UP);
        }
    }

    // ── BOTONES DE JUEGO ─────────────────────────────────────────

    public static bool SaltoPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.buttonSouth.wasPressedThisFrame;   // Cruz

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(0);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_A);
        }
    }

    public static bool JabPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.buttonWest.wasPressedThisFrame;    // Cuadrado / X

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(1);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_X);
        }
    }

    public static bool RectoPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.buttonNorth.wasPressedThisFrame;   // Triángulo / Y

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(3);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_Y);
        }
    }

    public static bool GanchoPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.buttonEast.wasPressedThisFrame;    // Círculo / B

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(2);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_B);
        }
    }

    public static bool BloqueoMantenido
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.leftShoulder.isPressed || g.rightShoulder.isPressed;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(4);
                return b != null && b.isPressed;
            }

            return XInputPad.Mantenido(XInputPad.BTN_L1) || XInputPad.Mantenido(XInputPad.BTN_R1);
        }
    }

    public static bool SuperPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null)
                return g.rightTrigger.wasPressedThisFrame || g.leftTrigger.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(5);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.GatilloDerechoPulsado || XInputPad.GatilloIzquierdoPulsado;
        }
    }

    // ── NAVEGACIÓN DE MENÚS ──────────────────────────────────────

    public static bool ConfirmarPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.buttonSouth.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(0);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_A);
        }
    }

    public static bool CancelarPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.buttonEast.wasPressedThisFrame || g.selectButton.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(1);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_B) || XInputPad.Pulsado(XInputPad.BTN_BACK);
        }
    }

    public static bool InicioPulsado
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.startButton.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null)
            {
                ButtonControl b = BotonJoystick(7);
                return b != null && b.wasPressedThisFrame;
            }

            return XInputPad.Pulsado(XInputPad.BTN_START);
        }
    }

    public static bool NavIzquierda
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.dpad.left.wasPressedThisFrame || g.leftStick.left.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null) return EjeHorizontal < -0.7f;

            return XInputPad.Pulsado(XInputPad.BTN_LEFT) || XInputPad.EjeX < -0.7f;
        }
    }

    public static bool NavDerecha
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.dpad.right.wasPressedThisFrame || g.leftStick.right.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null) return EjeHorizontal > 0.7f;

            return XInputPad.Pulsado(XInputPad.BTN_RIGHT) || XInputPad.EjeX > 0.7f;
        }
    }

    public static bool NavArriba
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.dpad.up.wasPressedThisFrame || g.leftStick.up.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null) return EjeVertical > 0.7f;

            return XInputPad.Pulsado(XInputPad.BTN_UP) || XInputPad.EjeY > 0.7f;
        }
    }

    public static bool NavAbajo
    {
        get
        {
            Gamepad g = GamepadActivo;
            if (g != null) return g.dpad.down.wasPressedThisFrame || g.leftStick.down.wasPressedThisFrame;

            Joystick j = JoystickActivo;
            if (j != null) return EjeVertical < -0.7f;

            return XInputPad.Pulsado(XInputPad.BTN_DOWN) || XInputPad.EjeY < -0.7f;
        }
    }

    // ── AYUDA ────────────────────────────────────────────────────

    private static ButtonControl BotonJoystick(int numero)
    {
        Joystick j = JoystickActivo;
        if (j == null) return null;
        return j.TryGetChildControl<ButtonControl>("button" + numero);
    }
}
