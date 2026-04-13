using System.Collections.Generic;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class GridOverlay3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private bool _showGrid = true;
        [SerializeField] private bool _showBlocked;
        [SerializeField] private bool _showBuildable;
        [SerializeField] private bool _showOccupancy;
        [SerializeField] private KeyCode _toggleGridKey = KeyCode.G;
        [SerializeField] private KeyCode _toggleBlockedKey = KeyCode.B;
        [SerializeField] private KeyCode _toggleBuildableKey = KeyCode.N;
        [SerializeField] private KeyCode _toggleOccupancyKey = KeyCode.M;
        [SerializeField] private float _lineHeight = 0.06f;
        [SerializeField] private float _cellFillHeight = 0.03f;
        [SerializeField] private float _gridLineThickness = 0.035f;
        [SerializeField] private float _fillSizeFactor = 0.72f;
        [SerializeField] private Color _gridColor = new(1f, 1f, 1f, 0.08f);
        [SerializeField] private Color _blockedColor = new(1f, 0.2f, 0.2f, 0.35f);
        [SerializeField] private Color _buildableColor = new(0.2f, 0.9f, 0.35f, 0.2f);
        [SerializeField] private Color _roadColor = new(1f, 0.65f, 0.1f, 0.35f);
        [SerializeField] private Color _buildingColor = new(1f, 0.9f, 0.1f, 0.35f);
        [SerializeField] private Color _siteColor = new(0.2f, 0.8f, 1f, 0.35f);

        private readonly List<GameObject> _instances = new();
        private int _lastHash;
        private bool _built;

        private void Awake()
        {
            ResolveRefs();
        }

        private void LateUpdate()
        {
            ResolveRefs();
            HandleInput();
            RefreshIfNeeded();
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(_toggleGridKey))
                _showGrid = !_showGrid;
            if (Input.GetKeyDown(_toggleBlockedKey))
                _showBlocked = !_showBlocked;
            if (Input.GetKeyDown(_toggleBuildableKey))
                _showBuildable = !_showBuildable;
            if (Input.GetKeyDown(_toggleOccupancyKey))
                _showOccupancy = !_showOccupancy;
        }

        private void RefreshIfNeeded()
        {
            if (_runtimeHost?.Mapper == null || _runtimeHost.GridMap == null)
                return;

            int hash = ComputeStateHash();
            if (_built && hash == _lastHash)
                return;

            Rebuild();
            _lastHash = hash;
            _built = true;
        }

        private int ComputeStateHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (_showGrid ? 1 : 0);
                hash = hash * 31 + (_showBlocked ? 1 : 0);
                hash = hash * 31 + (_showBuildable ? 1 : 0);
                hash = hash * 31 + (_showOccupancy ? 1 : 0);
                hash = hash * 31 + _runtimeHost.GridMap.Width;
                hash = hash * 31 + _runtimeHost.GridMap.Height;
                return hash;
            }
        }

        [ContextMenu("Rebuild Grid Overlay")]
        public void Rebuild()
        {
            ClearInstances();

            if (_runtimeHost?.Mapper == null || _runtimeHost.GridMap == null)
                return;

            if (_showGrid)
                BuildGridLines();

            for (int y = 0; y < _runtimeHost.GridMap.Height; y++)
            {
                for (int x = 0; x < _runtimeHost.GridMap.Width; x++)
                {
                    CellPos cell = new(x, y);
                    if (_showBuildable && ShouldShowBuildable(cell))
                        CreateCellFill($"Buildable_{x}_{y}", cell, _buildableColor, _cellFillHeight);

                    if (_showBlocked && _runtimeHost.GridMap.IsBlocked(cell))
                        CreateCellFill($"Blocked_{x}_{y}", cell, _blockedColor, _cellFillHeight + 0.01f);

                    if (_showOccupancy)
                        CreateOccupancyFill(cell);
                }
            }
        }

        private bool ShouldShowBuildable(CellPos cell)
        {
            return _runtimeHost.GeneratedWorld?.BuildableMap != null
                && cell.X >= 0 && cell.Y >= 0
                && cell.X < _runtimeHost.GeneratedWorld.Width
                && cell.Y < _runtimeHost.GeneratedWorld.Height
                && _runtimeHost.GeneratedWorld.BuildableMap[cell.X, cell.Y];
        }

        private void BuildGridLines()
        {
            float cellSize = _runtimeHost.Mapper.CellSize;
            int width = _runtimeHost.GridMap.Width;
            int height = _runtimeHost.GridMap.Height;

            for (int x = 0; x <= width; x++)
            {
                Vector3 start = _runtimeHost.Mapper.CellToWorldCorner(new CellPos(x, 0));
                Vector3 end = _runtimeHost.Mapper.CellToWorldCorner(new CellPos(x, height));
                CreateLine($"GridLineX_{x}", start, end, _gridLineThickness, _lineHeight, _gridColor);
            }

            for (int y = 0; y <= height; y++)
            {
                Vector3 start = _runtimeHost.Mapper.CellToWorldCorner(new CellPos(0, y));
                Vector3 end = _runtimeHost.Mapper.CellToWorldCorner(new CellPos(width, y));
                CreateLine($"GridLineY_{y}", start, end, _gridLineThickness, _lineHeight, _gridColor);
            }
        }

        private void CreateOccupancyFill(CellPos cell)
        {
            CellOccupancy occ = _runtimeHost.GridMap.Get(cell);
            switch (occ.Kind)
            {
                case CellOccupancyKind.Road:
                    CreateCellFill($"Road_{cell.X}_{cell.Y}", cell, _roadColor, _cellFillHeight + 0.02f);
                    break;
                case CellOccupancyKind.Building:
                    CreateCellFill($"Building_{cell.X}_{cell.Y}", cell, _buildingColor, _cellFillHeight + 0.02f);
                    break;
                case CellOccupancyKind.Site:
                    CreateCellFill($"Site_{cell.X}_{cell.Y}", cell, _siteColor, _cellFillHeight + 0.02f);
                    break;
            }
        }

        private void CreateLine(string name, Vector3 start, Vector3 end, float thickness, float heightOffset, Color color)
        {
            Vector3 delta = end - start;
            float length = delta.magnitude;
            if (length <= 0.0001f)
                return;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = (start + end) * 0.5f + Vector3.up * heightOffset;
            go.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            go.transform.localScale = new Vector3(thickness, 0.01f, length);
            ApplyVisual(go, color);
            _instances.Add(go);
        }

        private void CreateCellFill(string name, CellPos cell, Color color, float heightOffset)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = _runtimeHost.Mapper.CellToWorldCenter(cell) + Vector3.up * heightOffset;
            float size = _runtimeHost.Mapper.CellSize * _fillSizeFactor;
            go.transform.localScale = new Vector3(size, 0.01f, size);
            ApplyVisual(go, color);
            _instances.Add(go);
        }

        private static void ApplyVisual(GameObject go, Color color)
        {
            if (go.TryGetComponent<Collider>(out var collider))
                Destroy(collider);

            if (!go.TryGetComponent<Renderer>(out var renderer))
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
                renderer.sharedMaterial = new Material(shader);
            renderer.sharedMaterial.color = color;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void ClearInstances()
        {
            for (int i = 0; i < _instances.Count; i++)
            {
                GameObject go = _instances[i];
                if (go != null)
                    Destroy(go);
            }
            _instances.Clear();
        }
    }
}
