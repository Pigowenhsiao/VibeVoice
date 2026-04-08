$ErrorActionPreference = "Stop"

$IdsFile = "E:\AI Training\cc-notebook\.tmp_odd_db_ids.json"
$OutRoot = "E:\obsidian\PigoVault\Learning\notion-knowledge"
$ReportPath = "E:\AI Training\cc-notebook\.tmp_odd_db_sync_report.json"

function Get-SafeName([string]$Name) {
    $safe = $Name -replace '[<>:"/\\|?*\x00-\x1F]', "_"
    $safe = ($safe -replace '\s+', ' ').Trim()
    if ([string]::IsNullOrWhiteSpace($safe)) { $safe = "Untitled" }
    if ($safe.Length -gt 120) { $safe = $safe.Substring(0, 120).TrimEnd() }
    return $safe
}

function Pick-Folder([string]$Title, [string]$Content) {
    $text = "$Title`n$Content"
    if ($text -match '(?i)AI|Agent|Notion|Codex|Claude|GitHub|Git |Cloudflare|NotebookLM|workflow|ssh|TSMC|量子|電商') {
        return @("02_AI工程", "General")
    }
    if ($text -match '(?i)股票|遺產稅|致富|品牌|brand|金融|投資|商業|電商') {
        return @("04_商業財經", "Finance")
    }
    if ($text -match '(?i)高血壓|筋膜|肩頸|腰痛|健康|放鬆') {
        return @("03_健康生活", "Health")
    }
    if ($text -match '(?i)學習|筆記|English|Interview|Workshop|專家|鋼琴|教學') {
        return @("05_學習成長", "Learning")
    }
    return @("99_雜記", "General")
}

function Get-FirstJsonLine([string]$Raw) {
    $line = $Raw -split "`r?`n" | Where-Object { $_ -match '^\{.*\}$' } | Select-Object -First 1
    if (-not $line) {
        throw "No single-line JSON found in codex output."
    }
    return $line
}

if (-not (Test-Path $IdsFile)) {
    throw "IDs file not found: $IdsFile"
}

$idsObj = Get-Content $IdsFile -Raw | ConvertFrom-Json
$ids = @($idsObj.ids)
if ($ids.Count -eq 0) {
    throw "No IDs found in $IdsFile"
}

$converted = @()
$failedFetch = @()

for ($i = 0; $i -lt $ids.Count; $i++) {
    $id = $ids[$i]
    Write-Host "[$($i + 1)/$($ids.Count)] Fetching $id ..."
    try {
        $prompt = @"
請用 Notion MCP 讀取 page id $id。
只輸出「單行 JSON」，格式：
{"id":"...","title":"...","url":"...","processed":true,"tags":["..."],"created_time":"...","content":"..."}
說明：
- title 取「名稱」欄位或頁面標題
- processed 取「已處理」
- tags 取「標籤」multi_select
- content 放頁面 markdown 內容（不含 frontmatter）
"@
        $raw = codex exec $prompt
        $jsonLine = Get-FirstJsonLine -Raw $raw
        $obj = $jsonLine | ConvertFrom-Json

        $title = if ($obj.title) { [string]$obj.title } else { "Untitled" }
        $url = if ($obj.url) { [string]$obj.url } else { "https://www.notion.so/$($id -replace '-', '')" }
        $created = if ($obj.created_time) { [string]$obj.created_time } else { "" }
        $content = if ($obj.content) { [string]$obj.content } else { "（此頁面無可讀內容）" }
        $tags = @()
        if ($obj.tags) { $tags = @($obj.tags) }

        $folders = Pick-Folder -Title $title -Content $content
        $l1 = $folders[0]
        $l2 = $folders[1]
        $dir = Join-Path (Join-Path $OutRoot $l1) $l2
        New-Item -ItemType Directory -Force -Path $dir | Out-Null

        $safeTitle = Get-SafeName -Name $title
        $notePath = Join-Path $dir "$safeTitle-$($id -replace '-', '').md"
        $importedAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $tagList = if ($tags.Count -gt 0) { ($tags | ForEach-Object { '"' + ($_ -replace '"', "'") + '"' }) -join ", " } else { "" }

        $yaml = @(
            "---",
            "title: `"$($title -replace '"', "'")`"",
            "notion_page_id: `"$id`"",
            "notion_url: `"$url`"",
            "source: `"notion`"",
            "processed: true",
            "imported_at: `"$importedAt`"",
            "created_time: `"$created`"",
            "notion_tags: [$tagList]",
            "---",
            "",
            "## Notion 來源",
            "- 原始連結: $url",
            "",
            "## 內容",
            "",
            $content,
            ""
        ) -join "`n"

        Set-Content -Path $notePath -Value $yaml -Encoding UTF8
        $converted += [pscustomobject]@{
            id = $id
            title = $title
            file = $notePath
        }
        Write-Host "  converted -> $notePath"
    }
    catch {
        $failedFetch += [pscustomobject]@{
            id = $id
            error = $_.Exception.Message
        }
        Write-Host "  failed -> $($_.Exception.Message)"
    }
}

