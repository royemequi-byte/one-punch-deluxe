param(
  [int]$x = 960,
  [int]$y = 600,
  [int]$veces = 8,
  [int]$pausa = 1300
)

for ($i = 0; $i -lt $veces; $i++) {
  & 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\accion.ps1' -modo click -x $x -y $y | Out-Null
  Start-Sleep -Milliseconds $pausa
}

Write-Output ("Clics enviados: " + $veces)
