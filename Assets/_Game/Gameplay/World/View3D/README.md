# View3D

3D presentation layer shell for Seasonal Bastion V3D.

Rules:
- Gameplay authority remains in core gameplay systems.
- View3D reads gameplay/runtime state and presents it in 3D.
- Do not duplicate placement, pathfinding, combat, or world validation logic here.
- Do not introduce reverse dependency from gameplay core back into View3D.

Initial folders:
- Camera
- Input
- Map
- Buildings
- NPC
- Enemies
- Preview
- Selection
- VFX
- Shared/Spatial
