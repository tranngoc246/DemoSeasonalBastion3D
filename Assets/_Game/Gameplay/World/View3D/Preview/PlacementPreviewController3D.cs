using System.Collections.Generic;
using System.Text;
using SeasonalBastion.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using Mouse = UnityEngine.InputSystem.Mouse;

namespace SeasonalBastion
{
    public sealed class PlacementPreviewController3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _gameplayBootstrap;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private WorldViewRoot3D _worldView;
        [SerializeField] private string _activeBuildingDefId = "tower_arrow_l1";
        [SerializeField] private Dir4 _rotation = Dir4.N;
        [SerializeField] private KeyCode _rotateKey = KeyCode.R;
        [SerializeField] private KeyCode _confirmKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode _togglePlacementModeKey = KeyCode.P;
        [SerializeField] private KeyCode _nextBuildingKey = KeyCode.Tab;
        [SerializeField] private KeyCode _setTowerKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode _setHqKey = KeyCode.Alpha2;
        [SerializeField] private bool _placementMode = true;
        [SerializeField] private PlacementGhostView3D _ghostView;
        [SerializeField] private FootprintOverlay3D _footprintOverlay;
        [SerializeField] private bool _showDebugPlacementLabel = true;
        [SerializeField] private Vector2 _debugLabelScreenOffset = new(16f, 16f);

        private readonly List<GameObject> _footprintMarkers = new();
        private readonly StringBuilder _debugLabelBuilder = new();
        private readonly List<string> _availableBuildingIds = new();
        private GameObject _entryMarker;
        private PlacementResult _lastResult;
        private BuildingDef _lastDef;
        private string _debugPlacementText = string.Empty;

