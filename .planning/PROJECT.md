# VibeVoice Windows Desktop

## What This Is

VibeVoice is being turned into a Windows desktop application for local multi-speaker speech generation. The product target is a native-feeling desktop workflow where a user opens one app, loads a local model, runs generation, watches progress, then plays or exports the result without touching Python or Gradio.

## Core Value

The Windows app must feel like one direct native tool, not a research demo plus a hidden operator workflow.

## Requirements

### Validated

- ✓ Desktop app auto-starts the local runtime without asking the user to launch another process.
- ✓ Desktop UI and inference runtime are separated into `app/desktop` and `app/runtime`.
- ✓ Runtime contract is versioned and observable through local health, job, and artifact endpoints.

### Active

- [ ] Native result flow includes playback and export after generation completes.
- [ ] Desktop settings persist last-used defaults and output directory.
- [ ] Desktop shell consumes typed job progress events rather than polling-only updates.
- [ ] GSD planning artifacts stay current with the shipped desktop/runtime baseline.
- [ ] CEO-style review confirms the current design still matches the intended product shape.

### Out of Scope

- Waveform or timeline editing — not required for V1 generation workflow.
- Cloud inference — local-only workflow is the current product promise.
- Manual backend startup steps — violates the single-app experience.
- Extending `demo/gradio_demo.py` as product UI — desktop architecture has replaced that path.

## Context

The repository already contains a WPF desktop shell, a Python runtime host, design rules, feature specs, API contracts, and status files. A prior audit found four missing pieces for V1 closure: result playback/export, settings, typed progress consumption, and formal GSD tracking. This milestone closes those gaps without changing the model core direction.

## Constraints

- **Tech stack**: WPF on .NET 8, Python runtime, existing `vibevoice/` model core — preserves the current stack while moving to a native shell.
- **UX**: One visible Windows app only — users must never operate runtime lifecycle manually.
- **Scope**: Hold current V1 scope — finish direct workflow instead of adding editing, cloud, or multi-user features.
- **Verification**: Every substantial change must update STATUS files and remain testable via Python tests, .NET tests, build validation, and runtime QA.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Keep `app/runtime` as the primary runtime path | Align naming, design rules, and future migration | ✓ Good |
| Consume runtime progress via typed event stream in desktop shell | Matches spec, removes polling-only behavior, and keeps progress semantics explicit | ✓ Good |
| Add playback, export, and settings as desktop services | Keeps UI thin and makes result flow testable | ✓ Good |
| Initialize `.planning/` for this repo now | Formal GSD progress checks need durable project state | ✓ Good |
| Review current flow in HOLD_SCOPE CEO mode | User asked to finish the planned V1 gaps, not expand product scope | ✓ Good |

---
*Last updated: 2026-04-05 after Windows V1 flow-closure implementation and QA*
