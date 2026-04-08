# Requirements: VibeVoice Windows Desktop

**Defined:** 2026-04-05
**Core Value:** The Windows app must feel like one direct native tool, not a research demo plus a hidden operator workflow.

## v1 Requirements

### Runtime

- [ ] **RUN-01**: Desktop app verifies internal runtime readiness at startup without requiring manual backend launch.
- [ ] **RUN-02**: Desktop app can load a local model from the native workflow.

### Generation

- [ ] **GEN-01**: User can select 1 to 4 voice presets and submit a generation job.
- [ ] **GEN-02**: Desktop app shows typed progress updates and elapsed time during a running job.
- [ ] **GEN-03**: User can stop a running generation job from the desktop workflow.

### Result

- [ ] **RES-01**: Completed generation exposes a visible artifact path in the desktop app.
- [ ] **RES-02**: User can play the generated result from the desktop app.
- [ ] **RES-03**: User can export a copy of the generated artifact to the configured output directory.

### Settings

- [ ] **SET-01**: Desktop app exposes a settings section for output directory and runtime diagnostics.
- [ ] **SET-02**: Desktop app can persist last-used defaults for model path, speaker count, CFG scale, script, and output directory.

### Governance

- [ ] **GOV-01**: Repo contains `.planning/` artifacts so GSD progress can run against durable project state.
- [ ] **GOV-02**: STATUS and design rule documents stay aligned with the implemented baseline.
- [ ] **GOV-03**: CEO review exists for the current V1 flow closure and confirms scope alignment.

## v2 Requirements

### Editing

- **EDIT-01**: User can edit generated waveform or timing after generation.
- **EDIT-02**: User can regenerate only selected sections instead of the full script.

### Distribution

- **DIST-01**: Windows installer bundles runtime dependencies for non-developer environments.
- **DIST-02**: Packaged app supports smoother first-run environment diagnostics.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Cloud inference | Contradicts the current local-only product promise |
| Voice cloning upload flow | Not required for current Windows V1 |
| Timeline editor | Large UX surface not needed to close generation shell |
| Manual runtime console pages | Violates the single-app experience |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| RUN-01 | Phase 1 | Complete |
| RUN-02 | Phase 1 | Complete |
| GEN-01 | Phase 2 | Complete |
| GEN-02 | Phase 2 | Complete |
| GEN-03 | Phase 2 | Complete |
| RES-01 | Phase 2 | Complete |
| RES-02 | Phase 2 | Complete |
| RES-03 | Phase 2 | Complete |
| SET-01 | Phase 2 | Complete |
| SET-02 | Phase 2 | Complete |
| GOV-01 | Phase 4 | Complete |
| GOV-02 | Phase 4 | Complete |
| GOV-03 | Phase 4 | Complete |

**Coverage:**
- v1 requirements: 13 total
- Mapped to phases: 13
- Unmapped: 0

---
*Requirements defined: 2026-04-05*
*Last updated: 2026-04-05 after Windows V1 flow closure*
