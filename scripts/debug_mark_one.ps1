$ErrorActionPreference = "Stop"
$id = "33a42529-badd-81b7-915b-e1acd19547fe"
$prompt = @"
請用 Notion MCP 更新 page id $id 的「已處理」欄位為 checked。
使用 notion-update-page：
- command: update_properties
- properties: {"已處理":"__YES__"}
完成後只輸出單行 JSON：
{"id":"$id","ok":true}
"@

$tmp = [System.IO.Path]::GetTempFileName()
try {
    codex exec --dangerously-bypass-approvals-and-sandbox -o $tmp $prompt | Out-Null
    Write-Host "===== LAST MESSAGE START ====="
    Get-Content $tmp -Raw
    Write-Host "===== LAST MESSAGE END ====="
}
finally {
    if (Test-Path $tmp) {
        Remove-Item $tmp -Force -ErrorAction SilentlyContinue
    }
}
