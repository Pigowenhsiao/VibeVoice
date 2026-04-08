# Windows App V1 Feature Spec

## Goal

Build the first usable Windows desktop version of VibeVoice for local multi-speaker audio generation.

V1 is not a research demo. V1 is a product shell that can:

- load a local model
- let users configure a generation job
- show progress while generation runs
- stop a running job
- play and export the generated result

## Product Scope

### In Scope

- Windows desktop application
- internal inference runtime managed by the desktop app
- local model loading from checkpoint directory or configured path
- speaker selection from available preset voices
- script input for 1 to 4 speakers
- generation job creation and progress tracking
- stop or cancel a running job
- final audio playback and export
- clear user-facing errors for common setup and generation failures

### Out of Scope

- cloud inference
- user account system
- collaborative projects
- timeline-based audio editing
- advanced transcript editor
- waveform editing
- overlapping speech authoring
- automatic voice cloning upload flow
- direct Gradio production deployment

## Primary Users

### User Type 1: local creator

This user wants to generate long-form audio locally on a Windows machine without touching Python scripts.

### User Type 2: technical operator

This user is comfortable with model files and runtime setup, but still wants a desktop workflow instead of CLI commands.

## User Problems

- current usage is tied to Python scripts or Gradio demo flow
- model readiness is not surfaced as a product step
- generation lifecycle is hard to manage from a desktop workflow
- stop and progress behavior are tied to demo code instead of a stable app contract
- there is no installable Windows application shell yet
- a user should not need to manually start a separate runtime process before using the app

## V1 User Journey

### Flow

1. Launch desktop app
2. App checks internal runtime readiness
3. User selects or confirms model path
4. User sees available voice presets
5. User enters script and generation settings
6. User starts generation
7. App shows live job progress and allows stop
8. App shows completed audio result
9. User plays or exports the artifact

## V1 Screens

### 1. Load Model

Purpose:

- confirm required runtime dependencies are available
- show whether model is loaded

Must show:

- device summary
- model loaded or not loaded
- clear next action

### 2. Generation Workspace

Purpose:

- collect all inputs required for generation

Must include:

- number of speakers
- per-speaker voice selection
- script editor
- generation parameters exposed to users in V1
- generate action

### 3. Job Progress

Purpose:

- make job state visible and trustworthy

Must show:

- current job state
- progress message
- elapsed time
- stop action
- warnings if runtime reports degraded behavior

### 4. Result / Export

Purpose:

- let user consume the generated result

Must include:

- final audio playback
- artifact path or export action
- regenerate entry point
- visible failure state if generation did not complete

### 5. Settings

Purpose:

- hold desktop-local preferences and non-model global options

V1 scope:

- runtime diagnostics if needed
- output directory
- last-used defaults

## Functional Requirements

### FR-1 Runtime health

The desktop app must verify internal runtime readiness at startup without asking the user to launch another process manually.

### FR-2 Model lifecycle

The user must be able to load a model before starting generation.

### FR-3 Voice preset discovery

The system must list available voice presets from runtime-managed sources.

### FR-4 Script validation

The system must reject empty scripts and invalid speaker selections before job submission.

### FR-5 Generation jobs

Each generation request must create a tracked job with a stable identifier.

### FR-6 Progress events

The desktop app must receive typed progress updates during a running job.

### FR-7 Stop action

The user must be able to request stop during generation.

### FR-8 Final artifact

Completed jobs must produce a retrievable audio artifact.

### FR-9 Error reporting

The system must surface structured errors for:

- runtime unavailable
- model load failure
- invalid request
- runtime generation failure
- artifact retrieval failure

## Non-Functional Requirements

### NFR-1 Process isolation

Desktop UI and model inference must run in separate processes.

### NFR-2 Local-only V1

All V1 interactions must work on the local machine without requiring a remote service after setup.

### NFR-3 Recoverable UX

If a generation job fails, the app must remain usable without restart whenever possible.

### NFR-4 Single-app experience

The user experience must be a single desktop application. Any internal worker process must be launched and managed by the desktop app automatically.

### NFR-5 Stable contract

Desktop and runtime integration must depend on a versioned local worker contract.

### NFR-6 Maintainable boundaries

No new product feature may be implemented by extending `demo/gradio_demo.py` directly once desktop work begins.

## V1 Exposed User Parameters

V1 should expose only parameters that materially affect outcomes and are understandable in UI.

Default exposed parameters:

- speaker count
- selected voices
- script input
- guidance scale

Hidden from V1 user surface unless later justified:

- low-level scheduler tuning
- internal tensor/runtime flags
- debug-only controls

## Runtime Responsibilities

- runtime readiness checks
- model load and unload
- voice preset enumeration
- request validation
- job creation
- generation execution
- stop handling
- event streaming
- final artifact registration

## Desktop Responsibilities

- navigation and state transitions
- form validation before submit
- worker invocation and event subscription
- playback and export
- user-readable error display
- persistence of local UI preferences

## Worker Boundary

The desktop app must only talk to the inference runtime through a typed local worker contract.

The contract may be implemented through direct process calls, named pipes, or stdio.

No user-visible HTTP server is allowed for the final V1 experience.

## Data Model Summary

### Job

- `id`
- `state`
- `progress`
- `message`
- `created_at`
- `updated_at`
- `artifact_path`
- `error`

### VoicePreset

- `id`
- `display_name`
- `path`
- `language_hint`

### ModelInfo

- `id`
- `path`
- `loaded`
- `device`
- `status`

## Migration Guidance From Current Demo

Current extraction source:

- `demo/gradio_demo.py`

Extract first:

- voice loading logic
- generation workflow wrapper
- stop behavior
- progress and streaming behavior
- script formatting logic

Leave behind:

- Gradio layout and event wiring
- demo-only examples UI
- Gradio theme and CSS

## Acceptance Criteria

V1 is considered complete when:

- a user can launch the Windows app
- the app directly opens to a usable native workflow
- a model can be loaded from the desktop flow
- the app shows available voices
- the user can submit a generation request
- the app shows live job progress
- the user can stop a running job
- the app can present and export the final audio

## Risks

- embedded worker startup and dependency detection may be fragile on first Windows packaging pass
- large model loading can create long waits that require careful UI state handling
- audio streaming behavior may need adjustment when moved out of Gradio
- checkpoint path assumptions may differ between development and packaged app environments
- direct worker communication design needs to stay simple enough to replace the current loopback prototype

## OpenSpec Decision

V1 does not require OpenSpec.

The current operating model is:

- one stable architecture rule document
- one focused feature spec per milestone
- one API contract document
- one STATUS file per substantial change

This is sufficient until the project has more contributors, more parallel features, or stricter review governance.
