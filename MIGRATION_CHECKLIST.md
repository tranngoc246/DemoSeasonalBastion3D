# Migration Checklist - SeasonalBastionV2 -> DemoSeasonalBastion3D

## Goal
Port gameplay core from `SeasonalBastionV2` into `DemoSeasonalBastion3D`, keep gameplay grid-first, and use Demo3D terrain/worldgen as the 3D foundation.

---

## Overall Status
- **Progress:** ~75%
- **Current focus:** Phase A.3 / Phase 2.2 - terrain-derived runtime correctness and Unity verification
- **Current state:** Core gameplay foundation exists, terrain bridge exists, 3D interaction prototype exists, and a minimal build lifecycle is wired with terrain-aware placement, terrain-derived bounds, start zone, and spawn lanes, but runtime correctness and Unity verification are still incomplete.

## Current Blockers
- Unity compile has not been verified yet, because Unity reimport/compile has not been run from this environment.
- `BuildOrderServiceStub` now performs a minimal place/build/complete loop, but upgrade/repair and full worker-driven progression are still stubbed.
- Terrain-derived start zone and spawn lanes are heuristic selections from terrain bounds, not yet gameplay-tuned paths/config.
- Placement feedback is still debug-level, not proper runtime HUD/UI.
- Runtime defs/registry are still demo/manual and not yet connected to a real loading path.

---

## Baseline
**Status:** Done

- [x] Target project fixed to `DemoSeasonalBastion3D`
- [x] Source gameplay fixed to `SeasonalBastionV2`
- [x] Grid-first migration direction confirmed
- [x] 3D terrain/worldgen chosen as foundation, not 2D view/input reuse

---

## Phase 1 - Gameplay Core Foundation
**Status:** In progress
**Progress:** ~95%
**Blocked by:** Unity compile verification and first real compile-fix pass

### 1.1 Contracts, state, stores, grid
**Status:** Done
- [x] Port id/resource/common contract types
- [x] Port grid contracts and grid map foundation
- [x] Port event bus contract and runtime bus
- [x] Port placement contracts
- [x] Port job and notification contracts
- [x] Port world state contracts
- [x] Port world stores and runtime store implementations
- [x] Port world state root

### 1.2 Minimal defs and runtime services
**Status:** Done
- [x] Add minimal def DTOs (`BuildingDef`, `NpcDef`, `EnemyDef`, `TowerDef`, `CostDef`, etc.)
- [x] Add `IDataRegistry`
- [x] Add simplified `DataRegistry`
- [x] Add `WorldIndexService`
- [x] Add `WorldOps`
- [x] Add `PlacementService`
- [x] Add gameplay events needed by runtime core

### 1.3 Compile closure / missing types
**Status:** In progress
**Blocked by:** Need Unity compile pass
- [x] Add `CellPos`
- [x] Add `Dir4`
- [x] Add `BuildOrder` types
- [x] Add `BuildOrderServiceStub`
- [x] Wire bootstrap/update tick to build orders
- [ ] Verify compile in Unity and fix first real compiler errors

---

## Phase A - Gameplay Correctness (re-baselined priority)
**Status:** In progress
**Progress:** ~55%
**Blocked by:** Runtime still not Unity-verified and build loop is still workerless

### A1. Build/order/site/building loop
**Status:** Minimal loop implemented
**Blocked by:** Still not worker-driven and not Unity-verified end-to-end
- [x] Replace `BuildOrderServiceStub` with a minimally functional build loop
- [x] Create build site correctly on placement commit
- [x] Render build sites in 3D view
- [x] Add build progression over time
- [x] Transition `BuildSite -> Building(IsConstructed=true)`
- [x] Clear site occupancy when construction completes
- [x] Apply building occupancy when construction completes
- [x] Update `WorldIndexService` after completion
- [ ] Add upgrade flow
- [ ] Add repair flow
- [ ] Replace time-only progression with worker/job-driven construction

### A2. Occupancy and placement correctness
**Status:** In progress
**Blocked by:** Needs runtime verification against scene data
- [x] Align build/build-site occupancy footprint math with rotation-aware placement rules
- [x] Verify building footprint occupancy rules
- [x] Verify site footprint occupancy rules
- [x] Verify road placement/removal baseline logic exists
- [x] Verify driveway / entry-road auto-create baseline exists
- [x] Verify destroy/remove transitions clean up grid correctly
- [x] Validate placement against terrain `BuildableMap`, water, and blocked cells instead of only `BuildableRect`
- [x] Validate rotated footprint dimensions for non-square buildings

