using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Lectura directa de mandos por XInput (xinput1_4.dll de Windows).
/// Existe porque Unity Input System a veces no ve mandos que Windows SÍ ve
/// (mandos genéricos, PS4 por Bluetooth, adaptadores USB baratos...).
/// Con esto, si el mando aparece en "joy.cpl" de Windows, el juego lo lee.
/// </summary>
public static class XInputPad
{
    private const ushort DPAD_UP        = 0x0001;
    private const ushort DPAD_DOWN      = 0x0002;
    private const ushort DPAD_LEFT      = 0x0004;
    private const ushort DPAD_RIGHT     = 0x0008;
    private const ushort START          = 0x0010;
    private const ushort BACK           = 0x0020;
    private const ushort LEFT_SHOULDER  = 0x0100;
    private const ushort RIGHT_SHOULDER = 0x0200;
    private const ushort A              = 0x1000;
    private const ushort B              = 0x2000;
    private const ushort X              = 0x4000;
    private const ushort Y              = 0x8000;

    private const float ZonaMuerta = 0.28f;
    private const byte  GatilloUmbral = 110;

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_GAMEPAD
    {
        public ushort wButtons;
        public byte   bLeftTrigger;
        public byte   bRightTrigger;
        public short  sThumbLX;
        public short  sThumbLY;
        public short  sThumbRX;
        public short  sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_STATE
    {
        public uint          dwPacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern int GetState14(uint index, ref XINPUT_STATE state);

    [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")]
    private static extern int GetState13(uint index, ref XINPUT_STATE state);

    [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
    private static extern int GetState910(uint index, ref XINPUT_STATE state);

    private static bool  probado;
    private static bool  disponible = true;
    private static int   versionDll;          // 0 = sin probar, 1 = 1_4, 2 = 1_3, 3 = 9_1_0
    private static int   frameCache = -1;
    private static int   indiceActivo = -1;

    private static ushort botonesAhora;
    private static ushort botonesAntes;
    private static byte   gatilloDerAhora, gatilloDerAntes;
    private static byte   gatilloIzqAhora, gatilloIzqAntes;
    private static Vector2 stickIzq;
    private static Vector2 stickDer;

    public static bool Disponible => disponible;
    public static bool Conectado  => indiceActivo >= 0;
    public static int  Indice     => indiceActivo;

    /// <summary>Se llama solo una vez por frame; guarda el estado del mando.</summary>
    public static void Actualizar()
    {
        if (frameCache == Time.frameCount) return;
        frameCache = Time.frameCount;

        botonesAntes     = botonesAhora;
        gatilloDerAntes  = gatilloDerAhora;
        gatilloIzqAntes  = gatilloIzqAhora;

        if (!disponible) return;

        for (uint i = 0; i < 4; i++)
        {
            XINPUT_STATE estado = new XINPUT_STATE();
            int resultado = Leer(i, ref estado);
            if (resultado != 0) continue;

            indiceActivo    = (int)i;
            botonesAhora    = estado.Gamepad.wButtons;
            gatilloIzqAhora = estado.Gamepad.bLeftTrigger;
            gatilloDerAhora = estado.Gamepad.bRightTrigger;
            stickIzq = Normalizar(estado.Gamepad.sThumbLX, estado.Gamepad.sThumbLY);
            stickDer = Normalizar(estado.Gamepad.sThumbRX, estado.Gamepad.sThumbRY);
            return;
        }

        indiceActivo    = -1;
        botonesAhora    = 0;
        gatilloIzqAhora = 0;
        gatilloDerAhora = 0;
        stickIzq = Vector2.zero;
        stickDer = Vector2.zero;
    }

    private static int Leer(uint indice, ref XINPUT_STATE estado)
    {
        if (!probado)
        {
            probado = true;
            versionDll = DetectarVersion();
        }

        try
        {
            switch (versionDll)
            {
                case 1: return GetState14(indice, ref estado);
                case 2: return GetState13(indice, ref estado);
                case 3: return GetState910(indice, ref estado);
                default: disponible = false; return -1;
            }
        }
        catch (Exception)
        {
            disponible = false;
            return -1;
        }
    }

    private static int DetectarVersion()
    {
        int[] versiones = { 1, 2, 3 };
        for (int i = 0; i < versiones.Length; i++)
        {
            try
            {
                XINPUT_STATE prueba = new XINPUT_STATE();
                switch (versiones[i])
                {
                    case 1: GetState14(0, ref prueba); return 1;
                    case 2: GetState13(0, ref prueba); return 2;
                    case 3: GetState910(0, ref prueba); return 3;
                }
            }
            catch (Exception) { }
        }
        return 0;
    }

    private static Vector2 Normalizar(short x, short y)
    {
        Vector2 v = new Vector2(x / 32767f, y / 32767f);
        if (v.magnitude < ZonaMuerta) return Vector2.zero;
        return v;
    }

    // ── CONSULTAS ────────────────────────────────────────────────

    public static Vector2 StickIzquierdo { get { Actualizar(); return stickIzq; } }

    public static float EjeX
    {
        get
        {
            Actualizar();
            if (!Conectado) return 0f;

            if ((botonesAhora & DPAD_LEFT)  != 0) return -1f;
            if ((botonesAhora & DPAD_RIGHT) != 0) return  1f;
            return Mathf.Abs(stickIzq.x) > ZonaMuerta ? Mathf.Sign(stickIzq.x) : 0f;
        }
    }

    public static float EjeY
    {
        get
        {
            Actualizar();
            if (!Conectado) return 0f;

            if ((botonesAhora & DPAD_UP)   != 0) return  1f;
            if ((botonesAhora & DPAD_DOWN) != 0) return -1f;
            return Mathf.Abs(stickIzq.y) > ZonaMuerta ? Mathf.Sign(stickIzq.y) : 0f;
        }
    }

    public static bool Abajo
    {
        get
        {
            Actualizar();
            return Conectado && ((botonesAhora & DPAD_DOWN) != 0 || stickIzq.y < -0.55f);
        }
    }

    public static bool Mantenido(ushort mascara)
    {
        Actualizar();
        return Conectado && (botonesAhora & mascara) != 0;
    }

    public static bool Pulsado(ushort mascara)
    {
        Actualizar();
        return Conectado && (botonesAhora & mascara) != 0 && (botonesAntes & mascara) == 0;
    }

    public static bool GatilloDerechoPulsado
    {
        get
        {
            Actualizar();
            return Conectado && gatilloDerAhora >= GatilloUmbral && gatilloDerAntes < GatilloUmbral;
        }
    }

    public static bool GatilloIzquierdoPulsado
    {
        get
        {
            Actualizar();
            return Conectado && gatilloIzqAhora >= GatilloUmbral && gatilloIzqAntes < GatilloUmbral;
        }
    }

    // Constantes públicas para que PadInput las use sin números mágicos
    public const ushort BTN_A = A, BTN_B = B, BTN_X = X, BTN_Y = Y;
    public const ushort BTN_START = START, BTN_BACK = BACK;
    public const ushort BTN_L1 = LEFT_SHOULDER, BTN_R1 = RIGHT_SHOULDER;
    public const ushort BTN_UP = DPAD_UP, BTN_DOWN = DPAD_DOWN;
    public const ushort BTN_LEFT = DPAD_LEFT, BTN_RIGHT = DPAD_RIGHT;
}
