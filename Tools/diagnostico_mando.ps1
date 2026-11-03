Write-Output "=== Mandos CONECTADOS ahora ==="
$conectados = Get-PnpDevice -PresentOnly -ErrorAction SilentlyContinue | Where-Object {
  $_.FriendlyName -match 'DualSense|DualShock|Wireless Controller|Xbox|Gamepad|Joystick|6-Axis|Pro Controller'
}
if ($conectados) { $conectados | Select-Object Status, FriendlyName | Format-List } else { Write-Output "(ninguno)" }

Write-Output "=== Mandos EMPAREJADOS / INSTALADOS (aunque esten apagados) ==="
$todos = Get-PnpDevice -ErrorAction SilentlyContinue | Where-Object {
  $_.FriendlyName -match 'DualSense|DualShock|Wireless Controller|Xbox|Gamepad|Joystick|6-Axis|Pro Controller'
}
if ($todos) { $todos | Select-Object Status, Present, FriendlyName | Format-List } else { Write-Output "(ninguno)" }

Write-Output "=== Bluetooth emparejado ==="
$bt = Get-PnpDevice -Class Bluetooth -ErrorAction SilentlyContinue | Where-Object {
  $_.FriendlyName -notmatch 'Adapter|Radio|Enumerador|Service|Protocol|Device'
}
if ($bt) { $bt | Select-Object Status, Present, FriendlyName | Format-List } else { Write-Output "(nada)" }

Write-Output "=== Fin ==="
