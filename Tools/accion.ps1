param(
  [string]$modo = "click",       # click | teclas | captura
  [int]$x = 960,
  [int]$y = 540,
  [string]$teclas = "",
  [string]$salida = "",
  [int]$espera = 2
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class WinApi {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, int extra);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int cmd);
}
"@

# Traer la ventana del juego al frente para que reciba las teclas
$proc = Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Select-Object -First 1
if ($proc -and $proc.MainWindowHandle -ne 0) {
  [WinApi]::ShowWindow($proc.MainWindowHandle, 5) | Out-Null
  [WinApi]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
  Start-Sleep -Milliseconds 300
}

if ($modo -eq "click") {
  [WinApi]::SetCursorPos($x, $y) | Out-Null
  Start-Sleep -Milliseconds 250
  [WinApi]::mouse_event(0x0002, 0, 0, 0, 0)   # left down
  Start-Sleep -Milliseconds 90
  [WinApi]::mouse_event(0x0004, 0, 0, 0, 0)   # left up
  Write-Output ("Clic en " + $x + "," + $y)
}

if ($modo -eq "teclas" -and $teclas -ne "") {
  [System.Windows.Forms.SendKeys]::SendWait($teclas)
  Write-Output ("Teclas enviadas: " + $teclas)
}

if ($salida -ne "") {
  Start-Sleep -Seconds $espera
  New-Item -ItemType Directory -Force (Split-Path $salida) | Out-Null
  $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
  $bmp = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
  $bmp.Save($salida, [System.Drawing.Imaging.ImageFormat]::Png)
  $g.Dispose()
  $bmp.Dispose()
  Write-Output ("Captura: " + $salida)
}
