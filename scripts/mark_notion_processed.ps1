param(
    [string]$IdsFile = "E:\AI Training\cc-notebook\.tmp_odd_db_ids.json",
    [string]$ReportPath = "E:\AI Training\cc-notebook\.tmp_odd_db_update_report.json"
)

$ErrorActionPreference = "Stop"

$CodexBypassArg = "--dangerously-bypass-approvals-and-sandbox"

function Parse-CodexObject([string]$Text) {
    $raw = $Text.Trim()
    if ([string]::IsNullOrWhiteSpace($raw)) {
        throw "Empty last message from codex exec."
    }

    try {
        return ($raw | ConvertFrom-Json)
    }
    catch {
        # Fallback for pseudo-JSON like: {id:abc,ok:true}
        if ($raw -notmatch '^\{.*\}$') {
            throw
        }
        $body = $raw.TrimStart('{').TrimEnd('}')
        $pairs = $body -split ','
        $map = @{}
        foreach ($pair in $pairs) {
            $idx = $pair.IndexOf(':')
            if ($idx -lt 1) { continue }
            $key = $pair.Substring(0, $idx).Trim().Trim('"').Trim("'")
            $val = $pair.Substring($idx + 1).Trim()
            if ($val -match '^(?i:true|false)$') {
                $map[$key] = [System.Convert]::ToBoolean($val)
            }
            else {
                $map[$key] = $val.Trim('"').Trim("'")
            }
        }
        if ($map.Count -eq 0) {
            throw "Unable to parse codex last message: $raw"
        }
        return [pscustomobject]$map
    }
}

function Invoke-CodexJson([string]$Prompt) {
    $tmp = [System.IO.Path]::GetTempFileName()
    try {
        # Use output-last-message to avoid parsing noisy execution logs.
        $null = codex exec $CodexBypassArg -o $tmp $Prompt
        $text = Get-Content $tmp -Raw
        return (Parse-CodexObject -Text $text)
    }
    finally {
        if (Test-Path $tmp) {
            Remove-Item $tmp -Force -ErrorAction SilentlyContinue
        }
    }
}

if (-not (Test-Path $IdsFile)) {
    throw "IDs file not found: $IdsFile"
}

$idsObj = Get-Content $IdsFile -Raw | ConvertFrom-Json
$ids = @($idsObj.ids)
if ($ids.Count -eq 0) {
    throw "No IDs found in $IdsFile"
}

$updated = @()
$failed = @()

for ($i = 0; $i -lt $ids.Count; $i++) {
    $id = $ids[$i]
    Write-Host "[$($i + 1)/$($ids.Count)] Updating $id ..."
    $ok = $false
    $lastError = ""
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        try {
            $prompt = @"
請用 Notion MCP 更新 page id $id 的「已處理」欄位為 checked。
使用 notion-update-page：
- command: update_properties
- properties: {"已處理":"__YES__"}
完成後只輸出單行 JSON：
{"id":"$id","ok":true}
"@
            $obj = Invoke-CodexJson -Prompt $prompt
            if ($obj.ok -eq $true) {
                $updated += $id
                $ok = $true
                Write-Host "  ok"
                break
            }
            $lastError = "ok=false"
        }
        catch {
            $lastError = $_.Exception.Message
        }
        Start-Sleep -Seconds 1
    }

    if (-not $ok) {
        $failed += [pscustomobject]@{
            id = $id
            error = $lastError
        }
        Write-Host "  failed -> $lastError"
    }
}

$verify = @()
if ($updated.Count -gt 0) {
    $sampleCount = [Math]::Min(8, $updated.Count)
    $sampleIds = $updated | Get-Random -Count $sampleCount
    foreach ($sid in $sampleIds) {
        try {
            $prompt = @"
請用 Notion MCP 讀取 page id $sid，只輸出單行 JSON：
{"id":"$sid","processed":true}
其中 processed 取「已處理」欄位。
"@
            $verify += (Invoke-CodexJson -Prompt $prompt)
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
    updated_count = $updated.Count
    failed_count = $failed.Count
    updated_ids = $updated
    failed = $failed
    verify_samples = $verify
}
$report | ConvertTo-Json -Depth 8 | Set-Content -Path $ReportPath -Encoding UTF8

$summary = [pscustomobject]@{
    input_count = $ids.Count
    updated_count = $updated.Count
    failed_count = $failed.Count
    report = $ReportPath
}
$summary | ConvertTo-Json -Compress
