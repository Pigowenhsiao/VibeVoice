import json
import random
import re
import subprocess
import sys
from datetime import datetime
from pathlib import Path
from typing import Any


IDS_FILE = Path(r"E:\AI Training\cc-notebook\.tmp_odd_db_ids.json")
OUT_DIR = Path(r"E:\obsidian\PigoVault\Learning\notion-knowledge")
REPORT_FILE = Path(r"E:\AI Training\cc-notebook\.tmp_odd_db_sync_report.json")


def run_codex(prompt: str, timeout: int = 240) -> str:
    proc = subprocess.run(
        ["codex", "exec", prompt],
        capture_output=True,
        text=True,
        timeout=timeout,
        encoding="utf-8",
        errors="ignore",
    )
    if proc.returncode != 0:
        raise RuntimeError(f"codex exec failed: {proc.stderr.strip()}")
    return proc.stdout


def extract_first_json(text: str) -> Any:
    start = text.find("{")
    if start == -1:
        raise ValueError("No JSON object start found")
    s = text[start:]
    depth = 0
    in_str = False
    esc = False
    end = None
    for i, ch in enumerate(s):
        if in_str:
            if esc:
                esc = False
            elif ch == "\\":
                esc = True
            elif ch == '"':
                in_str = False
        else:
            if ch == '"':
                in_str = True
            elif ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    end = i + 1
                    break
    if end is None:
        raise ValueError("No JSON object end found")
    return json.loads(s[:end])


def safe_filename(title: str, page_id: str) -> str:
    name = re.sub(r"[<>:\"/\\|?*\x00-\x1F]", "_", title).strip()
    name = re.sub(r"\s+", " ", name)
    if not name:
        name = "Untitled"
    if len(name) > 120:
        name = name[:120].rstrip()
    return f"{name}-{page_id.replace('-', '')}.md"


def choose_folder(title: str, content: str) -> tuple[str, str]:
    text = f"{title}\n{content}"
    ai_pat = r"(AI|Agent|Notion|Codex|Claude|GitHub|Git |Cloudflare|NotebookLM|workflow|ssh|TSMC|量子|電商)"
    biz_pat = r"(股票|遺產稅|致富|品牌|brand|金融|投資|商業|電商)"
    health_pat = r"(高血壓|筋膜|肩頸|腰痛|健康|放鬆)"
    learn_pat = r"(學習|筆記|English|Interview|Workshop|專家|鋼琴|教學)"
    if re.search(ai_pat, text, re.IGNORECASE):
        return ("02_AI工程", "General")
    if re.search(biz_pat, text, re.IGNORECASE):
        return ("04_商業財經", "Finance")
    if re.search(health_pat, text, re.IGNORECASE):
        return ("03_健康生活", "Health")
    if re.search(learn_pat, text, re.IGNORECASE):
        return ("05_學習成長", "Learning")
    return ("99_雜記", "General")


def fetch_page(page_id: str) -> dict[str, Any]:
    prompt = f"""
請用 Notion MCP 讀取 page id `{page_id}`。
只輸出 JSON，格式：
{{
  "id":"...",
  "title":"...",
  "url":"...",
  "processed": true 或 false 或 null,
  "tags": ["..."],
  "created_time":"...",
  "content":"..."
}}
說明：
- title 取「名稱」欄位或頁面標題
- processed 取「已處理」
- tags 取「標籤」multi_select
- content 放頁面 markdown 內容（不含 frontmatter）
"""
    out = run_codex(prompt, timeout=300)
    obj = extract_first_json(out)
    return obj


