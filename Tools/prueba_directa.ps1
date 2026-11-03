Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$carpeta = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\capturas'
New-Item -ItemType Directory -Force $carpeta | Out-Null

Get-Process OnePunchDeluxe -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

Start-Process -FilePath 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Builds\OnePunchDeluxe\OnePunchDeluxe.exe' -ArgumentList '-pelea'
Start-Sleep -Seconds 13

for ($i=0; $i -lt 8; $i++) {
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save((Join-Path $carpeta ("pelea_" + $i + ".png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Start-Sleep -Milliseconds 450
}

Write-Output "capturas listas"
