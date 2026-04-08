from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from app.runtime.services.job_service import JobService
from app.runtime.services.runtime_service import VibeVoiceRuntime


@dataclass(slots=True)
class AppSettings:
    repo_root: Path
    voices_dir: Path
    checkpoints_root: Path
    artifacts_root: Path
    host: str = "127.0.0.1"
    port: int = 8765


class AppState:
    def __init__(self, repo_root: Path) -> None:
        self.settings = AppSettings(
            repo_root=repo_root,
            voices_dir=repo_root / "demo" / "voices",
            checkpoints_root=repo_root / "checkpoints",
            artifacts_root=repo_root / "outputs" / "jobs",
        )
        self.settings.artifacts_root.mkdir(parents=True, exist_ok=True)
        self.runtime = VibeVoiceRuntime(self.settings)
        self.jobs = JobService(self.settings, self.runtime)
