# Graph Report - .  (2026-04-08)

## Corpus Check
- 93 files · ~0 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1246 nodes · 2840 edges · 60 communities detected
- Extraction: 43% EXTRACTED · 57% INFERRED · 0% AMBIGUOUS · INFERRED: 1610 edges (avg confidence: 0.5)
- Token cost: 0 input · 0 output

## God Nodes (most connected - your core abstractions)
1. `VibeVoiceAcousticTokenizerConfig` - 102 edges
2. `VibeVoiceTokenizerStreamingCache` - 99 edges
3. `VibeVoiceSemanticTokenizerConfig` - 97 edges
4. `VibeVoiceTokenizerEncoderOutput` - 68 edges
5. `DPMSolverMultistepScheduler` - 68 edges
6. `VibeVoiceTextTokenizer` - 66 edges
7. `VibeVoiceTextTokenizerFast` - 66 edges
8. `VibeVoiceASRForConditionalGeneration` - 60 edges
9. `VibeVoiceAcousticTokenizerModel` - 58 edges
10. `VibeVoiceSemanticTokenizerModel` - 58 edges

## Surprising Connections (you probably didn't know these)
- `Proxy to decoder_config.num_hidden_layers (required for transformers >= 4.57).` --uses--> `VibeVoiceAcousticTokenizerConfig`  [INFERRED]
  vibevoice\modular\configuration_vibevoice_streaming.py → vibevoice\modular\configuration_vibevoice.py
- `Audio input mapper for vLLM multimodal pipeline.  This module handles audio da` --uses--> `AudioNormalizer`  [INFERRED]
  vllm_plugin\inputs.py → vibevoice\processor\audio_utils.py
- `Load and normalize audio from file path.          Args:         audio_path: P` --uses--> `AudioNormalizer`  [INFERRED]
  vllm_plugin\inputs.py → vibevoice\processor\audio_utils.py
- `Map audio input data to vLLM MultiModalInputs format.          This function i` --uses--> `AudioNormalizer`  [INFERRED]
  vllm_plugin\inputs.py → vibevoice\processor\audio_utils.py
- `VibeVoice vLLM Plugin - Registers VibeVoice model for vLLM inference.  This pl` --uses--> `VibeVoiceForCausalLM`  [INFERRED]
  vllm_plugin\__init__.py → vllm_plugin\model.py

## Communities

### Community 0 - "Community 0"
Cohesion: 0.03
Nodes (117): AudioNormalizer, Audio normalization class for VibeVoice tokenizer.          This class provide, Initialize the audio normalizer.                  Args:             target_dB, Adjust the audio to the target dB FS level.                  Args:, Avoid clipping by scaling down if necessary.                  Args:, Normalize the audio by adjusting to target dB FS and avoiding clipping., BaseProcessingInfo, VibeVoiceAcousticTokenizerConfig (+109 more)

### Community 1 - "Community 1"
Cohesion: 0.04
Nodes (92): BaseModelOutputWithPast, BaseStreamer, ConfigMixin, VibeVoice Streaming model configuration, VibeVoiceStreamingConfig, VibeVoiceConfig, DPMSolverMultistepScheduler, `DPMSolverMultistepScheduler` is a fast dedicated high-order solver for diffusio (+84 more)

### Community 2 - "Community 2"
Cohesion: 0.03
Nodes (81): FeatureExtractionMixin, main(), parse_args(), parse_txt_script(), Maps speaker names to voice file paths, Setup voice presets by scanning the voices directory., Get voice file path for a given speaker name, Parse txt script content and extract speakers and their text     Fixed pattern: (+73 more)

### Community 3 - "Community 3"
Cohesion: 0.03
Nodes (72): Dataset, load_lora_model(), main(), Load base model and merge with LoRA weights.          Args:         base_mode, Transcribe an audio file using the LoRA fine-tuned model.          Args:, transcribe(), DataArguments, get_lora_config() (+64 more)

### Community 4 - "Community 4"
Cohesion: 0.07
Nodes (33): AppSettings, AppState, BaseModel, ArtifactResponse, CreateJobResponse, ErrorDetail, ErrorResponse, GenerateJobRequest (+25 more)

### Community 5 - "Community 5"
Cohesion: 0.05
Nodes (50): clip_and_encode_audio(), convert_audio_to_mp3(), convert_video_to_mp4(), create_gradio_interface(), download_cloudflared(), extract_audio_from_video(), extract_audio_segments(), format_srt_time() (+42 more)

