---
title: "Notion YouTube 入庫流程：授權失敗 fallback 與可驗證寫回"
date: "2026-04-06"
problem_type: "best-practice"
module: "notion-ingest"
component: "notion-mcp + codex-exec fallback"
tags: ["notion", "youtube", "mcp", "automation", "knowledge-base"]
category: "best-practices"
---

## Context

使用者要把 YouTube 影片寫入 Notion `Ai DataBase View`，包含欄位填寫、摘要與影片嵌入；但主會話的 Notion MCP 可能回傳 `Auth required`。

## Guidance

先走「直接 MCP」，失敗就切「`codex exec` 代理通道」，不要在同一路徑重試。

完整流程：

1. 定位資料庫與 data source（`notion-search`/`notion-fetch`）。
2. 讀 schema 與 multi-select 可用值。
3. 取影片 metadata（`yt-dlp`）與逐字稿（`youtube_transcript_api`）。
4. 產生 Notion 頁面內容（繁中摘要、重點、風險、建議）。
5. 寫入 properties + 內文（含單獨一行 YouTube URL 觸發嵌入）。
6. 以 `notion-fetch` 驗證欄位與內容是否持久化。

## Why This Matters

GUI 點擊式自動化易漂移、耗 token、結果不穩；CLI/MCP 管道可追蹤、可重試、可驗證。加上 fallback 通道可避免整批流程因授權抖動中斷。

## When to Apply

- 任何「YouTube -> Notion 知識庫」流程
- 任何需要「欄位 + 摘要 + 嵌入 + 寫回驗證」的一次性或批次任務
- 主會話 MCP 偶發授權失效場景

## Examples

### Example A: 主會話可用

- 直接用 `notion-create-pages` 建頁
- 同步寫入 `Name/URL/Tags/內容類型/平台/整合對象/標籤/已處理`
- fetch 驗證完成

### Example B: 主會話 `Auth required`

- 改用 `codex exec` 呼叫 Notion MCP
- 僅以 JSON 回傳關鍵結果（page id/url/核心欄位）
- 回主流程做二次驗證與結果回報
