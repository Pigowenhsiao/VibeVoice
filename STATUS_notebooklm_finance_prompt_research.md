# STATUS_notebooklm_finance_prompt_research.md

## 本次變更
- 研究 NotebookLM 筆記本 `fc3a79a8-b52b-4765-b11d-51c7f26ea8e3`
- 確認筆記標題為 `[財經] AI 財報數據自動化提取：八要素 SSOT 指令指南`
- 確認來源數量為 1，主來源為 `🔢 Prompt 解拆及示範︰命令 AI 從年報提取公司 (任何港股適用) 歷年經營數據`
- 提取並整理下列重點：
- 八要素定義
- SSOT 的用途
- DIP / C.V.S. 驗證機制
- 完整使用流程
- 核心痛點、常見錯誤、方法限制
- 新增 Obsidian 筆記：
- `E:\obsidian\PigoVault\article-notes\07-analysis\Finance\AI-Financial-Statement-Eight-Factors-SSOT-Guide.md`
- 更新 hub 連結：
- `NotebookLM-Index.md`
- `Knowledge-Management-Index.md`
- `AI-Workflow-Index.md`
- `Kind-Analysis-Index.md`
- `Prompt-Engineering-Index.md`
- 依使用者需求，補充一份可直接給 Gemini 使用的台股財報提取 prompt
- 範例公司驗證為：`宏全國際股份有限公司（9939）`

## 驗證結果
- 使用 `nlm doctor` 驗證 NotebookLM CLI 與登入狀態，結果為 `All checks passed`
- 使用 `nlm notebook get ... --json` 驗證 notebook 存在、標題、來源數與來源標題
- 使用 `nlm notebook describe ... --json` 取得 AI 生成摘要
- 使用 `nlm notebook query ... --json` 追問方法論細節與限制
- 已驗證結論：
- 此 notebook 的核心不是一般財報摘要，而是用 Prompt Engineering 把 AI 變成可審計的財務數據提取流程
- 方法核心是 `八要素 + SSOT + DIP/C.V.S. 雙源驗證 + 續跑補缺`
- Obsidian 筆記已建立，並完成 hub 掛載
- 已額外查核範例公司：
- `宏全國際股份有限公司（9939）`
- 驗證日期：`2026-04-05`
- 來源交叉參考：
- 政大商學院 OSAAS 公司頁
- 鉅亨公告頁

## 若仍失敗
- `notebooklm` skill 內建 `run.py` 環境初始化失敗，卡在 `.venv` 內 `pip install --upgrade pip`
- 本次已改用可正常工作的 `nlm` CLI 完成研究
- 風險：
- 目前依賴 `nlm` CLI，而非 skill 內自帶 Python wrapper
- 若後續要修 skill，本問題需另行排查

## 下一步
- 可進一步抽成「財報數據提取版 NotebookLM -> Obsidian 模板」
- 若要重複使用，可把這套財經研究流程整理成獨立 skill 或 prompt playbook
- 可再補一版：
- 台股通用版 prompt
- 美股 ADR 版 prompt
- 銀行 / 保險 / REIT 特化版 prompt
