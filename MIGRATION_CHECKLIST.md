# Migration Checklist - SeasonalBastionV2 -> DemoSeasonalBastion3D

## Goal
Port gameplay core from `SeasonalBastionV2` into `DemoSeasonalBastion3D`, keep gameplay grid-first, and use Demo3D terrain/worldgen as the 3D foundation.

---

## Overall Status
- **Progress:** ~83%
- **Current focus:** Phase 4 - runtime interaction usability and inspect/HUD support on the clean generated scene path
- **Current state:** Core gameplay foundation exists, terrain bridge exists, 3D interaction prototype exists, a minimal build lifecycle is wired with terrain-aware placement and terrain-derived bounds/start/lane data, there is a clean scene-generation path for a fresh gameplay scene with explicit runtime wiring, placement feedback has a minimal runtime HUD path, a basic selection/inspect HUD now exists for generated-scene testing, and recent Unity runtime verification confirmed camera pitch control plus corrected grid/building-site terrain alignment, but full compile/runtime verification is still incomplete.

## Current Blockers
- Unity compile has not been verified yet, because Unity reimport/compile has not been run from this environment.
- `BuildOrderServiceStub` now performs a minimal place/build/complete loop, but upgrade/repair and full worker-driven progression are still stubbed.
- Terrain-derived start zone and spawn lanes are heuristic selections from terrain bounds, not yet gameplay-tuned paths/config.
- Placement feedback now has a minimal runtime HUD path, but it is still prototype-level rather than final production UI.
- Runtime defs/registry are still demo/manual and not yet connected to a real loading path.
- Clean scene creation now has an editor automation path, but it still needs real Unity compile/open verification.
- New camera/selection/placement input path has been moved away from legacy `UnityEngine.Input`; camera pitch/rotation and terrain alignment have now seen initial runtime verification, but broader interaction verification is still incomplete.
- Generated scene and assets have now been created by Unity, but scene-side runtime components still need final Input System compatibility and asset-binding verification.

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
**Blocked by:** Scene still uses some convenience lookups/polling and clean scene path is not Unity-verified yet
- [x] Add `GameplayRuntimeBootstrap`
- [x] Bind `WorldViewRoot3D` to shared runtime
- [x] Add editor script to generate a clean gameplay scene setup
- [x] Add explicit installer wiring for generated-scene runtime references
- [ ] Reduce remaining `FindObjectOfType` coupling outside generated-scene wiring
- [ ] Move from polling refresh to event-driven refresh
- [ ] Open/generated scene verified in Unity
- [x] Remove legacy `UnityEngine.Input` dependency from newly added 3D camera/interaction scripts

### 3.3 Interaction
**Status:** In progress
**Blocked by:** Feedback is now available in a minimal HUD, but the runtime UI flow is still prototype-level
- [x] Add raycast world-to-cell selection
- [x] Add hover/selection highlight
- [x] Add placement footprint preview
- [x] Add placement rotation input
- [x] Add placement confirm input
- [x] Show placement failure reason in UI/debug label
- [x] Add minimal runtime placement HUD for generated scene testing
- [x] Support footprint preview that respects rotation for non-square buildings

---

## Phase 4 - Replace 2D gameplay interaction with proper 3D gameplay interaction
**Status:** In progress
**Progress:** ~20%
**Blocked by:** Core gameplay correctness and Unity scene verification are still pending
- [x] Add minimal inspect/selection HUD for cells/buildings/sites
- [ ] Expand inspect model to NPC/enemy/site actions and richer details
- [x] Add strategy camera controller for 3D terrain
- [x] Improve strategy camera startup framing, drag-pan, and zoom usability
- [x] Add strategy camera rotation controls (Q/E step rotate, middle-mouse rotate drag)
- [x] Add camera reset/focus-to-map-center behavior at startup and on demand
- [ ] Add proper placement mode switching / selected building type UI
- [x] Add minimal 3D-friendly runtime placement HUD
- [x] Add basic inspect HUD alongside placement status
- [ ] Expand runtime HUD/panels beyond placement status and basic inspection

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
- [x] A clean generated gameplay scene path exists (`SeasonalBastion/Create Demo Gameplay Scene` -> `Assets/Scenes/DemoGameplayScene.unity`)
- [ ] `SampleScene` can initialize terrain gameplay runtime
- [ ] Generated `DemoGameplayScene` opens and initializes correctly in Unity
- [x] Add runtime fallback loading for generated worldgen settings when scene instance references are missing
- [x] Generated scene path now uses `InputSystemUIInputModule` instead of legacy `StandaloneInputModule`
- [x] Grid maps correctly onto terrain
- [ ] Hover/select works on terrain cells
- [ ] Placement preview matches gameplay validation
- [x] Placement creates expected runtime entities
- [ ] Build completion loop works end-to-end
- [ ] World view stays in sync with runtime state

---

## Recommended Next Step
1. Verify hover/select works exactly on terrain cells after the mapper alignment changes.
2. Verify placement preview matches gameplay validation for rotated and multi-cell buildings.
3. Verify the generated scene installer wires camera/runtime/view components correctly in Unity.
4. Verify terrain-derived HQ/start zone/spawn lanes visually and tune heuristics if needed.
5. Further tune camera feel and generated-scene usability based on live Unity testing.
6. Expand the minimal HUDs into a more complete runtime HUD/UI flow, including richer inspect details and actions.
7. Then move toward worker-driven construction flow.

---

## Notes
- Current priority is **gameplay correctness before 3D polish**.
- Phase 3 is useful for validation, but some parts are still prototype/debug scaffolding.
- Demo bootstrap currently seeds manual defs/content and updates the build loop from `GameplayRuntimeBootstrap.Update()`.
- Do not treat demo seed content or partial stubbed services as final gameplay implementation.