        public string DebugPlacementText => _debugPlacementText;
        public bool LastPlacementOk => _lastResult.Ok;
        public bool PlacementModeActive => _placementMode;

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
            HandleInput();
        }

        private void LateUpdate()
        {
            ResolveRefs();
            RefreshPreview();
        }

        private void OnGUI()
        {
            if (!_showDebugPlacementLabel || string.IsNullOrEmpty(_debugPlacementText))
                return;

            GUI.color = _lastResult.Ok ? Color.white : new Color(1f, 0.8f, 0.8f, 1f);
            GUI.Label(new Rect(_debugLabelScreenOffset.x, _debugLabelScreenOffset.y, 520f, 80f), _debugPlacementText);
            GUI.color = Color.white;
        }

        public void SetActiveBuilding(string buildingDefId)
        {
            TrySetActiveBuilding(buildingDefId);
        }

        public void SetRotation(Dir4 rotation)
        {
            _rotation = rotation;
        }

        public void SetPlacementMode(bool enabled)
        {
            _placementMode = enabled;
            if (!enabled)
                HideAll();
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_gameplayBootstrap == null)
                _gameplayBootstrap = FindObjectOfType<GameplayRuntimeBootstrap>();
            if (_selection == null)
                _selection = FindObjectOfType<WorldSelectionController3D>();
            if (_worldView == null)
                _worldView = FindObjectOfType<WorldViewRoot3D>();
            if (_ghostView == null)
                _ghostView = GetComponent<PlacementGhostView3D>();
            if (_footprintOverlay == null)
                _footprintOverlay = GetComponent<FootprintOverlay3D>();
        }

        private void HandleInput()
        {
            if (WasPressedThisFrame(_togglePlacementModeKey))
            {
                _placementMode = !_placementMode;
                if (!_placementMode)
                    HideAll();
            }

            ResolveAvailableBuildings();

            if (WasPressedThisFrame(_setTowerKey))
                TrySetActiveBuilding("tower_arrow_l1");
            if (WasPressedThisFrame(_setHqKey))
                TrySetActiveBuilding("hq_l1");
            if (WasPressedThisFrame(_nextBuildingKey))
                CycleBuilding(1);

            if (!_placementMode)
                return;

            if (WasPressedThisFrame(_rotateKey))
                _rotation = NextRotation(_rotation);

            bool confirmPressed = WasPressedThisFrame(_confirmKey);
            if (confirmPressed && !IsPointerOverUi())
                TryCommitPlacement();
        }

        private void RefreshPreview()
        {
            _lastDef = null;
            _lastResult = default;
            ResolveAvailableBuildings();

            if (!_placementMode || _runtimeHost == null || _runtimeHost.Mapper == null || _gameplayBootstrap == null || _selection == null)
            {
                _debugPlacementText = string.Empty;
                HideAll();
                return;
            }

            if (!_selection.HasHoveredCell)
            {
                _debugPlacementText = "Placement: hover a cell";
                HideAll();
                return;
            }

            if (_gameplayBootstrap.Data == null || !_gameplayBootstrap.Data.TryGetBuilding(_activeBuildingDefId, out var def) || def == null)
            {
                _debugPlacementText = "Placement: missing building def";
                HideAll();
                return;
            }

            _lastDef = def;
            _lastResult = _gameplayBootstrap.Placement.ValidateBuilding(_activeBuildingDefId, _selection.HoveredCell, _rotation);
            UpdateDebugPlacementText(_selection.HoveredCell, _rotation, _lastResult);
            GetFootprintSize(def, _rotation, out int footprintWidth, out int footprintHeight);
            EnsureFootprintMarkers(Mathf.Max(1, footprintWidth * footprintHeight));

            int markerIndex = 0;
            for (int dy = 0; dy < footprintHeight; dy++)
            {
                for (int dx = 0; dx < footprintWidth; dx++)
                {
                    CellPos cell = new(_selection.HoveredCell.X + dx, _selection.HoveredCell.Y + dy);
                    GameObject marker = _footprintMarkers[markerIndex++];
                    bool inside = _runtimeHost.Mapper.IsInside(cell);
                    marker.SetActive(true);
                    _ghostView?.PlaceCellMarker(marker, _runtimeHost.Mapper, cell, inside && _lastResult.Ok);
                }
            }

            for (int i = markerIndex; i < _footprintMarkers.Count; i++)
                _footprintMarkers[i].SetActive(false);

            EnsureEntryMarker();
            bool showEntry = _runtimeHost.Mapper.IsInside(_lastResult.SuggestedRoadCell);
            _entryMarker.SetActive(showEntry);
            if (showEntry)
                _footprintOverlay?.PlaceEntryMarker(_entryMarker, _runtimeHost.Mapper, _lastResult.SuggestedRoadCell);
        }

        private void TryCommitPlacement()
        {
            if (_gameplayBootstrap?.Placement == null || _selection == null || !_selection.HasHoveredCell)
            {
                Debug.Log("[PlacementPreviewController3D] Commit skipped: placement runtime or hovered cell unavailable.", this);
                return;
            }
            if (_lastDef == null || !_lastResult.Ok)
            {
                Debug.Log($"[PlacementPreviewController3D] Commit skipped: invalid placement for '{_activeBuildingDefId}', reason={_lastResult.FailReason}.", this);
                return;
            }

            CellPos anchor = _selection.HoveredCell;
            BuildingId id = _gameplayBootstrap.Placement.CommitBuilding(_activeBuildingDefId, anchor, _rotation);
            if (id.Value == 0)
            {
                Debug.Log($"[PlacementPreviewController3D] Commit returned no building for '{_activeBuildingDefId}' at ({anchor.X},{anchor.Y}).", this);
                return;
            }

            int siteCount = _gameplayBootstrap.World != null ? _gameplayBootstrap.World.Sites.Count : -1;
            int buildingCount = _gameplayBootstrap.World != null ? _gameplayBootstrap.World.Buildings.Count : -1;
            Debug.Log($"[PlacementPreviewController3D] Commit created building #{id.Value} for '{_activeBuildingDefId}' at ({anchor.X},{anchor.Y}). buildings={buildingCount}, sites={siteCount}", this);

            _worldView?.RefreshAll();
        }

        private static void GetFootprintSize(BuildingDef def, Dir4 rotation, out int width, out int height)
        {
            int sizeX = Mathf.Max(1, def.SizeX);
            int sizeY = Mathf.Max(1, def.SizeY);
            bool swap = rotation == Dir4.E || rotation == Dir4.W;
            width = swap ? sizeY : sizeX;
            height = swap ? sizeX : sizeY;
        }

        private void EnsureFootprintMarkers(int count)
        {
            while (_footprintMarkers.Count < count)
                _footprintMarkers.Add(_ghostView != null ? _ghostView.CreateMarker(transform, $"PlacementMarker_{_footprintMarkers.Count}") : CreateFallbackMarker($"PlacementMarker_{_footprintMarkers.Count}"));
        }

        private void EnsureEntryMarker()
        {
            if (_entryMarker != null)
                return;

            _entryMarker = _footprintOverlay != null ? _footprintOverlay.CreateEntryMarker(transform, "PlacementEntryMarker") : CreateFallbackMarker("PlacementEntryMarker");
        }

        private GameObject CreateFallbackMarker(string name)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            Collider col = go.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            Renderer renderer = go.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
                renderer.sharedMaterial = new Material(shader);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private void HideAll()
        {
            for (int i = 0; i < _footprintMarkers.Count; i++)
                _footprintMarkers[i].SetActive(false);
            if (_entryMarker != null)
                _entryMarker.SetActive(false);
        }

        private void UpdateDebugPlacementText(CellPos hoveredCell, Dir4 rotation, PlacementResult result)
        {
            _debugLabelBuilder.Clear();
            _debugLabelBuilder.Append("[P] place ");
            _debugLabelBuilder.Append(_placementMode ? "ON" : "OFF");
            _debugLabelBuilder.Append(" | [1] Tower [2] HQ [Tab] next [R] rotate [RMB] place\n");
            _debugLabelBuilder.Append("Placement ");
            _debugLabelBuilder.Append(result.Ok ? "OK" : "FAIL");
            _debugLabelBuilder.Append(" | def=");
            _debugLabelBuilder.Append(_activeBuildingDefId);
            _debugLabelBuilder.Append(" | anchor=(");
            _debugLabelBuilder.Append(hoveredCell.X);
            _debugLabelBuilder.Append(',');
            _debugLabelBuilder.Append(hoveredCell.Y);
            _debugLabelBuilder.Append(") | rot=");
            _debugLabelBuilder.Append(rotation);
            _debugLabelBuilder.Append(" | entry=(");
            _debugLabelBuilder.Append(result.SuggestedRoadCell.X);
            _debugLabelBuilder.Append(',');
            _debugLabelBuilder.Append(result.SuggestedRoadCell.Y);
            _debugLabelBuilder.Append(')');

            if (!result.Ok)
            {
                _debugLabelBuilder.Append(" | reason=");
                _debugLabelBuilder.Append(result.FailReason);
            }

            _debugPlacementText = _debugLabelBuilder.ToString();
        }

        private void ResolveAvailableBuildings()
        {
            _availableBuildingIds.Clear();
            if (_gameplayBootstrap?.Data == null)
                return;

            AddIfExists("tower_arrow_l1");
            AddIfExists("hq_l1");

            if (_availableBuildingIds.Count == 0)
                _activeBuildingDefId = string.Empty;
            else if (!_availableBuildingIds.Contains(_activeBuildingDefId))
                _activeBuildingDefId = _availableBuildingIds[0];
        }

        private void AddIfExists(string buildingDefId)
        {
            if (_gameplayBootstrap != null
                && _gameplayBootstrap.Data != null
                && _gameplayBootstrap.Data.TryGetBuilding(buildingDefId, out var def)
                && def != null)
            {
                _availableBuildingIds.Add(buildingDefId);
            }
        }

        private void TrySetActiveBuilding(string buildingDefId)
        {
            if (string.IsNullOrWhiteSpace(buildingDefId))
                return;

            if (_gameplayBootstrap?.Data != null && _gameplayBootstrap.Data.TryGetBuilding(buildingDefId, out var def) && def != null)
                _activeBuildingDefId = buildingDefId;
        }

        private void CycleBuilding(int direction)
        {
            if (_availableBuildingIds.Count == 0)
                return;

            int currentIndex = _availableBuildingIds.IndexOf(_activeBuildingDefId);
            if (currentIndex < 0)
            {
                _activeBuildingDefId = _availableBuildingIds[0];
                return;
            }

            int nextIndex = (currentIndex + direction) % _availableBuildingIds.Count;
            if (nextIndex < 0)
                nextIndex += _availableBuildingIds.Count;
            _activeBuildingDefId = _availableBuildingIds[nextIndex];
        }

        private static bool IsPointerOverUi()
        {
            return UnityEngine.EventSystems.EventSystem.current != null
                && Mouse.current != null
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        private static bool WasPressedThisFrame(KeyCode key)
        {
            if (Mouse.current != null)
            {
                switch (key)
                {
                    case KeyCode.Mouse0:
                        return Mouse.current.leftButton.wasPressedThisFrame;
                    case KeyCode.Mouse1:
                        return Mouse.current.rightButton.wasPressedThisFrame;
                    case KeyCode.Mouse2:
                        return Mouse.current.middleButton.wasPressedThisFrame;
                }
            }

            if (Keyboard.current == null)
                return false;

            return key switch
            {
                KeyCode.R => Keyboard.current.rKey.wasPressedThisFrame,
                KeyCode.P => Keyboard.current.pKey.wasPressedThisFrame,
                KeyCode.Tab => Keyboard.current.tabKey.wasPressedThisFrame,
                KeyCode.Alpha1 => Keyboard.current.digit1Key.wasPressedThisFrame,
                KeyCode.Alpha2 => Keyboard.current.digit2Key.wasPressedThisFrame,
                KeyCode.Space => Keyboard.current.spaceKey.wasPressedThisFrame,
                KeyCode.Return => Keyboard.current.enterKey.wasPressedThisFrame,
                _ => false,
            };
        }

        private static Dir4 NextRotation(Dir4 rotation)
        {
            return rotation switch
            {
                Dir4.N => Dir4.E,
                Dir4.E => Dir4.S,
                Dir4.S => Dir4.W,
                _ => Dir4.N,
            };
        }
    }
}