### A3. Runtime/data correctness
**Status:** Not started
**Blocked by:** Current bootstrap still intentionally demo-first
- [ ] Move demo defs toward a real loading path
- [ ] Reduce hardcoded registry/bootstrap content
- [ ] Verify `RunStartRuntime` values against actual terrain/gameplay needs

**Phase A exit:** placement and building lifecycle behave correctly in runtime

---

## Phase 2 - Terrain Bridge
**Status:** In progress
**Progress:** ~90%
**Blocked by:** Start/build areas and lane derivation are now terrain-derived but still heuristic

### 2.1 Terrain to gameplay mapping
**Status:** Done
- [x] Add `CellWorldMapper3D`
- [x] Add `TerrainGameplayBridge`
- [x] Add `TerrainGameplayRuntimeHost`
- [x] Add terrain debug gizmos

### 2.2 Terrain-driven gameplay data
**Status:** In progress
**Blocked by:** Start/build area and lane data are terrain-derived but still heuristic
- [x] Derive real `BuildableRect` from `BuildableMap`
- [x] Feed terrain blocked/water/buildable data into placement/gameplay rules
- [x] Derive start/build area from terrain data
- [x] Derive spawn gates/lanes from terrain or start config

---

## Phase 3 - 3D View and Interaction Prototype
**Status:** Prototype working
**Progress:** ~80%
**Blocked by:** View still prototype-level and refresh/composition are not clean yet

### 3.1 World view
**Status:** In progress
**Blocked by:** Build sites use simple placeholder visuals and refresh is still brute-force
- [x] Add `PrefabCatalog3D`
- [x] Add `WorldViewRoot3D`
- [x] Render buildings in 3D
- [x] Render NPCs in 3D
- [x] Render enemies in 3D
- [x] Render build sites in 3D

### 3.2 Runtime composition
**Status:** In progress
**Blocked by:** Scene still uses convenience lookups/polling
- [x] Add `GameplayRuntimeBootstrap`
- [x] Bind `WorldViewRoot3D` to shared runtime
- [ ] Reduce `FindObjectOfType` coupling
- [ ] Move from polling refresh to event-driven refresh

### 3.3 Interaction
**Status:** In progress
**Blocked by:** Feedback is debug-level only, not proper runtime UI yet
- [x] Add raycast world-to-cell selection
- [x] Add hover/selection highlight
- [x] Add placement footprint preview
- [x] Add placement rotation input
- [x] Add placement confirm input
- [x] Show placement failure reason in UI/debug label
- [x] Support footprint preview that respects rotation for non-square buildings

---

## Phase 4 - Replace 2D gameplay interaction with proper 3D gameplay interaction
**Status:** Not started
**Progress:** ~0%
**Blocked by:** Core gameplay correctness should be stabilized first
- [ ] Add inspect/selection model for building/NPC/enemy/site
- [ ] Add strategy camera controller for 3D terrain
- [ ] Add proper placement mode switching / selected building type UI
- [ ] Add 3D-friendly runtime HUD/panels

---

## Phase 5 - Port remaining gameplay systems from V2
**Status:** Not started
**Progress:** ~0%
**Blocked by:** Core runtime loop is not stable enough yet
- [ ] Build completion and worker build behavior
- [ ] Job board / worker job assignment
- [ ] Production and hauling loops
- [ ] Waves/spawning loop
- [ ] Tower attack/combat loop
- [ ] Runtime UI presenters and panels adapted to 3D flow

---

## Verification Checklist
**Status:** In progress

- [ ] Unity project compiles cleanly
- [x] `SampleScene` exists as the current integration scene target
- [ ] `SampleScene` can initialize terrain gameplay runtime
- [ ] Grid maps correctly onto terrain
- [ ] Hover/select works on terrain cells
- [ ] Placement preview matches gameplay validation
- [ ] Placement creates expected runtime entities
- [ ] Build completion loop works end-to-end
- [ ] World view stays in sync with runtime state

---

## Recommended Next Step
1. Run a real Unity compile/scene verification pass in `SampleScene`.
2. Verify terrain-derived HQ/start zone/spawn lanes visually and tune heuristics if needed.
3. Replace debug placement label with proper runtime HUD/UI presentation.
4. Then move toward worker-driven construction flow.

---

## Notes
- Current priority is **gameplay correctness before 3D polish**.
- Phase 3 is useful for validation, but some parts are still prototype/debug scaffolding.
- Demo bootstrap currently seeds manual defs/content and updates the build loop from `GameplayRuntimeBootstrap.Update()`.
- Do not treat demo seed content or partial stubbed services as final gameplay implementation.
