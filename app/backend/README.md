# Runtime Prototype

This directory contains the current local Python runtime prototype for the Windows desktop application.

Responsibilities:

- expose the loopback API
- orchestrate model lifecycle
- validate requests
- create and manage generation jobs
- stream progress and audio events

Source of truth:

- `docs/design-rules/windows-app-design-rules.md`
- `docs/specs/2026-04-04-windows-v1-feature-spec.md`
- `docs/contracts/local-runtime-api-v1.yaml`

## Run locally

From the repository root:

```powershell
python -m app.runtime.main
```

Default base URL:

```text
http://127.0.0.1:8765
```

Note:

- this remains a development prototype
- the intended product experience is that the desktop app starts and supervises this runtime automatically
- `app.backend.*` remains only as a temporary compatibility alias
