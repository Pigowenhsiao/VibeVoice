# Roadmap: VibeVoice Windows Desktop

## Overview

This roadmap closes the last V1 usability gaps between the original desktop foundation and the intended Windows product shape. The work starts from audit findings, finishes the native workflow, hardens the runtime event contract, then lands durable planning and review artifacts so future GSD progress checks have real project state to inspect.

## Phases

- [x] **Phase 1: Audit and Alignment** - Confirm the gap list and map it back to the approved design/spec baseline.
- [x] **Phase 2: Desktop Flow Closure** - Add result playback/export, settings persistence, and event-driven progress in the desktop shell.
- [x] **Phase 3: Runtime Contract Verification** - Verify typed runtime events and artifact delivery with regression coverage.
- [x] **Phase 4: Governance and CEO Review** - Initialize `.planning/`, update design/status docs, and publish a scope-holding CEO review.

## Phase Details

### Phase 1: Audit and Alignment
**Goal**: Reconfirm the missing V1 items before implementation starts.
**Depends on**: Nothing
**Requirements**: RUN-01, GOV-02
**Success Criteria** (what must be TRUE):
  1. Audit findings point to specific missing requirements, not vague concerns.
  2. Desktop gaps are mapped back to design rule and feature spec language.
  3. The implementation target stays inside current V1 scope.
**Plans**: 1 plan

Plans:
- [x] 01-01: Re-run project audit and lock the 4-item closure list.

### Phase 2: Desktop Flow Closure
**Goal**: Finish the native desktop workflow from model load through result use.
**Depends on**: Phase 1
**Requirements**: GEN-01, GEN-02, GEN-03, RES-01, RES-02, RES-03, SET-01, SET-02
**Success Criteria** (what must be TRUE):
  1. Desktop app shows job progress from typed events and displays elapsed time.
  2. Desktop app exposes result playback and export after completion.
  3. Desktop app exposes a settings section that persists last-used defaults.
**Plans**: 2 plans

Plans:
- [x] 02-01: Add desktop services and ViewModel state for playback, export, settings, and event streaming.
- [x] 02-02: Update WPF layout to match the native workflow order and expose result/settings UI.

### Phase 3: Runtime Contract Verification
**Goal**: Ensure runtime event ordering and artifact delivery support the desktop shell.
**Depends on**: Phase 2
**Requirements**: GEN-02, RES-01
**Success Criteria** (what must be TRUE):
  1. Runtime tests prove typed progress and artifact-ready events are both emitted.
  2. Desktop tests prove the final artifact enables playback and export actions.
  3. Build and test outputs stay green after the event-stream migration.
**Plans**: 2 plans

Plans:
- [x] 03-01: Add runtime regression coverage for typed event and artifact emission.
- [x] 03-02: Fix runtime event ordering so subscribers receive artifact-ready before stream termination.

### Phase 4: Governance and CEO Review
**Goal**: Leave the repo in a state that GSD progress and future planning can use directly.
**Depends on**: Phase 3
**Requirements**: GOV-01, GOV-02, GOV-03
**Success Criteria** (what must be TRUE):
  1. `.planning/` exists with current project, requirement, roadmap, and state context.
  2. STATUS and design rules reflect the implemented baseline.
  3. CEO review confirms scope is still right and calls out remaining V1 concerns.
**Plans**: 2 plans

Plans:
- [x] 04-01: Initialize `.planning/` using the current Windows desktop milestone.
- [x] 04-02: Publish CEO review and update status/design docs.

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Audit and Alignment | 1/1 | Complete | 2026-04-05 |
| 2. Desktop Flow Closure | 2/2 | Complete | 2026-04-05 |
| 3. Runtime Contract Verification | 2/2 | Complete | 2026-04-05 |
| 4. Governance and CEO Review | 2/2 | Complete | 2026-04-05 |
