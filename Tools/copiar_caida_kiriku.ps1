$origen  = 'C:\Users\Lenovo\.lmstudio\scratchpads\j\python-runs\GgVb'
$destino = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Resources\Profile\Kiriku'

foreach ($n in 0..5) {
  Copy-Item (Join-Path $origen ("kd_" + $n + ".png")) (Join-Path $destino ("knockdown_" + $n + ".png")) -Force
}

Write-Output "Caida de Kiriku:"
Get-ChildItem $destino -Filter 'knockdown_*.png' | Select-Object Name, Length | Format-Table
