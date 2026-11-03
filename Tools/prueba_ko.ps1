Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class PWC {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

$carpeta = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\capturas'
New-Item -ItemType Directory -Force $carpeta | Out-Null

Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Start-Process -FilePath 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Builds\OnePunchDeluxe\OnePunchDeluxe.exe' -ArgumentList '-pelea'

# esperar a que el rival tumbe a Wally (unos 40-50 s) y capturar la ventana
Start-Sleep -Seconds 38

for ($i=0; $i -lt 16; $i++) {
    $p = Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
    if ($p) {
        $r = New-Object PWC+RECT
        [PWC]::GetWindowRect($p.MainWindowHandle, [ref]$r) | Out-Null
        $w = $r.Right - $r.Left; $h = $r.Bottom - $r.Top
        if ($w -gt 0 -and $h -gt 0) {
            $bmp = New-Object System.Drawing.Bitmap $w, $h
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $hdc = $g.GetHdc()
            [PWC]::PrintWindow($p.MainWindowHandle, $hdc, 2) | Out-Null
            $g.ReleaseHdc($hdc); $g.Dispose()
            $bmp.Save((Join-Path $carpeta ("ko_" + $i + ".png")), [System.Drawing.Imaging.ImageFormat]::Png)
            $bmp.Dispose()
        }
    }
    Start-Sleep -Milliseconds 700
}

Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Output "capturas ko listas"
