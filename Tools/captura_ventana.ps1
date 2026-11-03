Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class PW {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

$proc = Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { Write-Output "No hay ventana del juego"; exit }

$hwnd = $proc.MainWindowHandle
$r = New-Object PW+RECT
[PW]::GetWindowRect($hwnd, [ref]$r) | Out-Null
$ancho = $r.Right - $r.Left
$alto  = $r.Bottom - $r.Top
Write-Output ("ventana: " + $ancho + "x" + $alto)

$bmp = New-Object System.Drawing.Bitmap $ancho, $alto
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
[PW]::PrintWindow($hwnd, $hdc, 2) | Out-Null
$g.ReleaseHdc($hdc)
$g.Dispose()

$salida = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\capturas\ventana.png'
$bmp.Save($salida, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output ("guardado: " + $salida)