def write_obsidian_note(page: dict[str, Any]) -> str:
    page_id = page["id"]
    title = page.get("title") or "Untitled"
    notion_url = page.get("url") or f"https://www.notion.so/{page_id.replace('-', '')}"
    tags = page.get("tags") or []
    content = (page.get("content") or "").strip()
    created_time = page.get("created_time") or ""
    l1, l2 = choose_folder(title, content)
    folder = OUT_DIR / l1 / l2
    folder.mkdir(parents=True, exist_ok=True)
    filename = safe_filename(title, page_id)
    path = folder / filename
    imported_at = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    fm_tags = ", ".join([f'"{t}"' for t in tags])
    body = content if content else "（此頁面無可讀內容）"
    note = (
        "---\n"
        f'title: "{title.replace(chr(34), chr(39))}"\n'
        f'notion_page_id: "{page_id}"\n'
        f'notion_url: "{notion_url}"\n'
        'source: "notion"\n'
        "processed: true\n"
        f'imported_at: "{imported_at}"\n'
        f'created_time: "{created_time}"\n'
        f"notion_tags: [{fm_tags}]\n"
        "---\n\n"
        "## Notion 來源\n"
        f"- 原始連結: {notion_url}\n\n"
        "## 內容\n\n"
        f"{body}\n"
    )
    path.write_text(note, encoding="utf-8")
    return str(path)


def update_processed(ids: list[str]) -> dict[str, Any]:
    chunks = [ids[i : i + 15] for i in range(0, len(ids), 15)]
    updated = 0
    failed: list[str] = []
    for chunk in chunks:
        ids_json = json.dumps(chunk, ensure_ascii=False)
        prompt = f"""
請用 Notion MCP 將下列 page ids 的「已處理」欄位更新為 checked：
{ids_json}

對每個 page 使用 notion-update-page:
- command: update_properties
- properties: {{"已處理":"__YES__"}}

完成後只輸出 JSON：
{{"updated": 數字, "failed": ["id..."]}}
"""
        out = run_codex(prompt, timeout=300)
        obj = extract_first_json(out)
        updated += int(obj.get("updated", 0))
        failed.extend(obj.get("failed", []))
    return {"updated": updated, "failed": failed}


def verify_samples(ids: list[str], n: int = 6) -> list[dict[str, Any]]:
    sample_ids = ids if len(ids) <= n else random.sample(ids, n)
    result = []
    for pid in sample_ids:
        prompt = f"""
請用 Notion MCP 讀取 page id `{pid}`，只輸出 JSON：
{{"id":"{pid}","processed":true 或 false 或 null}}
其中 processed 取「已處理」欄位值。
"""
        out = run_codex(prompt, timeout=240)
        obj = extract_first_json(out)
        result.append(obj)
    return result


def main() -> int:
    if not IDS_FILE.exists():
        print(f"IDs file not found: {IDS_FILE}")
        return 1

    ids_obj = json.loads(IDS_FILE.read_text(encoding="utf-8"))
    ids = ids_obj.get("ids", [])
    if not ids:
        print("No IDs to process.")
        return 1

    converted = []
    failed_fetch = []
    for idx, pid in enumerate(ids, start=1):
        try:
            page = fetch_page(pid)
            file_path = write_obsidian_note(page)
            converted.append({"id": pid, "title": page.get("title", ""), "file": file_path})
            print(f"[{idx}/{len(ids)}] converted: {pid}")
        except Exception as e:
            failed_fetch.append({"id": pid, "error": str(e)})
            print(f"[{idx}/{len(ids)}] failed: {pid} -> {e}")

    converted_ids = [x["id"] for x in converted]
    update_result = {"updated": 0, "failed": []}
    verify_result = []
    if converted_ids:
        update_result = update_processed(converted_ids)
        verify_result = verify_samples(converted_ids, n=6)

    report = {
        "input_count": len(ids),
        "converted_count": len(converted),
        "failed_fetch_count": len(failed_fetch),
        "converted": converted,
        "failed_fetch": failed_fetch,
        "update_result": update_result,
        "verify_samples": verify_result,
        "output_root": str(OUT_DIR),
    }
    REPORT_FILE.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({
        "converted_count": len(converted),
        "failed_fetch_count": len(failed_fetch),
        "updated": update_result.get("updated", 0),
        "update_failed_count": len(update_result.get("failed", [])),
        "report": str(REPORT_FILE),
    }, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main())
