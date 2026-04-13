# Seasonal Bastion V3D Migration Checklist

---

## PHASE 0 — BASELINE

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

### Spatial
- [x] GridWorldSettings created
- [x] CellWorldMapper3D working
- [x] WorldToCellResolver3D working
- [ ] Mapping accurate (no offset)

### Camera
- [x] StrategyCameraController3D works
- [x] Pan/zoom stable
- [x] Camera bounds applied

### Scene
- [x] Ground plane exists
- [x] Ground layer configured
- [x] Raycast works

### Debug
- [x] Hover cell debug visible
- [ ] Hover matches correct cell

---

## PHASE 2 — PLACEMENT 3D

- [x] PlacementPreviewController3D created
- [x] PlacementGhostView3D working
- [x] FootprintOverlay3D correct
- [x] Driveway marker visible

### Integration
- [x] Uses PlacementService (no duplicate logic)
- [x] Valid/invalid preview correct
- [x] Click build works

### Validation
- [x] Footprint correct
- [x] Road adjacency correct
- [x] Driveway correct

---

## PHASE 3 — BUILDING 3D

- [x] BuildingView3D created
- [x] BuildingViewFactory3D works
- [x] Prefab registry exists

### Runtime
- [x] Buildings spawn correctly
- [ ] Remove works
- [ ] Upgrade updates correctly
- [x] No duplicate views

### Visual
- [x] Correct position (center cell)
- [x] Scale matches grid
- [x] Construction state visible

---

## PHASE 4 — NPC / ENEMY 3D

### NPC
- [ ] NpcView3D exists
- [ ] Movement smooth
- [ ] Rotation correct

### Enemy
- [ ] EnemyView3D exists
- [ ] Movement smooth

### Validation
- [ ] Pathfinding unchanged
- [ ] No desync between logic and view

---

## PHASE 5 — SELECTION & UI

- [x] WorldSelectionController3D works
- [x] Object selection accurate
- [x] Click empty clears selection

### UI
- [x] Selection updates info panel
- [ ] No UI click-through issues

### Camera Followups
- [ ] Focus works on selection
- [ ] Focus works from notifications

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
- Building/build-site lifecycle sync was stabilized to reduce duplicate views and support construction-to-complete transitions without requiring real 3D assets.
- Phase 5 selection now supports placeholder world-object picking for buildings and build sites via `SelectedEntityBridge3D` and `SelectionHighlight3D`, with debug inspect HUD integration.
- Compile-clean status and remaining runtime behavior still need explicit editor/playmode verification for remove, upgrade, and click-through edge cases.

---

## PHASE 6 — COMBAT VISUAL

- [ ] Projectile visible
- [ ] Hit effect visible
- [ ] Death effect visible

### Validation
- [ ] Combat logic unchanged
- [ ] Visual matches events

---

## PHASE 7 — WORLDGEN

### Generation
- [ ] Noise generator working
- [ ] Heightmap stable
- [ ] Seed deterministic

### Data
- [ ] GeneratedMapData valid
- [ ] TerrainSemanticType defined

### Adapter
- [ ] TerrainToGridAdapter works
- [ ] GridMap remains authority

### Integration
- [ ] RunStart uses generated map
- [ ] HQ spawns valid
- [ ] No blocked spawn

### Visual
- [ ] MapPresenter3D renders map
- [ ] Terrain matches gameplay grid

---

## PHASE 8 — SAVE / LOAD

- [ ] Seed saved
- [ ] Worldgen config saved
- [ ] No GameObject saved directly

### Load
- [ ] Map regenerates correctly
- [ ] Buildings restored
- [ ] NPC restored
- [ ] No duplicate views

---

## PHASE 9 — DEBUG & HARDENING

### Debug
- [ ] Grid overlay toggle
- [ ] Blocked/buildable visible
- [ ] Footprint debug visible
- [ ] NPC path debug visible
- [ ] Combat debug visible

### Stability
- [ ] No reverse dependency to View3D
- [ ] No core logic duplication
- [ ] No circular asmdef dependency

### Performance
- [ ] No heavy FindObjectOfType in runtime
- [ ] No allocation spikes in loops

---

## FINAL CHECK

- [ ] Game playable fully in 3D
- [ ] All systems stable (build, NPC, combat)
- [ ] Save/load works
- [ ] No major bugs
- [ ] Architecture clean

RESULT:
- [ ] PASS
- [ ] FAIL
