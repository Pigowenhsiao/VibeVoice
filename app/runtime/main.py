from __future__ import annotations

from pathlib import Path

from fastapi import FastAPI

from app.runtime.api.routes import build_router
from app.runtime.services.app_state import AppState


def create_app() -> FastAPI:
    repo_root = Path(__file__).resolve().parents[2]
    state = AppState(repo_root)
    app = FastAPI(title="VibeVoice Local Runtime", version="1.0.0")
    app.state.container = state
    app.include_router(build_router(state))
    return app


app = create_app()


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("app.runtime.main:app", host="127.0.0.1", port=8765, reload=False)
