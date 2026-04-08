# Windows App Design Rules

## Purpose

This document defines the baseline architecture and collaboration rules for turning VibeVoice into a Windows desktop application.

The goal is not to copy the implementation details of `const-me/whisper`, but to adopt the same product shape:

- a native-feeling Windows desktop app
- a local inference engine
- clear boundaries between UI, orchestration, and model code
- predictable packaging and future maintainability

This document is the default reference for future implementation unless a later ADR or approved spec explicitly replaces part of it.

## Product Direction

The target product is a Windows desktop application for local long-form multi-speaker speech generation.

The default architecture is:

- Desktop shell: `WPF (.NET 8)`
- Local inference host: `embedded Python worker`
- Model core: existing `vibevoice/` package
- Process boundary: `desktop -> worker`, no loopback server

This is the default choice because it matches the desired Windows application experience while preserving the existing Python + PyTorch model stack.
The user must not need to manually start any separate runtime process before opening the desktop app.

## Current Baseline

The current implemented baseline in this repository is:

- desktop shell: `app/desktop/VibeVoice.Desktop`
- local runtime host: `app/runtime`
- temporary compatibility alias: `app/backend`
- contract document: `docs/contracts/local-runtime-api-v1.yaml`
- desktop event client: `app/desktop/VibeVoice.Desktop/Services/RuntimeEventStreamClient.cs`
- desktop playback/export/settings services:
  - `AudioPlaybackService`
  - `ArtifactExportService`
  - `DesktopSettingsService`

Important interpretation:

- `app/runtime` is the primary implementation path
- `app/backend` may exist temporarily to keep older local entrypoints working
- new work must target `app/runtime`, not extend `app/backend`
- the desktop app currently manages a loopback HTTP runtime internally for development, but users still experience one application
- the desktop shell now consumes typed runtime job events for progress and artifact readiness
- the desktop shell now exposes `Result / Export` and `Settings` inside the primary native window

## Non-Goals

These items are explicitly out of scope for the initial architecture:

- rewriting inference core in C++ or Rust
- embedding PyTorch logic directly into the WPF process
- keeping Gradio as the production desktop UI
- building cross-platform support before Windows V1 is stable
- adding cloud inference as a hard dependency

## Architecture Rules

### Rule 1: UI and inference must be separate runtimes

The desktop UI must not load model weights or import PyTorch directly into the WPF process.

Why:

- prevents UI freezes and memory pressure inside the desktop process
- simplifies crash isolation
- keeps the Windows shell responsive

The desktop app may start an internal worker automatically, but users must experience this as one application, not as "open app A, then separately launch app B".

### Rule 2: `vibevoice/` remains model-core only

The existing `vibevoice/` package is the model and processing core.

Allowed responsibilities:

- model loading
- tokenization
- processor logic
- inference
- audio streaming helpers

Forbidden responsibilities:

- desktop UI code
- HTTP route definitions tied to UI concerns
- installer logic
- app settings window logic

### Rule 3: local inference host owns orchestration

The local inference host is the application boundary around the model core.

It is responsible for:

- loading and unloading models
- enumerating voice presets
- validating generation requests
- formatting scripts into speaker turns
- creating and tracking generation jobs
- handling stop/cancel requests
- producing progress and streaming events
- exporting final audio artifacts

It is not exposed as a user-operated server.

### Rule 4: desktop app owns user interaction only

The desktop shell is responsible for:

- navigation
- form input
- local settings UI
- progress display
- audio playback
- download/export interactions
- user-facing error messages

The desktop shell must not contain model business logic.

### Rule 5: worker contract is a stable product boundary

All communication between the desktop shell and the inference host must go through a versioned local contract.

Preferred transport for V1:

- desktop-managed local runtime process
- typed local contract
- no user-visible HTTP server

Current implementation note:

- the current baseline uses a desktop-managed loopback HTTP runtime for local development and vertical-slice verification
- this is acceptable as an implementation detail as long as the user never manually launches or configures it
- a later change to named pipes, stdio, or direct process invocation is allowed if the contract stays stable

