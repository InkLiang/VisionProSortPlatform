$j = Get-Content 'D:\app\Work\visionPro\project\VisionProSortPlatform\diagrams\2026-09-15T143938\01-main\board_raw.json' -Raw -Encoding UTF8 | ConvertFrom-Json
$txt = ($j | ConvertTo-Json -Depth 30)
$keys = @('工业视觉分拣系统','主运行页','急停','拍照检测','匹配 98.5','设备状态')
foreach ($k in $keys) {
    if ($txt -match [regex]::Escape($k)) { Write-Host ("OK   : " + $k) } else { Write-Host ("MISS : " + $k) }
}
Write-Host ("text-node count: " + ([regex]::Matches($txt, '"text"').Count))
Write-Host ("total chars: " + $txt.Length)
