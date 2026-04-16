# T02 Spatial Audit

Project: `E:\Projects\DemoSeasonalBastion3D`
Date: 2026-04-11

## Scope

Deep audit of the existing 3D spatial stack before implementing T02:
- `CellWorldMapper3D`
- `TerrainGameplayRuntimeHost`
- View3D consumers of mapper APIs

Goal: compare the current implementation against T02 requirements.

## T02 target requirement

T02 requires:
- add `GridWorldSettings`
- add `CellWorldMapper3D` in `Shared/Spatial`
- mapping must be:
  - grid x -> world x
  - grid y -> world z
  - world y = visual height
- expose reusable helpers for:
  - cell center
  - cell corner
  - footprint world bounds
- keep it engine-light and reusable

## Current implementation overview

The project already contains an existing mapper:
- `Assets/_Game/Gameplay/World/View3D/Shared/Spatial/CellWorldMapper3D.cs`

Current public API:
- `CellSize`
- `Width`
- `Height`
- `CellToWorldCenter(CellPos)`
- `WorldToCell(Vector3)`
- `GetHeightAtCell(CellPos)`
- `GetAverageHeightForFootprint(CellPos, int, int)`
- `FootprintToWorldCenter(CellPos, int, int)`
- `IsInside(CellPos)`

It is actively used by:
- `TerrainGameplayRuntimeHost`
- `StrategyCameraController3D`
- `WorldViewRoot3D`
- `CellHighlightView3D`
- `PlacementPreviewController3D`
- `WorldSelectionController3D`
- `TerrainGameplayDebugGizmos`

## What is already good

### 1. Presentation mapping uses XZ ground space
The current mapper already projects gameplay cells into XZ space:
- world x from grid x
- world z from grid y
- world y from terrain height

This matches the intended V3D direction conceptually.

### 2. Mapper is already shared across multiple 3D systems
The mapper is not embedded in a single controller. It is consumed by camera, selection, preview, world view, and debug systems. That is good architecture directionally.

### 3. Gameplay authority is still outside the mapper
Placement validity still comes from gameplay systems. The mapper is used for positioning and spatial conversion only.

## Gaps and risks vs T02

### A. Missing `GridWorldSettings`
Current mapper depends on:
- `WorldMeshSettings`
- `WorldGenerationResult`
- `Vector3 origin`

This means it is tightly coupled to world generation and terrain mesh authoring.

#### Why this is a problem
T02 expects a reusable spatial layer. Right now the mapper cannot be reused cleanly for:
- flat test maps
- non-worldgen maps
- debug-only scenes
- future runtime setups without `WorldGenerationResult`

#### Risk
Spatial mapping becomes coupled to one terrain generation path, instead of being a general View3D utility.

## B. Mapper is not engine-light enough
The mapper currently depends on:
- `UnityEngine.Vector3`
- `WorldMeshSettings`
- `WorldGenerationResult`
- `TerrainCellData`

Using `Vector3` is fine. The heavier problem is dependency on worldgen domain objects for basic cell conversion.

#### Why this matters
Basic grid-to-world conversion should not require heightmap/worldgen runtime data just to answer:
- where is the center of cell (x,y)?
- what is the world-space corner of a cell?
- what are footprint bounds for a building?

## C. Missing explicit helper for cell corner
T02 explicitly wants cell center and cell corner helpers.

Current mapper provides:
- center
- world-to-cell
- footprint center

But it does not provide a clear cell-corner API.

## D. Missing explicit helper for footprint world bounds
T02 explicitly wants footprint world bounds.

Current mapper provides:
- `FootprintToWorldCenter(...)`
- `GetAverageHeightForFootprint(...)`

But it does not provide a reusable footprint bounds representation such as:
- min/max world corners
- center + size
- `Bounds`

This makes future preview/selection/building placement code more likely to recompute spatial data ad hoc.

## E. World-to-cell uses rounding instead of floor-safe cell resolution
Current code:
- `WorldToCell(Vector3 world)` uses `Mathf.RoundToInt(...)`

#### Why this is risky
For grid cell picking, rounding can flip to adjacent cells near borders and is usually less stable than floor-based conversion for cell occupancy logic.

This is especially risky for:
- mouse hover
- raycast hit conversion
- selection near edges
- preview placement on sloped terrain

