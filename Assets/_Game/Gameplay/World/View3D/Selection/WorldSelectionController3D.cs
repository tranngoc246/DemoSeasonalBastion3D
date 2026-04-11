using SeasonalBastion.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

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
            if (!WasPressedThisFrame(_selectKey))
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

            Vector2 pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Ray ray = _camera.ScreenPointToRay(pointer);
            if (!Physics.Raycast(ray, out var hit, 5000f, _rayMask, QueryTriggerInteraction.Ignore))
                return false;

            cell = _runtimeHost.Mapper.WorldToCell(hit.point);
            if (!_runtimeHost.Mapper.IsInside(cell))
                return false;

            return true;
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
                KeyCode.Space => Keyboard.current.spaceKey.wasPressedThisFrame,
                KeyCode.Return => Keyboard.current.enterKey.wasPressedThisFrame,
                _ => false,
            };
        }
    }
}
