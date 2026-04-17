# Seasonal Bastion V3D Migration Checklist

## Project Goal

- [x] `SeasonalBastionV2` identified as the Unity 2D source project at `E:\Projects\SeasonalBastionV2`
- [x] `DemoSeasonalBastion3D` identified as the Unity 3D target/prototype project at `E:\Projects\DemoSeasonalBastion3D`
- [x] Migration goal clarified: move from a Unity 2D project model to a Unity 3D project model while preserving gameplay behavior as much as practical
- [x] `View2D` in V2 recognized as the old 2D presentation/runtime approach
- [x] 3D migration recognized as more than a view swap: scene setup, camera, input, spatial mapping, prefab rendering, colliders, and terrain/world presentation all need 3D-native treatment
- [x] V2 remains the main source for gameplay rules, data, and runtime behavior that should be preserved
- [x] DemoSeasonalBastion3D remains the main target/reference for the new Unity 3D runtime shape
- [ ] Re-map all active migration tasks against the source-2D -> target-3D transition plan before continuing implementation

### Goal Statement

Migrate Seasonal Bastion from a Unity 2D project into a Unity 3D project. Use `SeasonalBastionV2` as the source of gameplay behavior, data, and runtime rules to preserve where possible, and use `DemoSeasonalBastion3D` as the target/prototype for the new 3D runtime. The end goal is not merely to bolt a `View3D` layer onto the old project, but to arrive at a playable Unity 3D game that carries forward the important gameplay logic from the 2D version.

### Working Strategy

1. Treat `E:\Projects\SeasonalBastionV2` as the 2D source project to mine for gameplay logic, defs, save/load contracts, run-start flow, combat behavior, and world rules.
2. Treat `E:\Projects\DemoSeasonalBastion3D` as the 3D target/prototype where Unity-3D-native runtime structure is being assembled.
3. Preserve core gameplay behavior where practical, but allow 3D-native rewrites for scene bootstrap, camera, input, raycast, selection, world presentation, terrain, prefabs, and colliders.
4. Reuse gameplay logic from V2 selectively instead of assuming all 2D runtime code should be copied as-is.
5. Port by vertical slice, but judge each slice by source-to-target migration needs, not only by "add a 3D view layer" thinking.

## Implementation Prompt Reference

Use this as the default implementation prompt framing when asking an agent to execute a task in this project.

### Role
You are a Unity technical lead working on migrating Seasonal Bastion from the Unity 2D source project `E:\Projects\SeasonalBastionV2` to the Unity 3D target project/prototype `E:\Projects\DemoSeasonalBastion3D`.

### Migration framing
- `SeasonalBastionV2` is the 2D source project and the main reference for gameplay behavior, data, and runtime rules.
- `DemoSeasonalBastion3D` is the 3D target/prototype project.
- This migration is not only a `View2D` -> `View3D` swap. It is a source-2D -> target-3D project transition.
- Preserve gameplay logic where practical, but allow 3D-native implementation for scene setup, camera, input, raycast, selection, prefab rendering, colliders, and terrain/world presentation.
- Reuse V2 systems selectively. Do not assume every 2D runtime pattern should be copied unchanged into the 3D project.

### Architecture constraints
- Core gameplay must remain unchanged where practical:
  `GridMap`, `PlacementService`, `WorldState`, `RunStartFacade`, `BuildOrderService`.
- 3D must not become a second gameplay authority.
- Never introduce reverse dependency from gameplay core to `View3D`.
- `GridMap` is the single source of truth for gameplay occupancy and cell semantics.
- Do not duplicate gameplay validation logic in 3D systems.

### Coding rules
- Prefer adding new modules/classes instead of modifying core.
- Keep changes minimal, incremental, and compile-safe.
- Avoid large refactors.
- Avoid `FindObjectOfType` unless debug-only.
- Avoid breaking public contracts unless required.

### Output rules
- Only include relevant files.
- Show full file content for new files.
- For modified files, show only necessary changes.
- Code must compile.

### Global prefix

```text
Use V2 as gameplay reference, build toward a real 3D runtime target, keep patches compile-safe, and avoid unnecessary reverse coupling.
```

---

## IMPLEMENTATION PRIORITY ORDER

- [ ] P0 - Gameplay parity baseline
- [ ] P1 - 3D runtime foundation
- [ ] P2 - 3D interaction loop
- [ ] P3 - 3D world presentation core
- [ ] P5 - UI bridge and camera focus
- [ ] P4 - Actor presentation parity
- [ ] P7 - Save/load parity in the 3D runtime
- [ ] P6 - Combat readability
- [ ] P8 - Terrain and worldgen integration
- [ ] P9 - Debug, hardening, and performance