#### Recommendation
Use floor-based cell resolution against a clearly defined origin/corner convention, then clamp or reject out-of-bounds.

## F. Axis orientation includes implicit inverted Y-to-Z mapping
Current mapper uses:
- positive grid Y -> decreasing world Z

That is not inherently wrong, but it is hidden inside `_gridOriginOffset` and subtraction logic.

#### Why this matters
This makes the axis convention harder to reason about and easier to break when adding:
- minimap
- camera bounds
- world corner helpers
- footprint bounds
- debug overlays

T02 should make axis convention explicit in settings and helper methods.

## G. Camera bounds likely do not fully match mapper origin convention
`StrategyCameraController3D` mixes:
- `GridMap.Width * CellSize`
- `GridMap.Height * CellSize`
- mapper world conversion

The mapper is centered around a world origin offset, but camera bounds appear partly expressed in direct width/height extents.

#### Risk
Bounds can drift from the actual terrain/map extents depending on origin and centering.

This is not strictly a T02 failure, but it is a signal that mapping helpers should expose world footprint/bounds more clearly.

## H. Selection raycast depends on terrain collider and raw hit point conversion
`WorldSelectionController3D` currently:
- raycasts physics world
- converts hit point through `Mapper.WorldToCell()`

This is okay as an interim approach, but because `WorldToCell` uses rounding and mapper depends on terrain height data, picking behavior is more fragile than it needs to be.

## I. Runtime host owns mapper creation directly from worldgen settings
`TerrainGameplayRuntimeHost.Initialize()` currently creates mapper like this:
- `new CellWorldMapper3D(_meshSettings, GeneratedWorld, _worldOrigin)`

This means spatial mapping is currently a byproduct of terrain worldgen setup, not a first-class spatial configuration.

#### Why this matters
For T02, spatial configuration should become explicit and readable. That is exactly what `GridWorldSettings` should fix.

## Assessment summary

### Current status
The existing mapper is:
- functional
- already integrated
- directionally correct for XZ presentation

But it is also:
- too coupled to worldgen
- missing some required helper APIs
- ambiguous in axis/corner convention
- risky in `WorldToCell()` because of rounding

## Recommended safe T02 refactor

### Keep
Keep the existing `CellWorldMapper3D` file and its role, because many systems already depend on it.

### Add
Add `GridWorldSettings` in `Shared/Spatial` as the stable spatial contract.

Suggested responsibilities:
- cell size
- world origin
- axis convention documentation
- optional map anchor convention
- optional visual height base offset

### Refactor mapper to depend primarily on `GridWorldSettings`
Prefer constructor shape conceptually closer to:
- `CellWorldMapper3D(GridWorldSettings settings, WorldGenerationResult world)`

Or, even better if keeping height optional:
- `CellWorldMapper3D(GridWorldSettings settings, int width, int height, ICellHeightProvider optional)`

For minimal patch safety, the first option is more realistic.

### Add missing helper APIs
Add at least:
- `CellToWorldCorner(CellPos cell)`
- `GetFootprintWorldBounds(CellPos anchor, int sizeX, int sizeY)`

Possibly also:
- `CellToWorldCenterFlat(CellPos cell)` if consumers sometimes need terrain-independent XZ mapping
- `TryWorldToCell(Vector3 world, out CellPos cell)`

### Change world-to-cell behavior
Change conversion to floor-based resolution with an explicit cell-corner convention.

### Keep height as optional visual data, not mapping ownership
The mapper can still sample worldgen height for visual Y, but that should be clearly separate from base XZ mapping.

## Minimal implementation strategy

1. Add `GridWorldSettings`
2. Refactor `CellWorldMapper3D` constructor to consume settings + world result
3. Preserve existing methods where possible to avoid breaking current 3D systems
4. Add new helper methods rather than rewriting all consumers at once
5. Update `TerrainGameplayRuntimeHost` to instantiate mapper through the new settings object
6. Leave camera/selection/world view mostly untouched for now unless required by compile changes

## Final recommendation

Do not delete the current mapper and start over.

Best path:
- normalize the existing mapper into the T02 architecture
- introduce `GridWorldSettings` as the missing abstraction
- add the required helper methods
- reduce worldgen coupling without breaking current 3D consumers

That gives the cleanest path into T03 and later phases without a risky rewrite.
