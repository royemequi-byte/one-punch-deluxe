param(
  [string]$salida = "C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\capturas\menu.png",
  [int]$espera = 8
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

New-Item -ItemType Directory -Force (Split-Path $salida) | Out-Null
Start-Sleep -Seconds $espera

$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$bmp.Save($salida, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()

Write-Output ("Captura guardada en " + $salida)
