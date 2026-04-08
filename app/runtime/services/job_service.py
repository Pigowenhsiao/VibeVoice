from __future__ import annotations

import asyncio
import queue
import threading
import uuid
from dataclasses import dataclass
from datetime import datetime, timezone
from typing import Any

import soundfile as sf

from app.runtime.schemas.contracts import (
    ArtifactResponse,
    ErrorDetail,
    GenerateJobRequest,
    Job,
    JobActionResponse,
    JobResponse,
    JobState,
)
from app.runtime.services.runtime_service import GenerationCancelled, VibeVoiceRuntime

TERMINAL_STATES = {JobState.COMPLETED, JobState.FAILED, JobState.CANCELLED}


@dataclass(slots=True)
class JobRuntimeState:
    cancel_event: threading.Event
    duration_seconds: float | None = None


class JobEventHub:
    def __init__(self) -> None:
        self._lock = threading.Lock()
        self._subscribers: dict[str, list[queue.Queue[dict[str, Any]]]] = {}

    def subscribe(self, job_id: str) -> queue.Queue[dict[str, Any]]:
        subscriber: queue.Queue[dict[str, Any]] = queue.Queue()
        with self._lock:
            self._subscribers.setdefault(job_id, []).append(subscriber)
        return subscriber

    def unsubscribe(self, job_id: str, subscriber: queue.Queue[dict[str, Any]]) -> None:
        with self._lock:
            subscribers = self._subscribers.get(job_id, [])
            if subscriber in subscribers:
                subscribers.remove(subscriber)
            if not subscribers and job_id in self._subscribers:
                del self._subscribers[job_id]

    def publish(self, job_id: str, event: dict[str, Any]) -> None:
        with self._lock:
            subscribers = list(self._subscribers.get(job_id, []))
        for subscriber in subscribers:
            subscriber.put(event)


class JobService:
    def __init__(self, settings, runtime: VibeVoiceRuntime) -> None:
        self.settings = settings
        self.runtime = runtime
        self.event_hub = JobEventHub()
        self._jobs: dict[str, Job] = {}
        self._runtime_state: dict[str, JobRuntimeState] = {}
        self._lock = threading.RLock()

    def create_job(self, request: GenerateJobRequest) -> Job:
        with self._lock:
            self._ensure_capacity()
            if len(request.speakers) < request.speaker_count:
                raise ValueError("The number of selected voices must match speaker_count.")

            job_id = uuid.uuid4().hex
            now = datetime.now(timezone.utc)
            job = Job(
                id=job_id,
                state=JobState.QUEUED,
                progress=0.0,
                message="Job accepted.",
                created_at=now,
                updated_at=now,
                artifact_path=None,
                error=None,
            )
            self._jobs[job_id] = job
            self._runtime_state[job_id] = JobRuntimeState(cancel_event=threading.Event())
            self._publish_snapshot(job_id, job)

        thread = threading.Thread(target=self._run_job, args=(job_id, request), daemon=True)
        thread.start()
        return job

    def get_job(self, job_id: str) -> Job:
        with self._lock:
            job = self._jobs.get(job_id)
            if job is None:
                raise KeyError(job_id)
            return job

    def stop_job(self, job_id: str) -> JobActionResponse:
        with self._lock:
            job = self._jobs.get(job_id)
            runtime = self._runtime_state.get(job_id)
            if job is None or runtime is None:
                raise KeyError(job_id)
            runtime.cancel_event.set()
            if job.state not in TERMINAL_STATES:
                job.state = JobState.STOPPING
                job.message = "Stop requested."
                job.updated_at = datetime.now(timezone.utc)
                self._publish_snapshot(job_id, job)
            return JobActionResponse(accepted=True, job=job)

    def get_artifact(self, job_id: str) -> ArtifactResponse:
        with self._lock:
            job = self._jobs.get(job_id)
            runtime = self._runtime_state.get(job_id)
            if job is None or job.artifact_path is None:
                raise KeyError(job_id)
            duration = runtime.duration_seconds if runtime else None
            if duration is None:
                duration = float(sf.info(job.artifact_path).duration)
            return ArtifactResponse(job_id=job.id, path=job.artifact_path, duration_seconds=duration)

    async def stream_events(self, job_id: str):
        initial_job = self.get_job(job_id)
        yield self._job_state_event(initial_job)
        subscriber = self.event_hub.subscribe(job_id)
        try:
            while True:
                event = await asyncio.to_thread(subscriber.get)
                yield event
                event_type = event.get("type")
                payload = event.get("payload", {})
                if event_type == "job_state_changed" and payload.get("state") in {state.value for state in TERMINAL_STATES}:
                    return
        finally:
            self.event_hub.unsubscribe(job_id, subscriber)

    def _run_job(self, job_id: str, request: GenerateJobRequest) -> None:
        self._update_job(job_id, state=JobState.LOADING, progress=0.05, message="Preparing job.")
        runtime_state = self._runtime_state[job_id]

        def on_progress(progress: float, message: str) -> None:
            self._update_job(job_id, state=JobState.RUNNING, progress=progress, message=message)

        try:
            result = self.runtime.generate(request, job_id, runtime_state.cancel_event, on_progress)
            runtime_state.duration_seconds = result.duration_seconds
            self._publish_event(
                job_id,
                {
                    "type": "artifact_ready",
                    "payload": {
                        "job_id": job_id,
                        "path": str(result.artifact_path),
                        "duration_seconds": result.duration_seconds,
                    },
                },
            )
            self._update_job(
                job_id,
                state=JobState.COMPLETED,
                progress=1.0,
                message="Generation complete.",
                artifact_path=str(result.artifact_path),
            )
        except GenerationCancelled as exc:
            self._update_job(job_id, state=JobState.CANCELLED, progress=0.0, message=str(exc))
        except Exception as exc:
            self._update_job(
                job_id,
                state=JobState.FAILED,
                progress=0.0,
                message="Generation failed.",
                error=ErrorDetail(code="generation_failed", message=str(exc)),
            )

    def _ensure_capacity(self) -> None:
        active = [job for job in self._jobs.values() if job.state not in TERMINAL_STATES]
        if active:
            raise ValueError("Only one generation job is supported at a time in V1.")

    def _update_job(
        self,
        job_id: str,
        *,
        state: JobState,
        progress: float,
        message: str,
        artifact_path: str | None = None,
        error: ErrorDetail | None = None,
    ) -> None:
        with self._lock:
            job = self._jobs[job_id]
            job.state = state
            job.progress = progress
            job.message = message
            job.updated_at = datetime.now(timezone.utc)
            if artifact_path is not None:
                job.artifact_path = artifact_path
            if error is not None:
                job.error = error
        self._publish_snapshot(job_id, job)
        self._publish_event(
            job_id,
            {
                "type": "progress_updated",
                "payload": {
                    "job_id": job_id,
                    "progress": progress,
                    "message": message,
                },
            },
        )

    def _publish_snapshot(self, job_id: str, job: Job) -> None:
        self._publish_event(job_id, self._job_state_event(job))

    def _job_state_event(self, job: Job) -> dict[str, Any]:
        return {
            "type": "job_state_changed",
            "payload": {
                "job_id": job.id,
                "state": job.state.value,
                "progress": job.progress,
                "message": job.message,
                "artifact_path": job.artifact_path,
            },
        }

    def _publish_event(self, job_id: str, event: dict[str, Any]) -> None:
        self.event_hub.publish(job_id, event)
