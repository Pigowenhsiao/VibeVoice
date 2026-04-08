# STATUS_notebooklm_obsidian_skill.md

## 本次變更
- 依 `ce:compound` 與 skill 建立流程，將固定 NotebookLM 筆記本 `論文代寫` 做成可重複使用的 Codex skill。
- 新增 skill：`C:\Users\pigow\.codex\skills\notebooklm-obsidian-learning`
- 新增 skill 主檔：
- `C:\Users\pigow\.codex\skills\notebooklm-obsidian-learning\SKILL.md`
- 新增 skill reference：
- `C:\Users\pigow\.codex\skills\notebooklm-obsidian-learning\references\notebook-playbook.md`
- 新增 `ce:compound` 解法文件：
- `E:\AI Training\VibeVoice\docs\solutions\best-practices\notebooklm-obsidian-learning-skill-2026-04-05.md`

## 驗證結果
- 已成功用 `nlm notebook get` 與 `nlm notebook query` 讀取固定 notebook：
- URL：`https://notebooklm.google.com/notebook/b4341994-db58-4821-a082-51d65715eab2`
- ID：`b4341994-db58-4821-a082-51d65715eab2`
- 已確認 Obsidian vault 路徑存在：
- `E:\obsidian\PigoVault`
- 已確認 `article-notes` 路徑存在：
- `E:\obsidian\PigoVault\article-notes`
- `quick_validate.py` 驗證成功：`Skill is valid!`
- 再次執行 `nlm notebook query ... "what is the minimum repeatable workflow..." --json` 成功，固定 notebook 可正常回傳學習流程內容。

## 若仍失敗
- 若 notebook 之後被刪除、改權限、改帳號可見性，skill 仍存在，但查詢會失效。
- 若 `nlm` 登入狀態失效，skill 的流程仍正確，但執行會卡在 NotebookLM 認證。
- 本次僅做結構驗證與依賴查詢驗證，尚未用一個全新 Codex session 實際觸發自動 skill 發現流程。

## 下一步
- 在新的 Codex 工作階段中直接用自然語言請求「用 NotebookLM + Obsidian 規劃學習」，確認 skill 能被自動觸發。
- 如有需要，再補一個把查詢結果直接寫進 Obsidian 的 helper script。
