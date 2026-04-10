using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class WorldSelectionController3D : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _gameplayBootstrap;
        [SerializeField] private LayerMask _rayMask = ~0;
        [SerializeField] private KeyCode _selectKey = KeyCode.Mouse0;

        public bool HasHoveredCell { get; private set; }
        public CellPos HoveredCell { get; private set; }
        public bool HasSelectedCell { get; private set; }
        public CellPos SelectedCell { get; private set; }
        public BuildingId SelectedBuilding { get; private set; }

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
            UpdateHover();
            UpdateSelection();
        }

        private void ResolveRefs()
        {
            if (_camera == null)
                _camera = Camera.main;
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_gameplayBootstrap == null)
                _gameplayBootstrap = FindObjectOfType<GameplayRuntimeBootstrap>();
        }

        private void UpdateHover()
        {
            HasHoveredCell = TryRaycastCell(out var cell);
            HoveredCell = cell;
        }

        private void UpdateSelection()
        {
            if (!Input.GetKeyDown(_selectKey))
                return;

            if (!HasHoveredCell)
            {
                HasSelectedCell = false;
                SelectedCell = default;
                SelectedBuilding = default;
                return;
            }

            HasSelectedCell = true;
            SelectedCell = HoveredCell;
            SelectedBuilding = default;

            if (_runtimeHost?.GridMap != null)
            {
                CellOccupancy occ = _runtimeHost.GridMap.Get(SelectedCell);
                if (occ.Kind == CellOccupancyKind.Building)
                    SelectedBuilding = occ.Building;
            }
        }

        public bool TryRaycastCell(out CellPos cell)
        {
            cell = default;
            if (_camera == null || _runtimeHost == null || _runtimeHost.Mapper == null)
                return false;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 5000f, _rayMask, QueryTriggerInteraction.Ignore))
                return false;

            cell = _runtimeHost.Mapper.WorldToCell(hit.point);
            if (!_runtimeHost.Mapper.IsInside(cell))
                return false;

            return true;
        }
    }
}
