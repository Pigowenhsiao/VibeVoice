from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

import pytest

from app.runtime.schemas.contracts import GenerateJobRequest, SpeakerSelection
from app.runtime.services.job_service import JobService


@dataclass(slots=True)
class _FakeGenerationResult:
    artifact_path: Path
    duration_seconds: float


class _FakeRuntime:
    def __init__(self, artifact_root: Path) -> None:
        self._artifact_root = artifact_root

    def generate(self, request, job_id, cancel_event, on_progress):
        on_progress(0.4, "Generating audio")
        artifact_path = self._artifact_root / f"{job_id}.wav"
        artifact_path.write_bytes(b"RIFFdemo")
        on_progress(0.9, "Saving artifact")
        return _FakeGenerationResult(artifact_path=artifact_path, duration_seconds=3.2)


@pytest.mark.asyncio
async def test_job_service_streams_typed_progress_and_artifact_events(tmp_path: Path):
    settings = type("Settings", (), {"artifacts_root": tmp_path})
    service = JobService(settings, _FakeRuntime(tmp_path))

    job = service.create_job(
        GenerateJobRequest(
            speaker_count=1,
            speakers=[SpeakerSelection(slot=1, voice_id="speaker-a")],
            script="Speaker 0: hello",
            cfg_scale=1.3,
        )
    )

    events = []
    async for event in service.stream_events(job.id):
        events.append(event)

    event_types = [event["type"] for event in events]
    assert "job_state_changed" in event_types
    assert "progress_updated" in event_types
    assert "artifact_ready" in event_types

    artifact_event = next(event for event in events if event["type"] == "artifact_ready")
    assert artifact_event["payload"]["path"].endswith(".wav")
    assert artifact_event["payload"]["duration_seconds"] == 3.2
