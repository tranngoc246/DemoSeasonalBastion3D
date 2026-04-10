using System.Collections.Generic;
using SeasonalBastion.Contracts;
using UnityEngine;

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
        [SerializeField] private bool _placementMode = true;
        [SerializeField] private Color _validColor = new(0.2f, 1f, 0.35f, 0.45f);
        [SerializeField] private Color _invalidColor = new(1f, 0.25f, 0.25f, 0.45f);
        [SerializeField] private Color _entryRoadColor = new(1f, 0.9f, 0.15f, 0.55f);
        [SerializeField] private float _heightOffset = 0.12f;
        [SerializeField] private float _cellFill = 0.9f;

        private readonly List<GameObject> _footprintMarkers = new();
        private GameObject _entryMarker;
        private PlacementResult _lastResult;
        private BuildingDef _lastDef;

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

        public void SetActiveBuilding(string buildingDefId)
        {
            _activeBuildingDefId = buildingDefId;
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
        }

        private void HandleInput()
        {
            if (!_placementMode)
                return;

            if (Input.GetKeyDown(_rotateKey))
                _rotation = NextRotation(_rotation);

            if (Input.GetKeyDown(_confirmKey))
                TryCommitPlacement();
        }

        private void RefreshPreview()
        {
            _lastDef = null;
            _lastResult = default;

            if (!_placementMode || _runtimeHost == null || _runtimeHost.Mapper == null || _gameplayBootstrap == null || _selection == null)
            {
                HideAll();
                return;
            }

            if (!_selection.HasHoveredCell)
            {
                HideAll();
                return;
            }

            if (_gameplayBootstrap.Data == null || !_gameplayBootstrap.Data.TryGetBuilding(_activeBuildingDefId, out var def) || def == null)
            {
                HideAll();
                return;
            }

            _lastDef = def;
            _lastResult = _gameplayBootstrap.Placement.ValidateBuilding(_activeBuildingDefId, _selection.HoveredCell, _rotation);
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
                    PlaceCellMarker(marker.transform, cell, inside ? (_lastResult.Ok ? _validColor : _invalidColor) : _invalidColor);
                }
            }

            for (int i = markerIndex; i < _footprintMarkers.Count; i++)
                _footprintMarkers[i].SetActive(false);

            EnsureEntryMarker();
            bool showEntry = _runtimeHost.Mapper.IsInside(_lastResult.SuggestedRoadCell);
            _entryMarker.SetActive(showEntry);
            if (showEntry)
                PlaceCellMarker(_entryMarker.transform, _lastResult.SuggestedRoadCell, _entryRoadColor);
        }

        private void TryCommitPlacement()
        {
            if (_gameplayBootstrap?.Placement == null || _selection == null || !_selection.HasHoveredCell)
                return;
            if (_lastDef == null || !_lastResult.Ok)
                return;

            BuildingId id = _gameplayBootstrap.Placement.CommitBuilding(_activeBuildingDefId, _selection.HoveredCell, _rotation);
            if (id.Value == 0)
                return;

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
                _footprintMarkers.Add(CreateMarker($"PlacementMarker_{_footprintMarkers.Count}"));
        }

        private void EnsureEntryMarker()
        {
            if (_entryMarker != null)
                return;

            _entryMarker = CreateMarker("PlacementEntryMarker");
        }

        private GameObject CreateMarker(string name)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            Collider col = go.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private void PlaceCellMarker(Transform marker, CellPos cell, Color color)
        {
            Vector3 pos = _runtimeHost.Mapper.CellToWorldCenter(cell);
            float cellSize = _runtimeHost.Mapper.CellSize * _cellFill;
            marker.position = pos + Vector3.up * _heightOffset;
            marker.localScale = new Vector3(cellSize, 0.03f, cellSize);

            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = color;
        }

        private void HideAll()
        {
            for (int i = 0; i < _footprintMarkers.Count; i++)
                _footprintMarkers[i].SetActive(false);
            if (_entryMarker != null)
                _entryMarker.SetActive(false);
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
