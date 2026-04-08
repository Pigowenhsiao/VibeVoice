# STATUS_windows_app_v1_foundation.md

## 本次變更
- 新增根目錄 Python 相依檔：
  - [requirements.txt](E:/AI%20Training/VibeVoice/requirements.txt)
  - 內容與 [pyproject.toml](E:/AI%20Training/VibeVoice/pyproject.toml) 的主要 runtime 相依對齊
  - 額外補入目前測試需要的 `pytest` 與 `pytest-asyncio`
- 更新根目錄 README，將 Windows 桌面版的操作方式完整寫入：
  - 啟動方式
  - `Load Model -> Speaker Presets -> Generation Workspace -> Job Progress -> Result / Export -> Settings` 使用流程
  - 輸出位置
  - 常見問題與限制
- 新增 Windows V1 flow-closure 實作：
  - `RuntimeEventStreamClient`，讓 desktop 消費 typed runtime job events
  - `AudioPlaybackService`，讓 desktop 可直接播放生成結果
  - `ArtifactExportService`，讓 desktop 可將結果匯出到設定輸出目錄
  - `DesktopSettingsService`，持久化 model path、speaker count、CFG、script、output directory
- `MainViewModel` 改為 event-driven job flow：
  - 不再只靠 polling 顯示進度
  - 新增 elapsed time、result summary、playback/export/settings state
- `MainWindow.xaml` 更新為更完整的 `Load Model -> Generation Workspace -> Job Progress -> Result / Export -> Settings` 單視窗原生流程
- 新增桌面端測試：
  - `ArtifactExportServiceTests`
  - `DesktopSettingsServiceTests`
  - `MainViewModelTests`
- 新增 runtime regression test：
  - `app/runtime/tests/test_job_events.py`
- 修正 runtime 事件順序 bug：
  - `artifact_ready` 現在會在 terminal completion snapshot 前送出，避免 subscriber 提前結束
- 初始化 `.planning/`：
  - `.planning/PROJECT.md`
  - `.planning/REQUIREMENTS.md`
  - `.planning/ROADMAP.md`
  - `.planning/STATE.md`
  - `.planning/config.json`
- 新增 CEO review 文件：
  - [2026-04-05-ceo-review-windows-v1-flow.md](E:/AI%20Training/VibeVoice/docs/reviews/2026-04-05-ceo-review-windows-v1-flow.md)
- 新增 V1 功能規格文件：
  [2026-04-04-windows-v1-feature-spec.md](E:/AI%20Training/VibeVoice/docs/specs/2026-04-04-windows-v1-feature-spec.md)
- 新增第一版本機 runtime API contract：
  [local-runtime-api-v1.yaml](E:/AI%20Training/VibeVoice/docs/contracts/local-runtime-api-v1.yaml)
- 建立桌面版骨架目錄與責任說明：
  `app/backend/*`
  `app/desktop/*`
- 實作 Python backend 最小垂直切片：
  `app/backend/main.py`
  `app/backend/api/routes.py`
  `app/backend/services/*`
  `app/backend/schemas/contracts.py`
- 實作 WPF 桌面殼最小垂直切片：
  `app/desktop/VibeVoice.Desktop/MainWindow.xaml`
  `app/desktop/VibeVoice.Desktop/ViewModels/*`
  `app/desktop/VibeVoice.Desktop/Services/RuntimeApiClient.cs`
  `app/desktop/VibeVoice.Desktop/Models/ApiContracts.cs`
- 更新 `.gitignore`，避免 WPF `bin/obj` 產物污染版本控制
- 根據產品方向修正文件基線：
  桌面版最終體驗改為「單一原生 App，直接操作，不需手動啟動獨立 runtime process」
- 回頭檢視並補強 design rule：
  - 清除殘留的 `backend` 術語，改以 `runtime` / `worker` 為正式用語
  - 新增 worker startup lifecycle 規則
  - 新增 reference workflow contract，將 `Load Model -> Generation Workspace -> Job Progress -> Result / Export` 固定為主要操作流
- 桌面端新增 runtime 自動接管能力：
  - 新增 `RuntimeHostService`
  - 新增 `RuntimeLaunchOptions`
  - 新增 `RepositoryRootLocator`
  - 將原本直連 HTTP 的 `BackendApiClient` 重構為 `RuntimeApiClient`
  - `MainViewModel` 改為啟動時自動檢查並拉起本機 runtime
- 新增桌面端測試專案：
  - `app/desktop/VibeVoice.Desktop.Tests`
  - 驗證 runtime 已存在時不重啟
  - 驗證 runtime 不存在時桌面端會自動啟動並重試連線
- 更新桌面 UI 文案與流程分段：
  - `Refresh Backend` 改為 `Refresh Runtime`
  - 左右欄標題改成更貼近 `Load Model -> Generation Workspace -> Job Progress`
- 更新 desktop/runtime README，讓文件描述與目前實作一致
- 新增正式 `app/runtime` 套件，並將 `app/backend` 降為相容層：
  - `app/runtime/main.py`
  - `app/runtime/api/routes.py`
  - `app/runtime/services/*`
  - `app/runtime/schemas/contracts.py`
  - `app/runtime/tests/test_smoke.py`
  - 桌面端改為啟動 `app.runtime.main`
  - API health 欄位由 `backend_version` 改為 `runtime_version`
- 將 contract 文件正式更名為：
  [local-runtime-api-v1.yaml](E:/AI%20Training/VibeVoice/docs/contracts/local-runtime-api-v1.yaml)
- 更新桌面 DTO、測試、README 與 spec，讓 `runtime_version`、`app.runtime`、`Local Runtime` 命名全部一致
- 依 `ce:compound` 將這段收斂過程整理成可重用 solution doc：
  [windows-desktop-runtime-baseline-2026-04-04.md](E:/AI%20Training/VibeVoice/docs/solutions/best-practices/windows-desktop-runtime-baseline-2026-04-04.md)