### Priority Notes

- Treat `SeasonalBastionV2` as the gameplay behavior reference.
- Treat `DemoSeasonalBastion3D` as the target runtime that must become playable as a real Unity 3D project.
- Prefer interaction and runtime stability before terrain ambition and visual polish.
- Judge progress by source-2D -> target-3D migration value, not only by isolated `View3D` feature count.

---

## PHASE 0 — BASELINE

### P0 - Gameplay parity baseline
- [ ] Confirm the gameplay systems that act as parity anchors from V2
- [ ] Lock the behavior expectations for GridMap, PlacementService, WorldState, RunStartFacade, and BuildOrderService
- [ ] Identify which save/load behaviors must remain equivalent
- [ ] Identify which combat behaviors must remain equivalent
- [ ] Identify which NPC/path behaviors must remain equivalent
- [ ] Treat V2 as the comparison oracle for major gameplay behavior

### Existing baseline findings
- [ ] Project compiles without errors
- [x] Core systems identified:
  - [x] GridMap
  - [x] PlacementService
  - [x] WorldState
  - [x] RunStartFacade
  - [x] BuildOrderService
- [x] Main gameplay scene confirmed
- [x] Grid orientation defined (XZ or XY)
- [x] Cell size documented
- [x] Baseline doc created

---

## PHASE 1 — FOUNDATION 3D

### P1 - 3D runtime foundation
- [ ] Verify `GridWorldSettings`
- [ ] Verify `CellWorldMapper3D`
- [ ] Verify `WorldToCellResolver3D`
- [ ] Eliminate mapping offset issues
- [ ] Lock stable ground layer + collider + raycast behavior
- [ ] Verify 3D camera pan/zoom/clamp behavior
- [ ] Verify basic 3D scene bootstrap stability

### Current phase status

Status wording in this checklist:
- **implemented** = code/module exists
- **wired** = scene/runtime hookup exists
- **verified** = explicitly checked in editor/playmode/runtime behavior

Runtime verification execution reference:
- `V3D_Runtime_Verification_Plan.md`

#### Spatial
- [x] GridWorldSettings implemented
- [x] CellWorldMapper3D implemented
- [x] WorldToCellResolver3D implemented
- [ ] Mapping verified accurate (no offset)

#### Camera
- [x] StrategyCameraController3D implemented
- [x] Pan/zoom wired
- [x] Camera bounds applied
- [x] Camera focus hookup present in scene/install path

#### Scene
- [x] Ground plane exists
- [x] Ground layer configured
- [x] Raycast wired
- [x] Core View3D controller set present in `DemoGameplayScene.unity`
- [x] `GameplaySceneInstaller3D` auto-wiring path present

#### Debug
- [x] Hover cell debug component exists
- [x] Hover/highlight debug path wired in `DemoGameplayScene.unity`
- [x] Grid overlay debug path wired in `DemoGameplayScene.unity`
- [ ] Hover matches correct cell

---

## PHASE 2 — PLACEMENT 3D

### P2 - 3D interaction loop
- [ ] Hover cell matches the intended grid cell
- [ ] Placement preview works in 3D
- [ ] Footprint preview is correct
- [ ] Driveway/front marker preview is correct
- [ ] Click-build works through 3D world input
- [ ] World selection works in 3D
- [ ] Clicking empty space clears selection correctly
- [ ] UI click-through issues are eliminated

### Current phase status
- [x] PlacementPreviewController3D implemented
- [x] PlacementGhostView3D implemented
- [x] FootprintOverlay3D implemented
- [x] Driveway marker visible

#### Integration
- [x] Uses PlacementService (no duplicate logic)
- [x] Valid/invalid preview wired
- [x] Click build wired
- [x] Placement HUD wiring path present
- [x] Placement validation debug path present

#### Validation
- [ ] Footprint verified correct
- [ ] Road adjacency verified correct
- [ ] Driveway verified correct

---

## PHASE 3 — BUILDING 3D

### P3 - 3D world presentation core
- [ ] BuildingView3D is stable
- [ ] BuildingViewFactory3D is stable
- [ ] Prefab registry is usable and extendable
- [ ] Construction visual state is clear
- [ ] Building remove flow works end-to-end
- [ ] Building upgrade flow works end-to-end
- [ ] No duplicate building/build-site views remain

### Current phase status
- [x] BuildingView3D implemented
- [x] BuildingViewFactory3D implemented
- [x] Prefab registry exists

