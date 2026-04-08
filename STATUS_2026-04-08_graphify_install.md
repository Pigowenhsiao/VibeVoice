# STATUS_2026-04-08_graphify_install.md

## 本次變更
- 在專案虛擬環境 `E:\AI Training\VibeVoice\.venv` 安裝 `graphify`（來源：`https://github.com/safishamsi/graphify`）。
- 透過 `pip` 從 GitHub 安裝，解析到 commit `92b70ce5f4f208bb7ea4d4e796f70e52e40418eb`。
- 安裝套件名稱為 `graphifyy`，CLI 指令名稱為 `graphify`。
- 執行 `graphify codex install`，已將 Graphify 規則寫入專案根目錄 `AGENTS.md`。
- 依 AGENTS 規則對目前 Vault（repo root）執行一次 graph rebuild。

## 驗證結果
- 驗證 Python 虛擬環境可用：`.venv\Scripts\python.exe --version` 成功。
- 驗證套件資訊：`.venv\Scripts\python.exe -m pip show graphifyy` 顯示版本 `0.3.12`。
- 驗證 import：`.venv\Scripts\python.exe -c "import graphify; print('graphify import OK')"` 成功。
- 驗證 CLI：`.venv\Scripts\graphify.exe --help` 成功顯示指令說明。
- 驗證整合：`AGENTS.md` 已存在 Graphify 章節與規則（包含 `graphify-out/GRAPH_REPORT.md` 與重建指令）。
- 執行重建指令：
  `E:\AI Training\VibeVoice\.venv\Scripts\python.exe -c "from graphify.watch import _rebuild_code; from pathlib import Path; _rebuild_code(Path('.'))"`
- 重建輸出摘要：`1246 nodes, 2840 edges, 60 communities`。
- 產物確認：
  - `graphify-out\graph.json`（已更新）
  - `graphify-out\GRAPH_REPORT.md`（已更新）

## 若仍失敗
- 若輸入 `graphify` 顯示找不到指令，通常是尚未啟用 `.venv` 或 PATH 未包含 `.venv\Scripts`。
- 可改用絕對路徑執行：`E:\AI Training\VibeVoice\.venv\Scripts\graphify.exe --help`。

## 下一步
- 若要在此 repo 啟用 Codex 整合，可執行：
  `E:\AI Training\VibeVoice\.venv\Scripts\graphify.exe codex install`
- 若要先試查詢流程，可在專案根目錄執行：
  `E:\AI Training\VibeVoice\.venv\Scripts\graphify.exe query "summarize this repository"`
- 若要產生/更新知識圖，可依 `AGENTS.md` 規則在修改後執行重建指令，確保圖譜與程式碼同步。