### Community 6 - "Community 6"
Cohesion: 0.06
Nodes (20): GenerationMixin, convert_to_16_bit_wav(), create_demo_interface(), main(), parse_args(), VibeVoice Gradio Demo - High-Quality Dialogue Generation Interface with Streamin, Initialize the VibeVoice demo with model loading., Load example scripts from the text_examples directory. (+12 more)

### Community 7 - "Community 7"
Cohesion: 0.06
Nodes (11): AudioPlaybackService, IAudioPlaybackService, BaseViewModel, BaseViewModel, IDisposable, INotifyPropertyChanged, MainViewModel, DefaultProcessStarter (+3 more)

### Community 8 - "Community 8"
Cohesion: 0.04
Nodes (17): IArtifactExportService, IAudioPlaybackService, IDesktopSettingsService, IProcessStarter, IRuntimeApiClient, IRuntimeEventStreamClient, IRuntimeHostService, FakeArtifactExportService (+9 more)

### Community 9 - "Community 9"
Cohesion: 0.08
Nodes (21): VibeVoice_AcousticTokenizer model configuration, Returns the decoder config (required for transformers >= 4.57 cache compatibilit, Proxy to decoder_config.num_hidden_layers (required for transformers >= 4.57)., Override to_dict to handle torch.dtype serialization.                  Fixes:, VibeVoiceDiffusionHeadConfig, FeedForwardNetwork, FinalLayer, HeadLayer (+13 more)

### Community 10 - "Community 10"
Cohesion: 0.1
Nodes (22): _extract_audio_from_video(), _find_last_segment_boundary(), _find_safe_print_boundary(), _get_duration_seconds_ffprobe(), _guess_mime_type(), _is_video_file(), main(), Find the position after the last complete segment boundary (},).     Returns -1 (+14 more)

### Community 11 - "Community 11"
Cohesion: 0.08
Nodes (10): AsyncAudioBatchIterator, AudioBatchIterator, AudioSampleIterator, Iterator that yields audio chunks for all samples in the batch., Returns an async iterator over all audio streams., Async iterator for batch audio streaming., Helper to get a chunk from a specific queue., Returns an iterator over the batch of audio streams. (+2 more)

### Community 12 - "Community 12"
Cohesion: 0.15
Nodes (21): _build_vllm_cmd(), download_model(), generate_tokenizer(), _install_nginx(), install_system_deps(), install_vibevoice(), main(), Start a single vLLM server (replaces current process). (+13 more)

### Community 13 - "Community 13"
Cohesion: 0.15
Nodes (19): compare_json_files(), compare_text_files(), compare_with_reference(), download_qwen_tokenizer_files(), generate_added_tokens_json(), generate_special_tokens_map_json(), generate_vibevoice_tokenizer_files(), main() (+11 more)

### Community 14 - "Community 14"
Cohesion: 0.14
Nodes (2): IRuntimeApiClient, RuntimeApiClient

### Community 15 - "Community 15"
Cohesion: 0.19
Nodes (5): get_timestamp(), _startup(), streaming_tts(), StreamingTTSService, websocket_stream()

### Community 16 - "Community 16"
Cohesion: 0.12
Nodes (16): ArtifactResponse, CreateJobResponse, GenerateJobRequest, HealthResponse, JobActionResponse, JobInfo, JobResponse, LoadModelRequest (+8 more)

### Community 17 - "Community 17"
Cohesion: 0.17
Nodes (4): convert_vibevoice_nnscaler_checkpoint_to_hf(), main(), Convert a nnscaler VibeVoice checkpoint to HuggingFace format.     Supports bot, VibeVoiceForConditionalGeneration

### Community 18 - "Community 18"
Cohesion: 0.31
Nodes (9): choose_folder(), extract_first_json(), fetch_page(), main(), run_codex(), safe_filename(), update_processed(), verify_samples() (+1 more)

### Community 19 - "Community 19"
Cohesion: 0.24
Nodes (11): _extract_audio_from_video(), _get_duration_seconds_ffprobe(), _guess_mime_type(), _is_video_file(), main(), Guess MIME type from file extension., Get audio duration using ffprobe., Check if the file is a video file that needs audio extraction. (+3 more)

### Community 20 - "Community 20"
Cohesion: 0.27
Nodes (3): DesktopSettings, DesktopSettingsService, IDesktopSettingsService

