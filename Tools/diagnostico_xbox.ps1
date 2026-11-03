Write-Output "=== 'Mando Xbox' en Windows ==="
$m = Get-PnpDevice -ErrorAction SilentlyContinue | Where-Object { $_.FriendlyName -match 'Mando Xbox|Xbox Wireless|Xbox Controller' }
if ($m) {
  $m | Select-Object Status, Present, Class, FriendlyName, InstanceId | Format-List
} else {
  Write-Output "(no aparece registrado)"
}

Write-Output ""
Write-Output "=== Estado AHORA (conectado o no) ==="
$ahora = Get-PnpDevice -PresentOnly -ErrorAction SilentlyContinue | Where-Object {
  $_.InstanceId -match 'VID_045E' -or $_.FriendlyName -match 'Xbox'
}
if ($ahora) { $ahora | Select-Object Status, Class, FriendlyName, InstanceId | Format-List } else { Write-Output "(nada conectado de Xbox ahora)" }

Write-Output "=== Fin ==="
