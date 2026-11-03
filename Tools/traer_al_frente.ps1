Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class Traer {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int cmd);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hWnd);
}
"@

$proc = Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Select-Object -First 1
if ($proc -and $proc.MainWindowHandle -ne 0) {
  [Traer]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null   # SW_RESTORE
  [Traer]::BringWindowToTop($proc.MainWindowHandle) | Out-Null
  [Traer]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
  Write-Output "Juego al frente"
} else {
  Write-Output "No encuentro la ventana del juego"
}