### Community 21 - "Community 21"
Cohesion: 0.28
Nodes (8): _get_ffmpeg_max_concurrency(), load_audio_bytes_use_ffmpeg(), load_audio_use_ffmpeg(), Decode audio bytes via ffmpeg stdin pipe.      Compared to writing bytes to a, Open an audio file and read as mono waveform, optionally resampling.     Return, Get the maximum FFmpeg concurrency from environment variable., Run ffmpeg with optional global concurrency limiting.      This is important f, _run_ffmpeg()

### Community 22 - "Community 22"
Cohesion: 0.28
Nodes (6): betas_for_alpha_bar(), __init__(), Create a beta schedule that discretizes the given alpha_t_bar function, which de, # TODO: Add this logic to the other schedulers, Rescales betas to have zero terminal SNR Based on https://arxiv.org/pdf/2305.088, rescale_zero_terminal_snr()

### Community 23 - "Community 23"
Cohesion: 0.29
Nodes (2): LogitNormalSampler, UniformSampler

### Community 24 - "Community 24"
Cohesion: 0.47
Nodes (2): AsyncRelayCommand, ICommand

### Community 25 - "Community 25"
Cohesion: 0.4
Nodes (5): load_audio(), Audio input mapper for vLLM multimodal pipeline.  This module handles audio da, Load and normalize audio from file path.          Args:         audio_path: P, Map audio input data to vLLM MultiModalInputs format.          This function i, vibevoice_audio_input_mapper()

### Community 26 - "Community 26"
Cohesion: 0.5
Nodes (2): App, VibeVoice.Desktop

### Community 27 - "Community 27"
Cohesion: 0.4
Nodes (2): MainWindow, VibeVoice.Desktop

### Community 28 - "Community 28"
Cohesion: 0.5
Nodes (2): ArtifactExportService, IArtifactExportService

### Community 29 - "Community 29"
Cohesion: 0.5
Nodes (2): IRuntimeEventStreamClient, RuntimeEventStreamClient

### Community 30 - "Community 30"
Cohesion: 0.67
Nodes (2): App, Application

### Community 31 - "Community 31"
Cohesion: 0.67
Nodes (2): MainWindow, Window

### Community 32 - "Community 32"
Cohesion: 0.67
Nodes (1): RepositoryRootLocator

### Community 33 - "Community 33"
Cohesion: 0.67
Nodes (1): ArtifactExportServiceTests

### Community 34 - "Community 34"
Cohesion: 0.67
Nodes (1): DesktopSettingsServiceTests

### Community 35 - "Community 35"
Cohesion: 0.67
Nodes (0): 

### Community 36 - "Community 36"
Cohesion: 1.0
Nodes (2): Invoke-CodexJson(), Parse-CodexObject()

### Community 37 - "Community 37"
Cohesion: 1.0
Nodes (0): 

### Community 38 - "Community 38"
Cohesion: 1.0
Nodes (1): RuntimeLaunchOptions

### Community 39 - "Community 39"
Cohesion: 1.0
Nodes (0): 

### Community 40 - "Community 40"
Cohesion: 1.0
Nodes (0): 

### Community 41 - "Community 41"
Cohesion: 1.0
Nodes (0): 

### Community 42 - "Community 42"
Cohesion: 1.0
Nodes (0): 

### Community 43 - "Community 43"
Cohesion: 1.0
Nodes (0): 

### Community 44 - "Community 44"
Cohesion: 1.0
Nodes (0): 

### Community 45 - "Community 45"
Cohesion: 1.0
Nodes (0): 

### Community 46 - "Community 46"
Cohesion: 1.0
Nodes (0): 

### Community 47 - "Community 47"
Cohesion: 1.0
Nodes (1): Id of the end of sequence token.

### Community 48 - "Community 48"
Cohesion: 1.0
Nodes (1): Id of the speech start token.

### Community 49 - "Community 49"
Cohesion: 1.0
Nodes (1): Id of the speech end token.

### Community 50 - "Community 50"
Cohesion: 1.0
Nodes (1): Id of the speech diffusion token.

### Community 51 - "Community 51"
Cohesion: 1.0
Nodes (1): Id used for padding (returns -100 for loss masking).

### Community 52 - "Community 52"
Cohesion: 1.0
Nodes (1): Id of the end of sequence token.

### Community 53 - "Community 53"
Cohesion: 1.0
Nodes (1): Id of the speech start token.