This contract must be treated as a public internal API. UI rewrites should not require runtime rewrites if the contract is stable.

### Rule 5.1: contract naming must reflect runtime ownership

The contract and payload naming must use `runtime`, not `backend`, for the primary architecture.

Current baseline:

- contract file: `docs/contracts/local-runtime-api-v1.yaml`
- health response field: `runtime_version`
- runtime package entrypoint: `app.runtime.main`

If compatibility aliases still exist, they must not become the naming source of truth.

### Rule 6: desktop app owns worker startup lifecycle

The desktop app must fully own the worker lifecycle.

Required behavior:

- worker startup is automatic and internal to the desktop app
- the user never needs to launch, attach to, or configure a separate server process
- the app may initialize the worker on app launch or on first model action, but this must appear as a normal app loading state
- if worker startup fails, the desktop app must show a user-facing initialization error and recovery action
- if the worker crashes during normal operation, the desktop app must handle restart or failure messaging without exposing raw console/process management steps

Forbidden behavior:

- requiring a terminal command before the app becomes usable
- showing a "connect to runtime" step in the primary workflow
- treating worker lifecycle as an operator task instead of an application responsibility

## Recommended Repository Layout

```text
docs/
  design-rules/
    windows-app-design-rules.md
  contracts/
    local-runtime-api-v1.yaml

app/
  desktop/
    VibeVoice.Desktop/
      Views/
      ViewModels/
      Services/
      Models/
      Assets/
    VibeVoice.Desktop.Tests/
  runtime/
    main.py
    api/
    services/
    schemas/
    adapters/
    tests/
  backend/
    ...compatibility alias only

vibevoice/
  modular/
  processor/
  schedule/
  scripts/

checkpoints/
demo/
```

## Runtime Design Rules

### Service boundaries

The local inference host should be split into focused modules:

- `main.py`: process entrypoint only
- `api/`: HTTP or IPC edge for the current runtime host
- `schemas/`: request and response contracts
- `services/`: model orchestration and domain logic
- `workers/`: job execution and streaming loops
- `adapters/`: filesystem, ffmpeg, device, checkpoint, and external runtime integration

### Required runtime commands

Minimum V1 commands:

- `health`
- `load_model`
- `list_voices`
- `generate`
- `stop`
- `get_job`
- `get_artifact`

These commands are currently surfaced through the runtime contract defined in:

- `docs/contracts/local-runtime-api-v1.yaml`

### Job model

Generation must run as an explicit job with lifecycle states:

- `queued`
- `loading`
- `running`
- `stopping`
- `completed`
- `failed`
- `cancelled`

No generation request should be treated as a fire-and-forget function call from the UI.

### Streaming rule

Audio streaming and progress streaming must be emitted as typed events, not ad hoc text blobs.

Minimum event types:

- `job_state_changed`
- `progress_updated`
- `audio_chunk`
- `artifact_ready`
- `warning`
- `error`

## Desktop Design Rules

### UI composition

The desktop app should use MVVM.

Required layers:

- `Views`: XAML screens only
- `ViewModels`: UI state and command orchestration
- `Services`: worker client, playback manager, settings, file picker, notifications
- `Models`: UI-facing DTOs only

Current baseline implementation:

- runtime startup orchestration lives in desktop `Services`
- runtime event streaming lives in desktop `Services`
- runtime job state orchestration lives in desktop `ViewModels`
- playback, export, and settings persistence live in dedicated desktop `Services`
- WPF window copy should describe `runtime`, not `backend`

### Required V1 screens

- `Load Model`
- `Generation Workspace`
- `Job Progress`
- `Result / Export`
- `Settings`

### UX rule

The app should follow the workflow shape inspired by `const-me/whisper`:

- start from model readiness
- then move to task configuration
- then run the job with visible progress
- then expose the final result clearly