## 驗證結果
- 已確認根目錄新增 [requirements.txt](E:/AI%20Training/VibeVoice/requirements.txt)
- 已人工比對其內容與 [pyproject.toml](E:/AI%20Training/VibeVoice/pyproject.toml)：
  - 主要 runtime 相依已同步
  - 去除 `pyproject.toml` 中重複出現的 `librosa`
  - 補入目前測試所需的 `pytest` 與 `pytest-asyncio`
- 已確認 [README.md](E:/AI%20Training/VibeVoice/README.md) 新增 Windows 桌面操作說明段落，包含：
  - `Usage 3: Run the Windows desktop app`
  - `Windows Desktop App Workflow`
  - `Troubleshooting`
  - `Current Limitations of the Windows App`
- 已執行 runtime 測試：
  - `python -m pytest app/runtime/tests -q`
  - 結果：`3 passed`
- 已確認規格、contract、骨架檔案都已寫入 repo
- 規格內容已對齊既有 design rule：
  [windows-app-design-rules.md](E:/AI%20Training/VibeVoice/docs/design-rules/windows-app-design-rules.md)
- `python -m pip install -e .` 已完成，runtime 依賴可用
- 已更新 design rule 與 V1 spec，將最終架構基線從 `loopback server` 改為 `desktop-managed internal worker`
- 已重新檢查 design rule 關鍵條文，確認：
  - 文件明確禁止手動啟動 server
  - 文件明確要求桌面 app 自動管理 worker lifecycle
  - 文件明確固定主操作流，不允許 server/API 連線頁進入 primary flow
  - 文件中的 `backend` 殘留術語已從 design rule 主文移除
- 已執行 Python runtime smoke tests：
  - `python -m pytest app/runtime/tests/test_smoke.py -q`
  - 結果：`2 passed`
- 已執行桌面端單元測試：
  - `dotnet test app/desktop/VibeVoice.Desktop.Tests/VibeVoice.Desktop.Tests.csproj`
  - 結果：`5 passed`
- 已執行桌面端建置：
  - `dotnet build app/desktop/VibeVoice.Desktop/VibeVoice.Desktop.csproj`
  - 結果：建置成功，`0 errors`
- 重新驗證桌面端實機啟停行為，在 rename 後仍正確：
  - 先確認 `/health` 為 down
  - 啟動 `VibeVoice.Desktop.exe`
  - `/health` 回傳 `runtime_version`
  - `/docs` 標題顯示 `VibeVoice Local Runtime`
  - 關閉桌面 App 後，`/health` 回到 down
- 已做實際行為驗證：
  - 先確認 `http://127.0.0.1:8765/health` 在 runtime 未啟動時為 `down`
  - 直接啟動 `VibeVoice.Desktop.exe`
  - 成功觀察到 `/health` 變成 `200 OK`
  - 使用一般視窗關閉流程後，再次確認 `/health` 回到 `down`
  - 這代表目前桌面 App 已能自動啟動並在正常關閉時收掉其管理的本機 runtime
- 已依 gstack QA 的 Web 驗證思路檢查本機 runtime 可觀測面：
  - Playwright 因系統目錄權限限制失敗，無法作為本機 QA 驗證工具
  - 改用 Chrome DevTools 驗證 `http://127.0.0.1:8765/docs`
  - 已確認 Swagger UI 正常載入，標題為 `VibeVoice Local Runtime`
  - 已確認 `/health`、`/models`、`/models/load`、`/voices`、`/jobs/generate`、`/jobs/{job_id}`、`/jobs/{job_id}/stop`、`/jobs/{job_id}/artifact` 端點都存在於文件頁
- 已建立 `docs/solutions/` 知識沉澱文件，將此次從 design rule、runtime 收斂、compatibility alias 到 QA 驗證的過程固化為可搜尋參考

## 若仍失敗
- 尚未完成整體安裝包與桌面 app 分發流程
- 嘗試做外部進程型整合驗證時，桌面環境策略阻擋背景程序啟停命令；目前以單元與建置驗證為主
- API contract 目前是 V1 草案，後續若 job event payload 有新增欄位，需要同步更新 contract
- `python -m pip install -e .` 將目前 Python 環境中的 `accelerate` 與 `transformers` 降到 VibeVoice 相容版本，pip 有回報與 `chandra-ocr` 的相依衝突警告
- 目前 repo 內仍保留 `app/backend` 相容層，避免舊路徑立即失效，但正式實作應以 `app/runtime` 為準
- `dotnet test` 與 `dotnet build` 若並行執行，偶爾會被 `Microsoft Defender` 鎖住輸出 DLL；順序執行可正常通過
- gstack QA 本身偏 Web app 驗證，無法直接覆蓋 WPF 原生視窗內容；目前已用其同類型驗證方式檢查本機 runtime HTTP 面，再用 .NET build/test 與桌面啟停行為測試補齊
- 目前 repo 根目錄沒有可供 discoverability check 寫回的 `AGENTS.md` 或 `CLAUDE.md` 實體檔案，因此這輪只建立 solution doc，未再附加 instruction-file 提示

## 下一步
- 視需要移除 `app/backend` 相容層，完全收斂到 `app/runtime`
- 補上真實模型載入與完整生成路徑的 end-to-end QA
- 視需要把單視窗流程進一步收斂成更接近參考軟體的頁面或主區塊切換
- 將 `demo/gradio_demo.py` 的剩餘生成與停止流程差異完全收斂到正式 runtime 模組
- 視需要把 README 再延伸成安裝包導向文件，等 installer 流程存在後補 installer 使用方式