### Community 54 - "Community 54"
Cohesion: 1.0
Nodes (1): Id of the speech end token.

### Community 55 - "Community 55"
Cohesion: 1.0
Nodes (1): Id of the speech diffusion token.

### Community 56 - "Community 56"
Cohesion: 1.0
Nodes (1): Id used for padding (returns -100 for loss masking).

### Community 57 - "Community 57"
Cohesion: 1.0
Nodes (1): The index counter for current timestep. It will increase 1 after each scheduler

### Community 58 - "Community 58"
Cohesion: 1.0
Nodes (1): The index for the first timestep. It should be set from pipeline with `set_begin

### Community 59 - "Community 59"
Cohesion: 1.0
Nodes (1): Get the model name (auto-detected if not specified).

## Knowledge Gaps
- **159 isolated node(s):** `HealthResponse`, `RuntimeInfo`, `ModelInfo`, `LoadModelRequest`, `ModelActionResponse` (+154 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Community 37`** (2 nodes): `routes.py`, `build_router()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 38`** (2 nodes): `RuntimeLaunchOptions.cs`, `RuntimeLaunchOptions`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 39`** (2 nodes): `main.py`, `create_app()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 40`** (2 nodes): `vibevoice_realtime_demo.py`, `main()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 41`** (1 nodes): `AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 42`** (1 nodes): `VibeVoice.Desktop.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 43`** (1 nodes): `VibeVoice.Desktop.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 44`** (1 nodes): `VibeVoice.Desktop.Tests.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 45`** (1 nodes): `VibeVoice.Desktop.Tests.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 46`** (1 nodes): `debug_mark_one.ps1`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 47`** (1 nodes): `Id of the end of sequence token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 48`** (1 nodes): `Id of the speech start token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 49`** (1 nodes): `Id of the speech end token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 50`** (1 nodes): `Id of the speech diffusion token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 51`** (1 nodes): `Id used for padding (returns -100 for loss masking).`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 52`** (1 nodes): `Id of the end of sequence token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 53`** (1 nodes): `Id of the speech start token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 54`** (1 nodes): `Id of the speech end token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 55`** (1 nodes): `Id of the speech diffusion token.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 56`** (1 nodes): `Id used for padding (returns -100 for loss masking).`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 57`** (1 nodes): `The index counter for current timestep. It will increase 1 after each scheduler`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 58`** (1 nodes): `The index for the first timestep. It should be set from pipeline with `set_begin`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 59`** (1 nodes): `Get the model name (auto-detected if not specified).`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `VibeVoiceTokenizerStreamingCache` connect `Community 0` to `Community 1`, `Community 3`, `Community 17`, `Community 6`?**
  _High betweenness centrality (0.111) - this node is a cross-community bridge._
- **Why does `VibeVoiceForConditionalGenerationInference` connect `Community 6` to `Community 0`, `Community 1`, `Community 2`, `Community 4`?**
  _High betweenness centrality (0.091) - this node is a cross-community bridge._
- **Why does `VibeVoiceASRForConditionalGeneration` connect `Community 3` to `Community 0`, `Community 9`, `Community 6`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **Are the 99 inferred relationships involving `VibeVoiceAcousticTokenizerConfig` (e.g. with `VibeVoiceStreamingConfig` and `VibeVoice Streaming model configuration`) actually correct?**
  _`VibeVoiceAcousticTokenizerConfig` has 99 INFERRED edges - model-reasoned connections that need verification._
- **Are the 92 inferred relationships involving `VibeVoiceTokenizerStreamingCache` (e.g. with `VibeVoiceCausalLMOutputWithPast` and `VibeVoiceGenerationOutput`) actually correct?**
  _`VibeVoiceTokenizerStreamingCache` has 92 INFERRED edges - model-reasoned connections that need verification._
- **Are the 94 inferred relationships involving `VibeVoiceSemanticTokenizerConfig` (e.g. with `ConvLayerNorm` and `RMSNorm`) actually correct?**
  _`VibeVoiceSemanticTokenizerConfig` has 94 INFERRED edges - model-reasoned connections that need verification._
- **Are the 63 inferred relationships involving `VibeVoiceTokenizerEncoderOutput` (e.g. with `encode()` and `VibeVoiceASRPreTrainedModel`) actually correct?**
  _`VibeVoiceTokenizerEncoderOutput` has 63 INFERRED edges - model-reasoned connections that need verification._