# SeasonalBastion V3D – OpenClaw Task Prompts

You are a Unity technical lead working on migrating Seasonal Bastion from the canonical 2D project `E:\Projects\SeasonalBastionV2` to a 3D presentation layer (`View3D`). Use `E:\Projects\DemoSeasonalBastion3D` only as a migration sandbox/reference implementation, not as the source of truth for gameplay architecture.

Migration framing:
- `SeasonalBastionV2` is the canonical gameplay/runtime source.
- `Assets\_Game\World\View2D` in V2 is the old presentation layer.
- `View3D` is the new presentation layer to port back onto V2 runtime.
- Reuse ideas and selected implementations from DemoSeasonalBastion3D only when they fit V2 module boundaries cleanly.
- Do not let demo-specific structure or shortcuts become the new gameplay authority.

Architecture constraints:
- Core gameplay must remain unchanged:
  GridMap, PlacementService, WorldState, RunStartFacade, BuildOrderService.
- 3D must be a presentation layer only (View3D).
- Never introduce reverse dependency from gameplay core to View3D.
- GridMap is the single source of truth for gameplay.
- Do not duplicate gameplay validation logic in View3D.

Coding rules:
- Prefer adding new modules/classes instead of modifying core.
- Keep changes minimal, incremental, and compile-safe.
- Avoid large refactors.
- Avoid FindObjectOfType unless debug-only.
- Avoid breaking public contracts unless required.

Output rules:
- Only include relevant files (no noise).
- Show full file content for new files.
- For modified files, show only necessary changes.
- Code must compile.

Goal:
Implement the requested feature safely without breaking existing systems, with `SeasonalBastionV2` remaining the gameplay source of truth and `View3D` being ported onto that runtime.

## Global prefix (optional)
Use this before any task if you want a shared constraint:

```text
Keep V2 core intact. Port View3D onto V2 runtime. Minimal compile-safe patch. No reverse dependency to View3D.
```

---

## PHASE 0

### T00 — Baseline
```text
Audit `E:\Projects\SeasonalBastionV2` before 3D migration. Fix only small compile issues if any. Identify gameplay authority files and module boundaries: GridMap, PlacementService, WorldState, RunStartFacade, BuildOrderService, plus the main runtime assemblies that own save/load, combat, and population behavior. Confirm `Assets\_Game\World\View2D` is the legacy presentation layer. Create Docs/V3D_Migration_Baseline.md summarizing main scene, cell/world axis assumptions, protected core modules, and where `View3D` should attach. Do not add new features.
```

---

## PHASE 1

### T01 — Create View3D structure
```text
Create the 3D architecture shell inside `E:\Projects\SeasonalBastionV2`, not as a new gameplay source. Add folders: View3D/Camera, Input, Map, Buildings, NPC, Enemies, Preview, Selection, VFX, and Shared/Spatial. If asmdefs are used, add compile-safe asmdefs for View3D/Shared with dependencies only toward V2 core/runtime modules. No circular dependency.
```

### T02 — GridWorldSettings + CellWorldMapper3D
```text
Add GridWorldSettings and CellWorldMapper3D in Shared/Spatial. Use grid x -> world x, grid y -> world z, world y as visual height. Expose helpers for cell center, cell corner, and footprint world bounds. Keep it engine-light and reusable.
```

### T03 — WorldToCell + raycast
```text
Add GroundRaycastService and WorldToCellResolver3D. Raycast only against a dedicated ground layer. Convert world position to grid cell safely, including fail/out-of-bounds cases. Keep logic separate from gameplay systems.
```

### T04 — StrategyCameraController3D
```text
Add StrategyCameraController3D for RTS-style camera: pan, zoom, optional light rotation, and clamp to bounds/config. Keep it independent from gameplay logic and scene-specific hacks.
```

### T05 — Minimal 3D scene setup
```text
Create minimal 3D gameplay scene support: flat ground root, camera rig, and bootstrap/helper script if needed for testing raycast and mapping. Use a dedicated ground layer. Keep changes minimal and compile-safe.
```

### T06 — Hover cell debug
```text
Add HoverCellDebug3D to test 3D mapping. Read mouse raycast on ground and show current hovered grid cell using simple gizmo/log/overlay. This is only for debug validation of world-to-cell mapping.
```

---

## PHASE 2

### T07 — PlacementPreviewController3D
```text
Add PlacementPreviewController3D. It should receive selected build definition, current hovered cell, and validation result from existing PlacementService. Do not duplicate gameplay validation logic inside View3D.
```

