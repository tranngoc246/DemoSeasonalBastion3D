# Seasonal Bastion V3D - Implementation Priority

## Framing

- Unity 2D source project: `E:\Projects\SeasonalBastionV2`
- Unity 3D target/prototype project: `E:\Projects\DemoSeasonalBastion3D`
- Main objective: migrate from a Unity 2D project to a playable Unity 3D project while preserving important gameplay behavior from V2 where practical.

## Priority Order

---

## P0 - Gameplay parity baseline

### Goal
Lock what must be preserved from V2 before pushing the 3D target further.

### Checklist
- [ ] Confirm the gameplay systems that act as parity anchors from V2
- [ ] Lock the behavior expectations for GridMap, PlacementService, WorldState, RunStartFacade, and BuildOrderService
- [ ] Identify which save/load behaviors must remain equivalent
- [ ] Identify which combat behaviors must remain equivalent
- [ ] Identify which NPC/path behaviors must remain equivalent
- [ ] Treat V2 as the comparison oracle for major gameplay behavior

### Why this priority exists
Without a parity baseline, the 3D project can drift away from the actual game very quickly.

---

## P1 - 3D runtime foundation

### Goal
Make the 3D target behave like a real Unity 3D runtime, not just a visual experiment.

### Checklist
- [ ] Verify `GridWorldSettings`
- [ ] Verify `CellWorldMapper3D`
- [ ] Verify `WorldToCellResolver3D`
- [ ] Eliminate mapping offset issues
- [ ] Lock stable ground layer + collider + raycast behavior
- [ ] Verify 3D camera pan/zoom/clamp behavior
- [ ] Verify basic 3D scene bootstrap stability

### Why this priority exists
If spatial mapping, raycast, and camera are not solid, every later feature becomes unreliable.

---

## P2 - 3D interaction loop

### Goal
Make core player interaction work properly in the 3D world.

### Checklist
- [ ] Hover cell matches the intended grid cell
- [ ] Placement preview works in 3D
- [ ] Footprint preview is correct
- [ ] Driveway/front marker preview is correct
- [ ] Click-build works through 3D world input
- [ ] World selection works in 3D
- [ ] Clicking empty space clears selection correctly
- [ ] UI click-through issues are eliminated

### Why this priority exists
This is the first real gameplay loop the player experiences in the 3D target.

---

## P3 - 3D world presentation core

### Goal
Make the 3D project show the game world clearly and reliably.

### Checklist
- [ ] BuildingView3D is stable
- [ ] BuildingViewFactory3D is stable
- [ ] Prefab registry is usable and extendable
- [ ] Construction visual state is clear
- [ ] Building remove flow works end-to-end
- [ ] Building upgrade flow works end-to-end
- [ ] No duplicate building/build-site views remain

### Why this priority exists
The 3D target has to become a readable world, not only a debug board.

---

## P4 - Actor presentation parity

### Goal
Make NPCs and enemies behave readably in 3D without breaking gameplay parity.

### Checklist
- [ ] NPC view/factory path is stable
- [ ] NPC movement presenter is stable
- [ ] NPC movement is smooth enough
- [ ] NPC rotation is correct enough
- [ ] Enemy view/factory path is stable
- [ ] Enemy movement presenter is stable
- [ ] Enemy movement is smooth enough
- [ ] No serious desync between logic and presentation
- [ ] Path behavior still respects V2 gameplay expectations

### Why this priority exists
Actors are where parity errors become visible immediately.

---

## P5 - UI bridge and camera focus

### Goal
Connect 3D interaction to the actual game UI flow.

### Checklist
- [ ] Selection updates the correct info panel state
- [ ] Selection highlight is clear and reliable
- [ ] Focus on selected entities works
- [ ] Focus from notifications works
- [ ] Selection and focus do not break UI interaction rules

### Why this priority exists
Without the UI bridge, the 3D world feels disconnected from the actual game loop.

---

## P6 - Combat readability

### Goal
Make combat readable in 3D without changing combat logic.

### Checklist
- [ ] Projectile visuals are visible
- [ ] Hit effects are visible
- [ ] Death effects are visible
- [ ] Visual events match combat events correctly
- [ ] Combat logic remains unchanged

### Why this priority exists
Combat feedback matters, but it should come after build/select/move flows are stable.

---

## P7 - Save/load parity in the 3D runtime

### Goal
Make the 3D target function like a real runtime, not only a fresh-play demo.

### Checklist
- [ ] Seed/worldgen config persistence is defined correctly
- [ ] Map rebuilds correctly after load
- [ ] Buildings restore correctly after load
- [ ] NPC views restore correctly after load
- [ ] Enemy views restore correctly after load
- [ ] No duplicate views appear after load
- [ ] No GameObject state is saved directly

### Why this priority exists
A real 3D target must survive load/rebuild flows, not only editor fresh starts.

---

## P8 - Terrain and worldgen integration

### Goal
Move from a flat debug board toward a real 3D world.

### Checklist
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

### Why this priority exists
Terrain/worldgen is important, but it should not outrank core interaction and gameplay readability.

---

## P9 - Debug, hardening, and performance

### Goal
Turn the 3D prototype into a safer long-term runtime base.

### Checklist
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

### Why this priority exists
Hardening should happen after the main 3D gameplay loop is genuinely working.

---

## Recommended execution sequence

- [ ] P1 - 3D runtime foundation
- [ ] P2 - 3D interaction loop
- [ ] P3 - 3D world presentation core
- [ ] P5 - UI bridge and camera focus
- [ ] P4 - Actor presentation parity
- [ ] P7 - Save/load parity in the 3D runtime
- [ ] P6 - Combat readability
- [ ] P8 - Terrain and worldgen integration
- [ ] P9 - Debug, hardening, and performance

## Summary

Priority is now driven by this rule:
- make the 3D target playable first
- preserve important gameplay behavior intentionally
- prefer interaction and runtime stability before terrain ambition and polish
