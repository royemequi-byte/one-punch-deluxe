$ErrorActionPreference = 'Continue'
$dir = 'C:\Users\Lenovo\Documents\OnePunchDeluxe\Tools\audio_incoming'
New-Item -ItemType Directory -Force $dir | Out-Null

$files = @{
  'impactPunch_heavy_000.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactPunch_heavy_000.ogg'
  'impactPunch_heavy_001.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactPunch_heavy_001.ogg'
  'impactPunch_heavy_002.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactPunch_heavy_002.ogg'
  'impactPunch_medium_000.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactPunch_medium_000.ogg'
  'impactPunch_medium_001.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactPunch_medium_001.ogg'
  'impactSoft_heavy_000.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactSoft_heavy_000.ogg'
  'impactBell_heavy_000.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactBell_heavy_000.ogg'
  'impactPlate_light_000.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/impactPlate_light_000.ogg'
  'footstep_wood_000.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/footstep_wood_000.ogg'
  'footstep_wood_001.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/footstep_wood_001.ogg'
  'footstep_wood_002.ogg' = 'https://cc0-sounds.netlify.app/sounds/kenney_impactsounds/Audio/footstep_wood_002.ogg'
  'crowd_shouting.ogg' = 'https://opengameart.org/sites/default/files/crowd_shouting_0.ogg'
  'boxing_crowd.mp3' = 'https://archive.org/download/Red_Library_Crowds_Sports/R07-06-Boxing%20Match.mp3'
  'battle_8bit.ogg' = 'https://opengameart.org/sites/default/files/8BitBattleLoop_0.ogg'
  'battle_chip.ogg' = 'https://opengameart.org/sites/default/files/battle_music_01-loop.ogg'
  'menu_loop.wav' = 'https://opengameart.org/sites/default/files/Loop-Menu.wav'
}

foreach ($name in $files.Keys) {
  $out = Join-Path $dir $name
  if (Test-Path $out) { continue }
  try {
    Invoke-WebRequest -Uri $files[$name] -OutFile $out -TimeoutSec 60
    Write-Output ("OK   " + $name)
  } catch {
    Write-Output ("FAIL " + $name + " :: " + $_.Exception.Message)
  }
}

Get-ChildItem $dir -File | Select-Object Name, Length | Format-Table -AutoSize
