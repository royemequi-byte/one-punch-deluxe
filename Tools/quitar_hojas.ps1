$destino = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Assets\Resources\Profile\Wally'
$basura  = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\sprites_descartados'

New-Item -ItemType Directory -Force $basura | Out-Null

Write-Output "== Antes =="
Get-ChildItem $destino -Filter '*.png' | Select-Object -ExpandProperty Name

Get-ChildItem $destino -Filter 'hit_*.png'   -ErrorAction SilentlyContinue | Move-Item -Destination $basura -Force
Get-ChildItem $destino -Filter 'block_*.png' -ErrorAction SilentlyContinue | Move-Item -Destination $basura -Force
Get-ChildItem $destino -Filter 'hit_*.meta'  -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem $destino -Filter 'block_*.meta' -ErrorAction SilentlyContinue | Remove-Item -Force

Write-Output "== Despues =="
Get-ChildItem $destino -Filter '*.png' | Select-Object -ExpandProperty Name
