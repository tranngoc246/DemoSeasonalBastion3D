# View3D Structure

This folder is organized by presentation responsibility, not gameplay ownership.

## Intended layout

- `Camera`
  - 3D camera controllers and focus helpers
- `Input`
  - world raycast/input adapters that feed presentation controllers
- `Map`
  - map presenters, scene installers, world roots, terrain surface presenters
- `Buildings`
  - building views, factories, prefab registries, construction visuals
- `NPC`
  - npc views and movement presenters
- `Enemies`
  - enemy views and movement presenters
- `Preview`
  - placement preview, ghost, footprint, debug hover/cell highlight
- `Selection`
  - world picking, highlight, selection-to-UI bridge
- `VFX`
  - visual-only combat and feedback effects
- `Shared/Spatial`
  - reusable grid/world mapping helpers, spatial config, raycast helpers

## Notes

- Keep gameplay authority in gameplay/core systems.
- View3D should consume runtime state and services, not own gameplay rules.
- Shared spatial helpers should stay engine-light when possible.
- Empty folders are intentional as migration placeholders for future tasks.
