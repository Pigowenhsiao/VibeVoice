# STATUS_windows_app_design_rules.md

## 本次變更
- 新增 Windows 桌面版架構基線文件：
  [windows-app-design-rules.md](E:/AI%20Training/VibeVoice/docs/design-rules/windows-app-design-rules.md)
- 文件定義了桌面 UI、Python runtime、`vibevoice/` 模型核心三層分工
- 文件補充了 IPC 邊界、V1 endpoint、MVVM 規則、遷移規則與 OpenSpec 採用建議
- 已將目前 repo 的實作現況正式寫回 design rule：
  - `app/runtime` 為正式主路徑
  - `app/backend` 僅為相容層
  - contract 文件以 `local-runtime-api-v1.yaml` 為準
  - health payload 命名以 `runtime_version` 為準
  - 測試規則補進 runtime smoke tests、desktop auto-start tests 與 `/docs` 驗證方式
- 本輪再同步目前已落地的 V1 flow-closure：
  - desktop 已改為 typed runtime event consumption
  - desktop 已有 `Result / Export` 與 `Settings` 的正式 service 邊界
  - design rule 已記錄這些 service 與當前 baseline 解讀

## 驗證結果
- 已確認 repo 原本沒有 `STATUS_*.md` 與既有 `docs/` 結構，這次為首次建立
- 已將設計規則寫入 repo 內，作為後續規劃與實作參考基線
- 已重新檢查 design rule 內容，確認已包含：
  - `app/runtime` 正式主路徑
  - `app/backend` compatibility alias 規則
  - `docs/contracts/local-runtime-api-v1.yaml`
  - desktop-managed loopback HTTP runtime 的現況說明
  - runtime 命名而非 backend 命名的契約規則
  - typed runtime event consumption 的現況
  - `Result / Export` 與 `Settings` 已進入 current baseline

## 若仍失敗
- 目前沒有程式執行失敗
- 限制是這份文件仍是架構基線，不會逐行記錄所有實作細節
- 若後續技術選型從 `WPF` 改為 `WinUI 3`，需要更新這份文件的桌面殼規則

## 下一步
- 後續新功能必須直接落在 `app/runtime` 與 `app/desktop`
- 視時機移除 `app/backend` 相容層
- 持續讓 feature spec、contract 與 design rule 三份文件保持同一套命名與邊界
