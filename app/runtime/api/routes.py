from __future__ import annotations

from fastapi import APIRouter, HTTPException, WebSocket, WebSocketDisconnect

from app.runtime.schemas.contracts import (
    CreateJobResponse,
    ErrorDetail,
    ErrorResponse,
    GenerateJobRequest,
    HealthResponse,
    JobResponse,
    LoadModelRequest,
    ModelActionResponse,
    ModelListResponse,
    VoiceListResponse,
)


def build_router(state) -> APIRouter:
    router = APIRouter()

    @router.get("/health", response_model=HealthResponse)
    async def get_health() -> HealthResponse:
        runtime = state.runtime.runtime_summary()
        model = state.runtime.model_info()
        overall = "ok" if model.loaded else "degraded"
        return HealthResponse(status=overall, runtime_version="1.0.0", runtime=runtime, model=model)

    @router.get("/models", response_model=ModelListResponse)
    async def list_models() -> ModelListResponse:
        return ModelListResponse(items=state.runtime.list_models())

    @router.post("/models/load", response_model=ModelActionResponse)
    async def load_model(request: LoadModelRequest) -> ModelActionResponse:
        try:
            model = state.runtime.load_model(request.model_path, request.inference_steps)
        except Exception as exc:
            raise HTTPException(
                status_code=400,
                detail=ErrorResponse(error=ErrorDetail(code="model_load_failed", message=str(exc))).model_dump(),
            ) from exc
        return ModelActionResponse(accepted=True, model=model)

    @router.get("/voices", response_model=VoiceListResponse)
    async def list_voices() -> VoiceListResponse:
        return VoiceListResponse(items=state.runtime.list_voices())

    @router.post("/jobs/generate", response_model=CreateJobResponse, status_code=202)
    async def create_generation_job(request: GenerateJobRequest) -> CreateJobResponse:
        try:
            job = state.jobs.create_job(request)
        except Exception as exc:
            raise HTTPException(
                status_code=400,
                detail=ErrorResponse(error=ErrorDetail(code="job_create_failed", message=str(exc))).model_dump(),
            ) from exc
        return CreateJobResponse(accepted=True, job=job)

    @router.get("/jobs/{job_id}", response_model=JobResponse)
    async def get_job(job_id: str) -> JobResponse:
        try:
            return JobResponse(job=state.jobs.get_job(job_id))
        except KeyError as exc:
            raise HTTPException(
                status_code=404,
                detail=ErrorResponse(error=ErrorDetail(code="job_not_found", message=f"Unknown job: {job_id}")).model_dump(),
            ) from exc

    @router.post("/jobs/{job_id}/stop")
    async def stop_job(job_id: str):
        try:
            return state.jobs.stop_job(job_id)
        except KeyError as exc:
            raise HTTPException(
                status_code=404,
                detail=ErrorResponse(error=ErrorDetail(code="job_not_found", message=f"Unknown job: {job_id}")).model_dump(),
            ) from exc

    @router.get("/jobs/{job_id}/artifact")
    async def get_artifact(job_id: str):
        try:
            return state.jobs.get_artifact(job_id)
        except KeyError as exc:
            raise HTTPException(
                status_code=404,
                detail=ErrorResponse(error=ErrorDetail(code="artifact_not_found", message=f"No artifact for job: {job_id}")).model_dump(),
            ) from exc

    @router.websocket("/ws/jobs/{job_id}/events")
    async def stream_job_events(websocket: WebSocket, job_id: str) -> None:
        await websocket.accept()
        try:
            async for event in state.jobs.stream_events(job_id):
                await websocket.send_json(event)
        except KeyError:
            await websocket.send_json(
                ErrorResponse(error=ErrorDetail(code="job_not_found", message=f"Unknown job: {job_id}")).model_dump()
            )
        except WebSocketDisconnect:
            return

    return router
