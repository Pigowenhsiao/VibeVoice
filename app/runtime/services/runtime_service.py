from __future__ import annotations

import re
import shutil
import sys
import threading
from dataclasses import dataclass
from pathlib import Path
from typing import Callable

import numpy as np
import soundfile as sf
from scipy.signal import resample_poly

from app.runtime.schemas.contracts import GenerateJobRequest, ModelInfo, VoicePreset


class GenerationCancelled(Exception):
    pass


@dataclass(slots=True)
class RuntimeGenerationResult:
    artifact_path: Path
    duration_seconds: float


class VibeVoiceRuntime:
    def __init__(self, settings) -> None:
        self.settings = settings
        self._lock = threading.RLock()
        self._model = None
        self._processor = None
        self._model_path: Path | None = None
        self._device = "uninitialized"
        self._status = "unloaded"
        self._message = "Model not loaded."
        self._inference_steps = 10

    def runtime_summary(self) -> dict[str, object]:
        return {
            "device": self._detect_device_name(),
            "python_version": sys.version.split()[0],
            "ffmpeg_available": shutil.which("ffmpeg") is not None,
            "checkpoints_root": str(self.settings.checkpoints_root),
        }

    def model_info(self) -> ModelInfo:
        loaded = self._model is not None and self._processor is not None
        return ModelInfo(
            id=self._model_path.name if self._model_path else None,
            path=str(self._model_path) if self._model_path else None,
            loaded=loaded,
            status=self._status,
            device=self._device,
            message=self._message,
        )

    def list_models(self) -> list[ModelInfo]:
        info = self.model_info()
        candidates: list[ModelInfo] = []
        if self.settings.checkpoints_root.exists():
            candidates.append(
                ModelInfo(
                    id=self.settings.checkpoints_root.name,
                    path=str(self.settings.checkpoints_root),
                    loaded=info.loaded and info.path == str(self.settings.checkpoints_root),
                    status=info.status if info.path == str(self.settings.checkpoints_root) else "unloaded",
                    device=info.device if info.path == str(self.settings.checkpoints_root) else None,
                    message=info.message if info.path == str(self.settings.checkpoints_root) else "Local checkpoint directory detected.",
                )
            )
        if info.path and all(item.path != info.path for item in candidates):
            candidates.append(info)
        return candidates or [info]

    def list_voices(self) -> list[VoicePreset]:
        if not self.settings.voices_dir.exists():
            return []
        items: list[VoicePreset] = []
        for path in sorted(self.settings.voices_dir.iterdir()):
            if path.suffix.lower() not in {".wav", ".mp3", ".flac", ".ogg", ".m4a", ".aac"}:
                continue
            items.append(
                VoicePreset(
                    id=path.stem,
                    display_name=path.stem,
                    path=str(path),
                    language_hint=self._infer_language(path.stem),
                )
            )
        return items

    def load_model(self, model_path: str, inference_steps: int = 10) -> ModelInfo:
        with self._lock:
            self._status = "loading"
            self._message = "Loading model."
            path = Path(model_path).expanduser().resolve()
            if not path.exists():
                self._status = "failed"
                self._message = f"Model path does not exist: {path}"
                raise FileNotFoundError(self._message)

            import torch
            from vibevoice.modular.modeling_vibevoice_inference import (
                VibeVoiceForConditionalGenerationInference,
            )
            from vibevoice.processor.vibevoice_processor import VibeVoiceProcessor

            device = "cuda" if torch.cuda.is_available() else "cpu"
            model_kwargs: dict[str, object] = {}
            if device == "cuda":
                model_kwargs["torch_dtype"] = torch.bfloat16
                model_kwargs["device_map"] = "cuda"
                model_kwargs["attn_implementation"] = "flash_attention_2"
            else:
                model_kwargs["torch_dtype"] = torch.float32

            processor = VibeVoiceProcessor.from_pretrained(str(path))
            model = VibeVoiceForConditionalGenerationInference.from_pretrained(str(path), **model_kwargs)
            if device != "cuda":
                model = model.to(device)
            model.eval()
            model.model.noise_scheduler = model.model.noise_scheduler.from_config(
                model.model.noise_scheduler.config,
                algorithm_type="sde-dpmsolver++",
                beta_schedule="squaredcos_cap_v2",
            )
            model.set_ddpm_inference_steps(num_steps=inference_steps)

            self._processor = processor
            self._model = model
            self._model_path = path
            self._device = device
            self._status = "loaded"
            self._message = f"Model loaded from {path.name}"
            self._inference_steps = inference_steps
            return self.model_info()

    def generate(
        self,
        request: GenerateJobRequest,
        job_id: str,
        cancel_event: threading.Event,
        on_progress: Callable[[float, str], None],
    ) -> RuntimeGenerationResult:
        with self._lock:
            if self._model is None or self._processor is None:
                raise RuntimeError("Model is not loaded.")

            selected_speakers = request.speakers[: request.speaker_count]
            voices_by_id = {voice.id: voice for voice in self.list_voices()}
            missing = [speaker.voice_id for speaker in selected_speakers if speaker.voice_id not in voices_by_id]
            if missing:
                raise ValueError(f"Unknown voice preset(s): {', '.join(missing)}")

            on_progress(0.1, "Loading voice presets.")
            voice_samples = [self._read_audio(voices_by_id[speaker.voice_id].path) for speaker in selected_speakers]
            if any(sample.size == 0 for sample in voice_samples):
                raise RuntimeError("At least one selected voice sample could not be loaded.")

            if cancel_event.is_set():
                raise GenerationCancelled("Job cancelled before inference.")

            on_progress(0.25, "Formatting script.")
            formatted_script = self._format_script(request.script, request.speaker_count)

            on_progress(0.4, "Preparing model inputs.")
            inputs = self._processor(
                text=[formatted_script],
                voice_samples=[voice_samples],
                padding=True,
                return_tensors="pt",
                return_attention_mask=True,
            )

            def stop_check() -> bool:
                return cancel_event.is_set()

            on_progress(0.55, "Generating audio. This can take a while.")
            outputs = self._model.generate(
                **inputs,
                max_new_tokens=None,
                cfg_scale=request.cfg_scale,
                tokenizer=self._processor.tokenizer,
                generation_config={"do_sample": False},
                stop_check_fn=stop_check,
                verbose=False,
                refresh_negative=True,
            )

            if cancel_event.is_set():
                raise GenerationCancelled("Job cancelled during inference.")

            on_progress(0.9, "Saving artifact.")
            speech_output = outputs.speech_outputs[0] if outputs.speech_outputs else None
            if speech_output is None:
                raise RuntimeError("The model returned no audio output.")

            audio_np = speech_output.detach().cpu().numpy()
            if audio_np.ndim > 1:
                audio_np = np.squeeze(audio_np)

            output_root = Path(request.output_dir).expanduser().resolve() if request.output_dir else self.settings.artifacts_root
            output_root.mkdir(parents=True, exist_ok=True)
            artifact_path = output_root / f"{job_id}.wav"
            sf.write(artifact_path, audio_np, 24000)

            on_progress(1.0, "Generation complete.")
            duration_seconds = float(len(audio_np) / 24000.0)
            return RuntimeGenerationResult(artifact_path=artifact_path, duration_seconds=duration_seconds)

    def _read_audio(self, audio_path: str, target_sr: int = 24000) -> np.ndarray:
        wav, sr = sf.read(audio_path)
        if wav.ndim > 1:
            wav = np.mean(wav, axis=1)
        if sr != target_sr:
            gcd = np.gcd(sr, target_sr)
            wav = resample_poly(wav, target_sr // gcd, sr // gcd)
        return wav.astype(np.float32, copy=False)

    def _format_script(self, script: str, speaker_count: int) -> str:
        script = script.replace("’", "'").strip()
        lines = [line.strip() for line in script.splitlines() if line.strip()]
        formatted: list[str] = []
        for line in lines:
            if re.match(r"^Speaker\s+\d+\s*:", line, re.IGNORECASE):
                formatted.append(line)
            else:
                speaker_id = len(formatted) % speaker_count
                formatted.append(f"Speaker {speaker_id}: {line}")
        return "\n".join(formatted)

    def _infer_language(self, name: str) -> str | None:
        lowered = name.lower()
        if lowered.startswith("en-"):
            return "en"
        if lowered.startswith("zh-"):
            return "zh"
        if lowered.startswith("in-"):
            return "id"
        return None

    def _detect_device_name(self) -> str:
        try:
            import torch

            if torch.cuda.is_available():
                return f"cuda:{torch.cuda.get_device_name(0)}"
            return "cpu"
        except Exception:
            return "unknown"