### T08 — Ghost + footprint
```text
Add PlacementGhostView3D and FootprintOverlay3D. Optional: DrivewayMarker3D. Support valid/invalid visual state, footprint cell display, and basic entry/driveway marker display. Keep visuals simple and placeholder-friendly.
```

### T09 — Wire preview to PlacementService
```text
Wire PlacementPreviewController3D to existing PlacementService. Hovered cell should query real validation and update ghost/footprint visuals. No copied rules in View3D. Preview must reflect real placement validity.
```

### T10 — Click-build in 3D
```text
Add 3D build input flow. Mouse click on ground -> resolve cell -> send build request through existing placement/build flow. Do not bypass PlacementService or BuildOrderService. Keep road/driveway/footprint rules unchanged.
```

---

## PHASE 3

### T11 — BuildingView3D + factory
```text
Add BuildingView3D and BuildingViewFactory3D. Spawn prefab views from existing building/site runtime data using CellWorldMapper3D. Keep view separate from building logic. Add clean prefab registry/config pattern.
```

### T12 — Sync building views from world/events
```text
Wire BuildingViewFactory3D to WorldState/EventBus so building/site views spawn, despawn, and update from runtime events. Cover build, remove, upgrade, and construction-complete flow. Avoid duplicate view instances.
```

### T13 — Basic prefab registry
```text
Add a basic BuildingPrefabRegistry3D mapping for at least HQ, Road, Warehouse, and one Tower. Use placeholders/primitives if real assets are missing. Keep registry simple and easy to extend.
```

### T14 — Construction visual
```text
Add ConstructionVisualController3D so under-construction sites look different from completed buildings. Keep it minimal: scaffold, placeholder mesh swap, scale state, or other lightweight visual distinction.
```

---

## PHASE 4

### T15 — NPC view + factory
```text
Add NpcView3D and NpcViewFactory3D. Create visual objects for runtime NPCs/workers without changing NPC logic. Keep binding clean and separate from gameplay state.
```

### T16 — NPC movement presenter
```text
Add NpcMovementPresenter3D. Read existing NPC movement/path state, convert cells to world positions, and interpolate movement/rotation smoothly. Do not modify core pathfinding or job logic.
```

### T17 — Enemy view + factory
```text
Add EnemyView3D and EnemyViewFactory3D. Create visual objects for runtime enemies while keeping combat/enemy logic untouched.
```

### T18 — Enemy movement presenter
```text
Add EnemyMovementPresenter3D. Smoothly present enemy movement in 3D using existing path/runtime state. Do not change gameplay movement rules.
```

---

## PHASE 5

### T19 — WorldSelectionController3D
```text
Add WorldSelectionController3D. Support selecting buildings first, then make design ready for NPC/enemy selection too. Use raycast/collider/layer-based picking. Clicking empty ground should clear selection.
```

### T20 — Selection highlight + entity bridge
```text
Add SelectionHighlight3D and SelectedEntityBridge3D. Selected world object should highlight clearly and map back to the correct runtime entity. Keep bridge robust and simple.
```

### T21 — Wire selection to UI
```text
Wire 3D selection into existing UI/info panel flow. Clicking a building in 3D should populate the current info panel without rewriting the full UI system.
```

### T22 — Camera focus
```text
Add CameraFocusController3D so camera can focus selected entities and notification targets. Keep it decoupled from gameplay logic and reusable from UI/selection systems.
```

---

## PHASE 6

### T23 — Minimal combat VFX
```text
Add minimal 3D combat feedback: ProjectileView3D, HitEffect3D, DeathEffect3D, and optional TowerFireVfx3D. Keep it event-driven and visual only. Do not change combat logic.
```

---

## PHASE 7

### T24 — Create WorldGen module
```text
Create WorldGen module structure in V2: Runtime/Models, Generators, Classification, Conversion, and Authoring/Configs. Add compile-safe asmdef if project uses asmdefs. Keep module independent from View3D.
```

### T25 — Port noise/height/falloff from Demo
```text
Port world generation data logic from `E:\Projects\DemoSeasonalBastion3D` into V2 WorldGen only where it fits cleanly: noise, height map, falloff/shape generation, and request/result models. Remove demo-specific scene/runtime host dependencies. Keep generation deterministic by seed.
```

### T26 — TerrainSemanticType + GeneratedMapData
```text
Add TerrainSemanticType and GeneratedMapData. Include width, height, height values, terrain type, walkable, buildable, and blocked data. This is the semantic bridge between WorldGen and gameplay grid.
```

