# Desktop Skeleton

This directory contains the Windows desktop shell.

Default technology choice:

- `WPF (.NET 8)`

The desktop shell owns user interaction only and should manage the local Python runtime automatically.

Current implementation status:

- the desktop app probes runtime health on startup
- if runtime is not available, the desktop app launches it internally
- users do not need to manually start a separate server for local development

## Run locally

From the repository root:

```powershell
dotnet run --project app/desktop/VibeVoice.Desktop/VibeVoice.Desktop.csproj
```
