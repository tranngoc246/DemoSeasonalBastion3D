# V3D Migration Baseline

Source project audited: `E:\Projects\SeasonalBastionV2`
Target project: `E:\Projects\DemoSeasonalBastion3D`

## Phase 0 Summary

This baseline audit confirms the V2 project is currently structured around a 2D grid gameplay core with a separate 2D presentation layer. The 3D migration should preserve gameplay authority in core runtime systems and add 3D strictly as presentation.

## Main gameplay scene

- Main gameplay scene: `Assets/Scenes/Game.unity`
- Build settings include:
  - `Assets/Scenes/MainMenu.unity`
  - `Assets/Scenes/Game.unity`
- `Game.unity` is the active gameplay scene candidate for V3D migration.

## Protected gameplay authority modules

These are the core authority files/modules that should remain gameplay-owned during migration:

### Grid authority
- `Assets/_Game/Grid/GridMap.cs`
  - Single occupancy source for roads, buildings, and sites.
  - Grid dimensions and occupancy queries are authoritative.

### Placement authority
- `Assets/_Game/Grid/PlacementService.cs`
  - Owns placement validation for roads and buildings.
  - Owns footprint validation, overlap checks, buildable area checks, entry/driveway computation, and road connectivity validation.
  - Must remain the source of truth for placement validity in V3D.

### World runtime authority
- `Assets/_Game/World/State/WorldState.cs`
  - Holds authoritative runtime stores for buildings, NPCs, towers, enemies, sites, zones, and resource piles.

### Run/bootstrap authority
- `Assets/_Game/Core/RunStart/RunStartFacade.cs`
  - Entry point for parsing and applying run start config.
  - Delegates world build, zones, storage, HQ lanes, NPC spawn, and validation.

### Build lifecycle authority
- `Assets/_Game/Build/BuildOrderService.cs`
  - Owns place/upgrade/repair build order lifecycle.
  - Ticks active orders and coordinates creation, cancellation, completion, job orchestration, and repair flow.

## Related core modules worth treating as protected

Although not named in the prompt shortlist, these are tightly coupled runtime/core systems and should also stay presentation-independent:

- `Assets/_Game/Core/RunStart/RunStartWorldBuilder.cs`
- `Assets/_Game/World/Ops/WorldOps.cs`
- `Assets/_Game/World/Index/WorldIndexService.cs`
- `Assets/_Game/Grid/Navigation/NpcPathfinder.cs`
- `Assets/_Game/Grid/Navigation/GridAgentMoverLite.cs`
- `Assets/_Game/Combat/*`
- `Assets/_Game/Jobs/*`
- `Assets/_Game/Economy/*`
- `Assets/_Game/Save/*`

## Current presentation layer in V2

The current project already separates runtime from view to a useful degree.

### 2D world presentation root
- `Assets/_Game/World/View2D/WorldViewRoot2D.cs`
  - Polls/runtime-synchronizes roads, buildings, NPCs, and enemies.
  - Resolves services via reflection to avoid tight assembly coupling.
  - Is a good conceptual precedent for a future `View3D` layer.

### 2D placement/input presentation
- `Assets/_Game/Grid/PlacementInputController.cs`
  - Handles mouse-to-cell input mapping on a 2D plane.
  - Calls `PlacementService` for validation and commit.
  - Already keeps gameplay validation inside core service instead of duplicating rules in the controller.

## Cell/world axis assumptions

The current V2 gameplay scene is configured as a 2D XY grid, not XZ.

### Evidence
- `Assets/Scenes/Game.unity`
  - `WorldGrid` has `Grid.m_CellSize = (1,1,1)`
  - `PlacementInputController._useXZ = 0`
  - Multiple debug components use `_useXZ = 0`
  - `Main Camera` is orthographic
  - `WorldViewRoot2D` uses `Grid.GetCellCenterWorld()` for XY placement
- `Assets/_Game/Debug/DebugGridUtil.cs`
  - When `UseXZ == false`, world-to-cell maps:
    - world x -> grid x
    - world y -> grid y
    - world z stays as plane depth
- `Assets/_Game/Input/StrategyCameraController.cs`
  - Camera pans in XY space and focuses cells using `(x + 0.5, y + 0.5, z)`

### Current V2 mapping
- Grid X -> World X
- Grid Y -> World Y
- World Z is presentation depth for the 2D scene
- Cell size = `1`
- Grid origin is effectively `(0,0,0)` in the current scene setup

### V3D target mapping recommendation
For V3D migration, preserve gameplay cell coordinates and remap only presentation:
- Grid X -> World X
- Grid Y -> World Z
- World Y -> visual height

This matches the prompt guidance and avoids changing gameplay semantics.

## Scene/layout assumptions relevant to migration

### Existing gameplay scene objects
Observed in `Assets/Scenes/Game.unity`:
- `Main Camera` with orthographic projection
- `WorldGrid` root with child tilemaps:
  - `RoadTilemap`
  - `ResourceOverlayTilemap`
  - `PreviewTilemap`
- `WorldViewRoot2D`
- `PlacementInputController`
- Existing debug components using XY plane mapping

### Implication
The current scene is built around Unity Grid/Tilemap for 2D presentation. V3D should not mutate gameplay semantics to match 3D. Instead, V3D should sit beside or replace the 2D presentation path while continuing to consume the same authoritative runtime state.

## Assembly/dependency baseline

Project uses asmdefs, so V3D should be introduced with explicit one-way dependencies.

Existing major gameplay assemblies include:
- `Game.Contracts`
- `Game.Core`
- `Game.RunStart`
- `Game.Grid`
- `Game.World`
- `Game.Build`
- `Game.Combat`
- `Game.Jobs`
- `Game.Economy`
- `Game.Save`
- `Game.UI`
- `Game.Debug`

## Compile status

### Observed
- The project contains normal Unity `Library/Bee` and `Logs` output from a prior editor/build session.
- No fresh compiler execution was run during this audit.
- No immediate source-level syntax problems were observed in the key authority files inspected.

### Current baseline conclusion
- Baseline appears structurally compile-ready from inspected sources.
- Compile-clean status should still be verified in Unity editor or CI before Phase 1.
- Per migration rule, only small compile fixes should be applied if a real compiler error is confirmed.

## Phase 0 conclusions

1. `Game.unity` is the main gameplay scene.
2. Gameplay authority is centered in `GridMap`, `PlacementService`, `WorldState`, `RunStartFacade`, and `BuildOrderService`.
3. V2 currently uses XY world presentation with cell size 1.
4. V3D should convert presentation only, using XZ for ground and Y for visual height.
5. The existing `WorldViewRoot2D` and `PlacementInputController` provide a useful separation pattern, but must not become gameplay authorities.
6. No reverse dependency from gameplay core to future `View3D` should be introduced.

## Recommended next step for Phase 1

Proceed with:
- `T01` create `View3D` architecture shell
- `T02` add `GridWorldSettings` and `CellWorldMapper3D`

Do not modify protected core gameplay modules unless a real compile fix is required.