Do not force users into a developer-style console workflow.
Do not force users to launch a server before opening the app.

### Reference workflow contract

The primary desktop flow must follow this order:

1. `Load Model`
2. `Generation Workspace`
3. `Job Progress`
4. `Result / Export`

Required interpretation:

- the initial landing screen must be `Load Model`
- `Settings` is a secondary screen, not the first-run primary workflow
- model readiness must gate the generation workspace
- active jobs must surface a dedicated progress state with visible stop or cancel controls
- successful completion must lead to a clear result/export state

Forbidden primary-flow screens:

- server connection pages
- API endpoint configuration pages
- developer console setup pages
- any startup flow that implies the user is operating two separate applications

## Migration Rules For Existing Code

`demo/gradio_demo.py` is a useful prototype, but it is not the production architecture.

It should be mined and split, not wrapped as-is.

Expected extraction targets:

- voice preset scanning -> runtime service
- generation workflow -> runtime worker/service
- script normalization -> runtime service
- stream event production -> runtime job/event layer
- UI wording and input defaults -> desktop shell

No new product feature should be added directly into `demo/gradio_demo.py` once desktop implementation starts.

### Runtime package migration rule

The production runtime path is `app/runtime`.

Migration guidance:

- move new runtime logic into `app/runtime`
- keep `app/backend` only as a temporary compatibility alias
- do not introduce new primary modules under `app/backend`
- remove the compatibility alias only after desktop startup, tests, and local scripts no longer depend on it

## Configuration Rules

Configuration must be separated into three groups:

- model configuration
- runtime configuration
- user preferences

Recommended ownership:

- runtime host owns model path, CUDA/runtime, ffmpeg/runtime dependencies
- desktop owns window layout, recent files, last-used UI settings
- shared contract defines only values needed across the boundary

## Error Handling Rules

- runtime host returns structured errors with machine-readable codes
- desktop maps runtime errors into user-readable messages
- raw Python stack traces must not be shown directly in normal UI flows
- stoppable jobs must be safely cancellable
- model loading errors must be isolated from generation job errors

## Packaging Rules

- runtime host must be bundled as part of the desktop app
- desktop installer must bundle and supervise the worker internally
- worker launch must happen automatically on app launch or first model interaction
- no manual worker startup step is allowed
- no external network requirement is allowed for normal local generation after setup

## Testing Rules

### Runtime

- unit tests for request validation and script parsing
- integration tests for model load and job lifecycle
- smoke test for generate -> progress -> artifact flow
- smoke test for `/health` and `/voices` through `app/runtime/tests/test_smoke.py`

### Desktop

- ViewModel tests for screen state transitions
- contract tests against runtime schemas
- manual QA checklist for install, load model, generate, stop, export
- test runtime auto-start behavior from the desktop shell
- verify that closing the desktop app also shuts down the managed runtime when the app launched it

### QA validation note

Current validation approach may combine:

- Python smoke tests
- .NET build and desktop unit tests
- browser-based verification of the runtime `/docs` and `/health` endpoints
- desktop ViewModel tests that verify typed event consumption, playback enablement, export enablement, and settings persistence service behavior

This is acceptable for the current stage because the runtime is locally observable while the user-facing product remains a single desktop app.

## Decision On OpenSpec

OpenSpec is optional here, not required.

Recommended rule:

- use this document as the architecture baseline now
- use lightweight specs for feature-level work
- introduce a heavier OpenSpec-style workflow only if the project later needs strict multi-document governance, multiple contributors, or frequent architecture reviews

For the current stage, the right approach is:

- keep one stable architecture rule document
- write focused feature specs when needed
- avoid adding a formal framework that slows down delivery before the product shell exists

## Default Decision

Until replaced by an approved later spec, the team should proceed with:

- `WPF (.NET 8)` for desktop UI
- `embedded Python worker` for orchestration
- existing `vibevoice/` package as model core
- direct desktop-to-worker contract as process boundary

This is the default path for implementation and future planning.