### T27 — TerrainToGridAdapter
```text
Add TerrainToGridAdapter that converts GeneratedMapData into gameplay grid flags for blocked/buildable/walkable/resource semantics. GridMap must remain the gameplay authority. No second source of truth.
```

### T28 — Wire WorldGen into RunStart
```text
Integrate WorldGen into RunStart. Use seed/config to generate map data before HQ/resources/roads placement. Extend RunStartFacade/RunStartWorldBuilder/StartMapConfigDto only as needed. HQ must spawn on valid cells.
```

### T29 — MapPresenter3D / TerrainSurfacePresenter3D
```text
Add MapPresenter3D and TerrainSurfacePresenter3D to render a simple 3D map/ground from generated semantic data. Prioritize correctness with gameplay grid over visual fidelity. Use simple visuals first.
```

---

## PHASE 8

### T30 — Save/load seed + worldgen config
```text
Extend save/load to persist world generation seed and config identifiers needed to rebuild the same map. Do not save GameObject/view state. Keep serialization changes minimal.
```

### T31 — Rebuild 3D views after load
```text
After load, rebuild the 3D world correctly from restored runtime state: map presenter, building views, NPC views, enemy views. Prevent missing or duplicated views after load.
```

---

## PHASE 9

### T32 — Grid overlay debug
```text
Add GridOverlay3D with toggles for grid, blocked cells, buildable cells, footprint, and driveway debug display. Keep it developer-oriented and runtime-toggleable.
```

### T33 — Path/combat/build debug
```text
Add basic 3D debug renderers for pathfinding, combat ranges/hits, and build state visualization. Keep them optional and isolated from gameplay logic.
```

### T34 — Final dependency cleanup
```text
Review the full V3D migration architecture. Clean asmdef/dependency issues, remove reverse coupling to View3D, reduce risky scene hacks, and create Docs/V3D_Migration_Review.md with final notes and remaining risks.
```

---

## Ultra-short version

### T00–T06
```text
T00: Audit V2 baseline. Fix only tiny compile issues. Document core authority files in Docs/V3D_Migration_Baseline.md.

T01: Create folders/asmdefs for View3D and Shared/Spatial. No reverse dependency to core.

T02: Add GridWorldSettings and CellWorldMapper3D. grid x->world x, grid y->world z.

T03: Add GroundRaycastService and WorldToCellResolver3D using ground layer only.

T04: Add StrategyCameraController3D with pan/zoom/clamp.

T05: Setup minimal 3D test scene with ground + camera.

T06: Add HoverCellDebug3D to show hovered grid cell from mouse raycast.
```

### T07–T14
```text
T07: Add PlacementPreviewController3D. Use existing PlacementService only.

T08: Add PlacementGhostView3D, FootprintOverlay3D, optional DrivewayMarker3D.

T09: Wire preview to PlacementService real validation.

T10: Add click-build in 3D through existing placement/build flow.

T11: Add BuildingView3D and BuildingViewFactory3D.

T12: Sync building views from WorldState/EventBus.

T13: Add basic prefab registry for HQ, Road, Warehouse, Tower.

T14: Add ConstructionVisualController3D for under-construction state.
```

### T15–T23
```text
T15: Add NpcView3D and NpcViewFactory3D.

T16: Add NpcMovementPresenter3D with smooth interpolation.

T17: Add EnemyView3D and EnemyViewFactory3D.

T18: Add EnemyMovementPresenter3D.

T19: Add WorldSelectionController3D.

T20: Add SelectionHighlight3D and SelectedEntityBridge3D.

T21: Wire selection to existing UI info panel.

T22: Add CameraFocusController3D.

T23: Add minimal combat VFX: projectile, hit, death.
```

### T24–T34
```text
T24: Create WorldGen module structure.

T25: Port noise/height/falloff worldgen logic from Demo.

T26: Add TerrainSemanticType and GeneratedMapData.

T27: Add TerrainToGridAdapter. GridMap stays authority.

T28: Wire WorldGen into RunStart. HQ must spawn valid.

T29: Add MapPresenter3D and TerrainSurfacePresenter3D.

T30: Extend save/load with seed + worldgen config.

T31: Rebuild 3D views correctly after load.

T32: Add GridOverlay3D debug toggles.

T33: Add path/combat/build debug visualizers.

T34: Final architecture cleanup and Docs/V3D_Migration_Review.md.
```
