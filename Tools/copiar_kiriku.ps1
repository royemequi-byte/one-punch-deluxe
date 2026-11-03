$origen  = 'C:\Users\Lenovo\.lmstudio\scratchpads\j\python-runs\nVWo'
$destino = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Resources\Profile\Kiriku'

foreach ($n in 0..3) {
  Copy-Item (Join-Path $origen ("hit_"   + $n + ".png")) (Join-Path $destino ("hit_"   + $n + ".png")) -Force
  Copy-Item (Join-Path $origen ("block_" + $n + ".png")) (Join-Path $destino ("block_" + $n + ".png")) -Force
}

Write-Output "Kiriku:"
Get-ChildItem $destino -Filter 'hit_*.png'   | Select-Object Name, Length | Format-Table
Get-ChildItem $destino -Filter 'block_*.png' | Select-Object Name, Length | Format-Table
