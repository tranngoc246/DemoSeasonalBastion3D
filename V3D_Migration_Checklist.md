# Seasonal Bastion V3D Migration Checklist

---

## PHASE 0 — BASELINE

- [ ] Project compiles without errors
- [ ] Core systems identified:
  - [ ] GridMap
  - [ ] PlacementService
  - [ ] WorldState
  - [ ] RunStartFacade
  - [ ] BuildOrderService
- [ ] Main gameplay scene confirmed
- [ ] Grid orientation defined (XZ or XY)
- [ ] Cell size documented
- [ ] Baseline doc created

---

## PHASE 1 — FOUNDATION 3D

### Spatial
- [ ] GridWorldSettings created
- [ ] CellWorldMapper3D working
- [ ] WorldToCellResolver3D working
- [ ] Mapping accurate (no offset)

### Camera
- [ ] StrategyCameraController3D works
- [ ] Pan/zoom stable
- [ ] Camera bounds applied

### Scene
- [ ] Ground plane exists
- [ ] Ground layer configured
- [ ] Raycast works

### Debug
- [ ] Hover cell debug visible
- [ ] Hover matches correct cell

---

## PHASE 2 — PLACEMENT 3D

- [ ] PlacementPreviewController3D created
- [ ] PlacementGhostView3D working
- [ ] FootprintOverlay3D correct
- [ ] Driveway marker visible

### Integration
- [ ] Uses PlacementService (no duplicate logic)
- [ ] Valid/invalid preview correct
- [ ] Click build works

### Validation
- [ ] Footprint correct
- [ ] Road adjacency correct
- [ ] Driveway correct

---

## PHASE 3 — BUILDING 3D

- [ ] BuildingView3D created
- [ ] BuildingViewFactory3D works
- [ ] Prefab registry exists

### Runtime
- [ ] Buildings spawn correctly
- [ ] Remove works
- [ ] Upgrade updates correctly
- [ ] No duplicate views

### Visual
- [ ] Correct position (center cell)
- [ ] Scale matches grid
- [ ] Construction state visible

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

- [ ] WorldSelectionController3D works
- [ ] Object selection accurate
- [ ] Click empty clears selection

### UI
- [ ] Selection updates info panel
- [ ] No UI click-through issues

### Camera
- [ ] Focus works on selection
- [ ] Focus works from notifications

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