$updatedTotal = 0
$updateFailed = @()

$convertedIds = @($converted | ForEach-Object { $_.id })
for ($j = 0; $j -lt $convertedIds.Count; $j += 15) {
    $chunk = $convertedIds[$j..([Math]::Min($j + 14, $convertedIds.Count - 1))]
    $chunkJson = $chunk | ConvertTo-Json -Compress
    Write-Host "Updating 已處理 for chunk size $($chunk.Count) ..."
    try {
        $prompt = @"
請用 Notion MCP 將下列 page ids 的「已處理」欄位更新為 checked：
$chunkJson

對每個 page 使用 notion-update-page:
- command: update_properties
- properties: {"已處理":"__YES__"}

完成後只輸出「單行 JSON」：
{"updated": 數字, "failed": ["id..."]}
"@
        $raw = codex exec $prompt
        $jsonLine = Get-FirstJsonLine -Raw $raw
        $obj = $jsonLine | ConvertFrom-Json
        $updatedTotal += [int]$obj.updated
        if ($obj.failed) { $updateFailed += @($obj.failed) }
    }
    catch {
        $updateFailed += @($chunk)
    }
}

$verify = @()
if ($convertedIds.Count -gt 0) {
    $sampleCount = [Math]::Min(6, $convertedIds.Count)
    $sampleIds = $convertedIds | Get-Random -Count $sampleCount
    foreach ($sid in $sampleIds) {
        try {
            $prompt = @"
請用 Notion MCP 讀取 page id $sid，只輸出「單行 JSON」：
{"id":"$sid","processed":true}
其中 processed 取「已處理」欄位值。
"@
            $raw = codex exec $prompt
            $jsonLine = Get-FirstJsonLine -Raw $raw
            $verify += ($jsonLine | ConvertFrom-Json)
        }
        catch {
            $verify += [pscustomobject]@{
                id = $sid
                processed = $null
                error = $_.Exception.Message
            }
        }
    }
}

$report = [pscustomobject]@{
    input_count = $ids.Count
    converted_count = $converted.Count
    failed_fetch_count = $failedFetch.Count
    updated_count = $updatedTotal
    update_failed_count = $updateFailed.Count
    output_root = $OutRoot
    converted = $converted
    failed_fetch = $failedFetch
    update_failed = $updateFailed
    verify_samples = $verify
}

$report | ConvertTo-Json -Depth 8 | Set-Content -Path $ReportPath -Encoding UTF8
$summary = [pscustomobject]@{
    converted_count = $converted.Count
    failed_fetch_count = $failedFetch.Count
    updated_count = $updatedTotal
    update_failed_count = $updateFailed.Count
    report = $ReportPath
}
$summary | ConvertTo-Json -Compress
