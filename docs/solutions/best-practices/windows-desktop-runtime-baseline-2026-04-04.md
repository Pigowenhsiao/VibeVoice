---
title: Desktop-managed runtime baseline for the Windows VibeVoice app
date: 2026-04-04
category: best-practices
module: windows-desktop-runtime
problem_type: best_practice
component: documentation
severity: high
applies_when:
  - converging a Python demo into a native Windows desktop application
  - separating production architecture from a temporary loopback prototype
  - deciding whether desktop and inference should be one visible app or two visible processes
  - renaming backend-oriented code and contracts to runtime-oriented boundaries
tags: [windows-desktop, local-runtime, wpf, architecture-baseline, compatibility-alias, single-app]
related_components:
  - development_workflow
  - tooling
---

# Desktop-managed runtime baseline for the Windows VibeVoice app

## Context

VibeVoice started from a Python-first demo shape, with the model flow exposed through Gradio and a loopback HTTP prototype. That was useful for getting a vertical slice running, but it created a naming and ownership mismatch once the product direction became "a native-feeling Windows desktop app like `const-me/whisper`."

The key friction was not just technical. It was architectural drift. The repository had already decided that the user should not manually launch a backend server, but the codebase still used `backend` naming, a loopback HTTP runtime was the active implementation detail, and the desktop shell was still described as talking to a backend. Without a clear baseline, future work could easily reintroduce a two-app mental model.

## Guidance

Use a desktop-managed local runtime as the source of truth.

That means:

- the Windows shell owns startup and shutdown of the local inference host
- the runtime process can still be local HTTP for development, but it must remain invisible to the user
- the primary implementation path must live under `app/runtime`
- any `app/backend` path should be treated only as a temporary compatibility alias
- contracts, health payloads, UI wording, and docs must use `runtime` naming, not `backend` naming

In this repo, the stable baseline is:

- desktop shell: `app/desktop/VibeVoice.Desktop`
- runtime host: `app/runtime`
- compatibility alias: `app/backend`
- contract file: `docs/contracts/local-runtime-api-v1.yaml`

The desktop startup path should point to the runtime package directly:

```csharp
public string ModuleName { get; init; } = "app.runtime.main";
```

The runtime health contract should also reflect ownership clearly:

```json
{
  "status": "degraded",
  "runtime_version": "1.0.0"
}
```

## Why This Matters

This architecture keeps one thing true across code, docs, and user experience: the product is one desktop application.

If the code calls everything `backend`, future contributors will naturally reintroduce backend-centric choices: connection screens, startup instructions, manual server steps, and desktop wording that sounds like two separate systems. That is exactly the product shape we said we do not want.

Using `runtime` naming forces the correct ownership model. The desktop app is the product. The runtime is an internal subsystem it manages.

Keeping `app/backend` as a compatibility alias is also useful during migration. It avoids breaking old entrypoints immediately, while still making it explicit that all new work belongs under `app/runtime`.

## When to Apply

- when a repo already has a Python model host and is being wrapped in a desktop shell
- when a loopback HTTP prototype exists, but the final UX should still feel like a single native app
- when naming drift between `backend`, `worker`, and `runtime` starts creating architectural ambiguity
- when design rules say one thing, but actual package names, contracts, and UI copy still say another

## Examples

Before, the repo had the right product instinct but mixed vocabulary and paths:

```text
app/backend/main.py
docs/contracts/local-backend-api-v1.yaml
backend_version
Refresh Backend
```

After the baseline was tightened, the implementation and docs aligned:

```text
app/runtime/main.py
docs/contracts/local-runtime-api-v1.yaml
runtime_version
Refresh Runtime
```

Another important example is verification. For this repo, the right test shape is not only unit tests. The baseline should be proved with all of the following:

- Python smoke tests for `app/runtime/tests/test_smoke.py`
- desktop tests for runtime auto-start behavior
- desktop build verification
- browser verification of `/docs` and `/health`
- real desktop start/stop verification showing that launching the app brings `/health` up and closing the app brings `/health` back down

## Related

- [windows-app-design-rules.md](E:/AI%20Training/VibeVoice/docs/design-rules/windows-app-design-rules.md)
- [2026-04-04-windows-v1-feature-spec.md](E:/AI%20Training/VibeVoice/docs/specs/2026-04-04-windows-v1-feature-spec.md)
- [local-runtime-api-v1.yaml](E:/AI%20Training/VibeVoice/docs/contracts/local-runtime-api-v1.yaml)
- [STATUS_windows_app_v1_foundation.md](E:/AI%20Training/VibeVoice/STATUS_windows_app_v1_foundation.md)