#### Runtime
- [x] Buildings spawn wired
- [x] Remove runtime hook wired
- [x] Upgrade runtime hook wired
- [x] Selection inspect HUD wiring path present
- [ ] Remove verified end-to-end
- [ ] Upgrade verified end-to-end
- [ ] No duplicate views verified

#### Visual
- [ ] Correct position verified (center cell)
- [ ] Scale verified against grid
- [x] Construction state visible

---

## PHASE 4 — NPC / ENEMY 3D

### P4 - Actor presentation parity
- [ ] NPC view/factory path is stable
- [ ] NPC movement presenter is stable
- [ ] NPC movement is smooth enough
- [ ] NPC rotation is correct enough
- [ ] Enemy view/factory path is stable
- [ ] Enemy movement presenter is stable
- [ ] Enemy movement is smooth enough
- [ ] No serious desync between logic and presentation
- [ ] Path behavior still respects V2 gameplay expectations

### Current phase status

#### NPC
- [x] NPC runtime presentation path exists in `WorldViewRoot3D`
- [x] `NpcMovementPresenter3D` implemented
- [ ] Movement smooth enough verified
- [ ] Rotation correct enough verified

#### Enemy
- [x] Enemy runtime presentation path exists in `WorldViewRoot3D`
- [x] `EnemyMovementPresenter3D` implemented
- [ ] Movement smooth enough verified

#### Validation
- [ ] Pathfinding unchanged
- [ ] No desync between logic and view

---

## PHASE 5 — SELECTION & UI

### P5 - UI bridge and camera focus
- [ ] Selection updates the correct info panel state
- [ ] Selection highlight is clear and reliable
- [ ] Focus on selected entities works
- [ ] Focus from notifications works
- [ ] Selection and focus do not break UI interaction rules

### Current phase status
- [x] WorldSelectionController3D implemented
- [x] Object-picking bridge path present
- [ ] Object selection verified accurate
- [ ] Click empty verified clears selection

#### UI
- [x] Selection-to-info-panel wiring present
- [x] Selection inspect HUD component exists
- [ ] No UI click-through issues verified

#### Camera Followups
- [x] Focus controller exists
- [x] Focus runtime wiring present
- [x] Focus hookup present in `DemoGameplayScene.unity`
- [ ] Focus verified on selection
- [ ] Focus verified from notifications

---

## Progress Notes

- Phase 0 baseline audited and documented in `Docs/V3D_Migration_Baseline.md`.
- T01 completed by creating and organizing the `View3D` folder shell.
- T02 completed as a minimal refactor by introducing `GridWorldSettings` and normalizing `CellWorldMapper3D`.
- T03 completed by introducing `GroundRaycastService` and `WorldToCellResolver3D`, then wiring `WorldSelectionController3D` to use them.
- T05 completed as a minimal 3D scene setup pass with runtime terrain/collider safety wiring.
- T06 completed by adding hover debug overlay and mapping verification helpers.
- Phase 1 ground-layer wiring was patched in project settings and scene serialization, but mapping offset accuracy still needs explicit runtime verification.
- Phase 2 validation helpers now expose footprint, driveway, and adjacent-road state for in-scene verification.
- Phase 3 now has placeholder-safe `BuildingView3D`, `BuildingViewFactory3D`, `BuildingPrefabRegistry3D`, and `ConstructionVisualController3D` with primitive fallback visuals.
- Building/build-site lifecycle sync code exists to reduce duplicate views and support construction-to-complete transitions without requiring real 3D assets, but this still needs explicit runtime verification.
- Phase 5 selection now supports placeholder world-object picking for buildings and build sites via `SelectedEntityBridge3D` and `SelectionHighlight3D`, with debug inspect HUD integration.
- Phase 4 currently uses `WorldViewRoot3D` plus `NpcMovementPresenter3D` and `EnemyMovementPresenter3D` for actor presentation. There is not currently a separate `NpcView3D` or `EnemyView3D` class.
- Phase 5/T22 now has a minimal `CameraFocusController3D` that can focus the strategy camera on selected buildings, build sites, or cells without introducing gameplay-to-view coupling.
- Phase 9/T32 now has a minimal runtime-toggleable `GridOverlay3D` for grid lines, blocked cells, buildable cells, and occupancy debug overlays using existing `GridMap` and generated terrain data.
- Phase 9/T33 now has a minimal `BuildStateDebug3D` overlay for inspecting runtime building/build-site state without touching gameplay logic, and selection action debug input was aligned with the Input System.
- `GameplaySceneInstaller3D` exists and contains scene auto-wiring logic for major View3D references, and `DemoGameplayScene.unity` contains the core controller set for the intended verification path, but scene-level/runtime behavior still remains pending.
- `SelectionActionDebug3D` provides debug hooks for remove/upgrade actions, while end-to-end success still requires explicit editor/playmode verification.
- Compile-clean status and remaining runtime behavior still need explicit editor/playmode verification for remove, upgrade, click-through edge cases, movement smoothness, camera focus behavior, and debug overlay usability.

