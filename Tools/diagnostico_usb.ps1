Write-Output "=== Todo lo que hay conectado por USB/HID que pueda ser un mando ==="

$firmas = @{
  'VID_045E' = 'Microsoft (Xbox)'
  'VID_054C' = 'Sony (PlayStation)'
  'VID_057E' = 'Nintendo'
  'VID_0F0D' = 'Hori'
  'VID_2DC8' = '8BitDo'
  'VID_2563' = 'Generic / ShenZhen'
  'VID_0079' = 'Generic DragonRise'
  'VID_20D6' = 'PowerA'
  'VID_0E6F' = 'PDP'
  'VID_1532' = 'Razer'
  'VID_046D' = 'Logitech'
}

$encontrados = Get-PnpDevice -PresentOnly -ErrorAction SilentlyContinue | Where-Object {
  $_.InstanceId -match 'VID_'
}

foreach ($d in $encontrados) {
  foreach ($clave in $firmas.Keys) {
    if ($d.InstanceId -match $clave) {
      Write-Output ("[" + $firmas[$clave] + "] " + $d.FriendlyName)
      Write-Output ("     " + $d.InstanceId)
      Write-Output ("")
    }
  }
}

Write-Output "=== Clase XnaComposite (mandos Xbox) ==="
$xna = Get-PnpDevice -Class XnaComposite -ErrorAction SilentlyContinue
if ($xna) { $xna | Select-Object Status, Present, FriendlyName | Format-List } else { Write-Output "(ninguno)" }

Write-Output "=== Fin ==="
