from __future__ import annotations

from datetime import datetime
from enum import Enum
from typing import Optional

from pydantic import BaseModel, Field


class JobState(str, Enum):
    QUEUED = "queued"
    LOADING = "loading"
    RUNNING = "running"
    STOPPING = "stopping"
    COMPLETED = "completed"
    FAILED = "failed"
    CANCELLED = "cancelled"


class RuntimeInfo(BaseModel):
    device: str
    python_version: str
    ffmpeg_available: bool
    checkpoints_root: str


class ModelInfo(BaseModel):
    id: Optional[str] = None
    path: Optional[str] = None
    loaded: bool
    status: str
    device: Optional[str] = None
    message: Optional[str] = None


class HealthResponse(BaseModel):
    status: str
    runtime_version: str
    runtime: RuntimeInfo
    model: ModelInfo


class ModelListResponse(BaseModel):
    items: list[ModelInfo]


class LoadModelRequest(BaseModel):
    model_path: str
    inference_steps: int = Field(default=10, ge=1)


class ModelActionResponse(BaseModel):
    accepted: bool
    model: ModelInfo


class VoicePreset(BaseModel):
    id: str
    display_name: str
    path: str
    language_hint: Optional[str] = None


class VoiceListResponse(BaseModel):
    items: list[VoicePreset]


class SpeakerSelection(BaseModel):
    slot: int = Field(ge=1, le=4)
    voice_id: str


class GenerateJobRequest(BaseModel):
    speaker_count: int = Field(ge=1, le=4)
    speakers: list[SpeakerSelection]
    script: str = Field(min_length=1)
    cfg_scale: float = Field(default=1.3, ge=1.0, le=2.0)
    output_dir: Optional[str] = None


class ErrorDetail(BaseModel):
    code: str
    message: str
    detail: Optional[str] = None


class ErrorResponse(BaseModel):
    error: ErrorDetail


class Job(BaseModel):
    id: str
    state: JobState
    progress: float = Field(ge=0.0, le=1.0)
    message: Optional[str] = None
    created_at: datetime
    updated_at: datetime
    artifact_path: Optional[str] = None
    error: Optional[ErrorDetail] = None


class CreateJobResponse(BaseModel):
    accepted: bool
    job: Job


class JobResponse(BaseModel):
    job: Job


class JobActionResponse(BaseModel):
    accepted: bool
    job: Job


class ArtifactResponse(BaseModel):
    job_id: str
    path: str
    format: str = "wav"
    duration_seconds: Optional[float] = None