---

## PHASE 6 — COMBAT VISUAL

### P6 - Combat readability
- [ ] Projectile visuals are visible
- [ ] Hit effects are visible
- [ ] Death effects are visible
- [ ] Visual events match combat events correctly
- [ ] Combat logic remains unchanged

### Current phase status
- [ ] Projectile visible
- [ ] Hit effect visible
- [ ] Death effect visible

#### Validation
- [ ] Combat logic unchanged
- [ ] Visual matches events

---

## PHASE 7 — SAVE / LOAD

### P7 - Save/load parity in the 3D runtime
- [ ] Seed/worldgen config persistence is defined correctly
- [ ] Map rebuilds correctly after load
- [ ] Buildings restore correctly after load
- [ ] NPC views restore correctly after load
- [ ] Enemy views restore correctly after load
- [ ] No duplicate views appear after load
- [ ] No GameObject state is saved directly

### Current phase status
- [ ] Seed saved
- [ ] Worldgen config saved
- [ ] No GameObject saved directly verified

#### Load
- [ ] Map regenerates correctly
- [ ] Buildings restored
- [ ] NPC restored
- [ ] No duplicate views

---

## PHASE 8 — WORLDGEN

### P8 - Terrain and worldgen integration
- [ ] Noise generator path is stable
- [ ] Heightmap generation is stable
- [ ] Seed behavior is deterministic
- [ ] GeneratedMapData is valid
- [ ] TerrainSemanticType is defined correctly
- [ ] TerrainToGridAdapter works correctly
- [ ] GridMap remains the gameplay authority
- [ ] RunStart uses generated map data correctly
- [ ] HQ spawns on valid cells
- [ ] Terrain/map presenter matches gameplay semantics

### Current phase status

#### Generation
- [ ] Noise generator working
- [ ] Heightmap stable
- [ ] Seed deterministic

#### Data
- [ ] GeneratedMapData valid
- [ ] TerrainSemanticType defined

#### Adapter
- [ ] TerrainToGridAdapter works
- [ ] GridMap remains authority

#### Integration
- [ ] RunStart uses generated map
- [ ] HQ spawns valid
- [ ] No blocked spawn

#### Visual
- [ ] MapPresenter3D renders map
- [ ] Terrain matches gameplay grid

---

## PHASE 9 — DEBUG & HARDENING

### P9 - Debug, hardening, and performance
- [ ] Footprint debug is usable
- [ ] NPC path debug is usable
- [ ] Combat debug is usable
- [ ] Reverse dependency to 3D layers is removed where needed
- [ ] Core gameplay duplication is reduced or eliminated
- [ ] Circular assembly dependency risks are removed
- [ ] Heavy runtime `FindObjectOfType` usage is removed where possible
- [ ] Allocation spikes in loops are reviewed
- [ ] Compile-clean pass is verified
- [ ] Final architecture review is documented

### Current phase status

#### Next recommended step (while runtime verification is blocked)
- [x] Create a code-backed verification prep checklist for P1/P2/P3/P5
- [x] Map each pending verify item to the specific scene object, script, and expected observable outcome
- [ ] Identify which claims can be promoted from implemented -> wired purely from code/scene inspection
- [x] Defer stability/pass claims until editor/playmode verification is available
- [x] Create `V3D_Runtime_Verification_Plan.md` as the execution reference for the first runtime verification pass

#### Debug
- [x] Grid overlay toggle
- [x] Blocked/buildable visible
- [x] Build state debug visible
- [ ] Footprint debug visible
- [ ] NPC path debug visible
- [ ] Combat debug visible

#### Stability
- [ ] No reverse dependency to View3D
- [ ] No core logic duplication
- [ ] No circular asmdef dependency

#### Performance
- [ ] No heavy FindObjectOfType in runtime
- [ ] No allocation spikes in loops

---

## FINAL CHECK

- [ ] Game playable fully in 3D
- [ ] All systems stable (build, NPC, combat)
- [ ] Save/load works
- [ ] No major bugs
- [ ] Architecture clean

## FINAL PRIORITY RULE

- [ ] Make the 3D target playable first
- [ ] Preserve important gameplay behavior intentionally
- [ ] Prefer interaction and runtime stability before terrain ambition and polish

RESULT:
- [ ] PASS
- [ ] FAIL
