# V3D Runtime Verification Plan

Project: `E:\Projects\DemoSeasonalBastion3D`
Reference source: `E:\Projects\SeasonalBastionV2`

## Purpose

This document turns the migration checklist into a concrete runtime verification pass for the 3D target project.

Use it when editor/playmode verification becomes available.

Until then:
- treat checklist items as implemented or wired only when code/scene evidence exists
- do not promote stability claims without explicit runtime confirmation
- use this file to reduce re-discovery cost when returning to the project later

## Verification status language

- **implemented** = code/module exists
- **wired** = hookup path exists in code or scene setup
- **verified** = explicitly observed in editor/playmode/runtime behavior
- **pass** = verified and acceptable for current migration goals
- **fail** = verified and not acceptable

---

## Recommended verification order

1. P1 foundation 3D
2. P2 placement 3D
3. P3 building 3D
4. P5 selection and camera focus
5. P4 actor presentation parity
6. P6 combat visual
7. P7 save/load
8. P8 worldgen integration
9. P9 debug/hardening/performance

Reason: stabilize spatial/input/build loops before checking downstream systems.

---

## P1 - Foundation 3D verification

### 1. Mapping accuracy

**Goal**
- Confirm hovered and resolved cells match intended ground positions with no visible offset drift.

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Shared/Spatial/GridWorldSettings.cs`
- `Assets/_Game/Gameplay/World/View3D/Shared/Spatial/CellWorldMapper3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Shared/Spatial/WorldToCellResolver3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Shared/Spatial/GroundRaycastService.cs`

**Scene/runtime dependencies to inspect**
- terrain/ground collider present and hittable
- correct ground layer assignment
- runtime host initialized before interaction
- hover/highlight debug objects active

**Expected observable outcome**
- pointer over a cell highlights the same intended gameplay cell
- movement across borders changes selected/hovered cell at correct boundaries
- no systematic one-cell offset on map edges or corners

**Fail examples**
- hover shifts by one cell near borders
- hit on ground resolves outside expected cell
- top/bottom inversion appears inconsistent with visual map orientation

### 2. Camera movement and bounds

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Camera/StrategyCameraController3D.cs`

**Expected observable outcome**
- pan/zoom respond consistently
- camera bounds clamp to playable map region
- reset/focus to map center produces sensible framing

**Fail examples**
- camera escapes map bounds
- zoom causes unusable angle or clipping
- reset centers on wrong world region

### 3. Scene bootstrap stability

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Map/GameplaySceneInstaller3D.cs`
- `Assets/Scenes/DemoGameplayScene.unity`

**Current code/scene confidence**
- `DemoGameplayScene.unity` contains the expected core View3D controller set, including selection, preview, world view, strategy camera, camera focus, grid overlay, and hover debug components.
- `GameplaySceneInstaller3D` contains the intended auto-wiring path for these references.

**Expected observable outcome**
- scene references self-wire successfully
- no missing-reference spam on startup for core 3D loop
- major controllers initialize without manual repair

---

## P2 - Placement 3D verification

### 1. Hover and placement preview

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Preview/PlacementPreviewController3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Preview/PlacementGhostView3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Preview/FootprintOverlay3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Preview/PlacementHudView3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Preview/PlacementValidationDebug3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Selection/WorldSelectionController3D.cs`

**Expected observable outcome**
- preview appears on hovered cell only when placement mode is active
- preview footprint shape matches rotated building footprint
- valid/invalid colors change consistently with placement result
- suggested driveway/front marker lands on expected road-facing side

**Fail examples**
- preview appears shifted from hovered cell
- rotated footprint dimensions are swapped incorrectly
- driveway marker points to impossible or wrong side

### 2. Commit building through world input

**Primary code evidence**
- `PlacementPreviewController3D` calling gameplay placement runtime

**Expected observable outcome**
- committing placement creates expected build site/building state
- world view refreshes accordingly
- invalid placement does not commit

### 3. UI interaction boundaries

**Expected observable outcome**
- UI interactions do not accidentally place or select world objects
- click-through edge cases are understood and reproducible if present

**Known caution**
- placement commit has UI pointer guarding in code
- selection still requires explicit runtime verification for click-through edge cases

---

## P3 - Building 3D verification

### 1. Building/build-site lifecycle presentation

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Buildings/BuildingView3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Buildings/BuildingViewFactory3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Buildings/BuildingPrefabRegistry3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Buildings/ConstructionVisualController3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Map/WorldViewRoot3D.cs`

**Expected observable outcome**
- build sites appear when placement is committed
- completed buildings replace or transition cleanly from build sites
- duplicate views do not remain after lifecycle transitions
- fallback primitive visuals stay readable when real prefabs are absent

### 2. Remove and upgrade flow

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Selection/SelectionActionDebug3D.cs`

**Expected observable outcome**
- selecting a building or build site enables expected debug action
- destroy removes corresponding view and gameplay object cleanly
- upgrade produces expected runtime/build-order effect without duplicate visuals

---

## P5 - Selection and camera focus verification

### 1. World selection

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Selection/WorldSelectionController3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Selection/SelectedEntityBridge3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Selection/SelectionHighlight3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Selection/SelectionInspectHudView3D.cs`

**Expected observable outcome**
- clicking a building selects that building
- clicking a build site selects that site
- clicking empty ground clears selection
- selection highlight is obvious and tracks current selection correctly

### 2. Focus behavior

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Camera/CameraFocusController3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Camera/StrategyCameraController3D.cs`

**Expected observable outcome**
- focus key frames selected object or selected cell sensibly
- selection-change focus works correctly if enabled
- focus does not jump to incorrect anchors

---

## P4 - Actor presentation parity verification

### Scope note

Current code uses presenter-driven actor GameObjects in `WorldViewRoot3D`.
There is not currently a separate `NpcView3D` or `EnemyView3D` class.

### 1. NPC motion

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/NPC/NpcMovementPresenter3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Map/WorldViewRoot3D.cs`

**Expected observable outcome**
- NPCs appear reliably
- movement is readable and smooth enough for current prototype goals
- facing direction updates are believable and not obviously wrong

### 2. Enemy motion

**Primary code evidence**
- `Assets/_Game/Gameplay/World/View3D/Enemies/EnemyMovementPresenter3D.cs`
- `Assets/_Game/Gameplay/World/View3D/Map/WorldViewRoot3D.cs`

**Expected observable outcome**
- enemies appear reliably
- movement along lane direction is readable
- facing direction is consistent with motion toward HQ

### 3. Desync check

**Expected observable outcome**
- no obvious long-lived mismatch between gameplay cell state and presented actor position

---

## Deferred verification groups

These remain lower priority until P1/P2/P3/P5 are verified:

### P6 - Combat visual
- projectile visibility
- hit/death readability
- event timing match

### P7 - Save/load
- rebuild after load
- restoring building/NPC/enemy views
- duplicate view checks after load

### P8 - Worldgen
- seed determinism
- generated map validity
- terrain/grid semantic match

### P9 - Hardening
- runtime `FindObjectOfType` reduction
- allocation review
- asmdef dependency review
- compile-clean confirmation

---

## Suggested next patch after this doc

If runtime verification is still blocked, the next best patch is:

1. Add checklist links from each pending verify-heavy phase to this file
2. Convert high-risk pending items into explicit test cases with owner notes
3. Audit scene/prefab references for the expected verification path

If runtime verification becomes available, use this file as the execution script for the first editor/playmode pass.
