Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class Win2 {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, int e);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
}
"@

function TraerJuego {
    $p = Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
    if ($p) {
        [Win2]::ShowWindow($p.MainWindowHandle, 3) | Out-Null
        [Win2]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
        Start-Sleep -Milliseconds 400
        return $true
    }
    return $false
}

function Clic($x, $y) {
    TraerJuego | Out-Null
    [Win2]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 200
    [Win2]::mouse_event(0x0002,0,0,0,0)
    Start-Sleep -Milliseconds 90
    [Win2]::mouse_event(0x0004,0,0,0,0)
}

function Capturar($ruta) {
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save($ruta, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}

$carpeta = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\capturas'
New-Item -ItemType Directory -Force $carpeta | Out-Null

Start-Process -FilePath 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Builds\OnePunchDeluxe\OnePunchDeluxe.exe'
Start-Sleep -Seconds 10

Clic 960 535          # NUEVA PARTIDA
Start-Sleep -Seconds 4

for ($i=0; $i -lt 6; $i++) { Clic 960 780; Start-Sleep -Milliseconds 1500 }   # avanzar historia

Start-Sleep -Seconds 3
Write-Output "en pelea, capturando rafaga"

for ($i=0; $i -lt 10; $i++) {
    TraerJuego | Out-Null
    Capturar (Join-Path $carpeta ("burst_" + $i + ".png"))
    Start-Sleep -Milliseconds 420
}

Write-Output "listo"
