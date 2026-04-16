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
        [SerializeField] private LayerMask _groundMask = 0;
        [SerializeField] private string _groundLayerName = "Ground";
        [SerializeField] private float _rayDistance = 5000f;
        [SerializeField] private KeyCode _selectKey = KeyCode.Mouse0;

        private GroundRaycastService _groundRaycast;
        private WorldToCellResolver3D _resolver;
        private int _resolvedGroundMaskValue;

        public bool HasHoveredCell { get; private set; }
        public CellPos HoveredCell { get; private set; }
        public bool HasSelectedCell { get; private set; }
        public CellPos SelectedCell { get; private set; }
        public BuildingId SelectedBuilding { get; private set; }
        public SiteId SelectedSite { get; private set; }
        public bool HasSelectedWorldObject => SelectedBuilding.Value != 0 || SelectedSite.Value != 0;
        public bool HasLastHoverDebugInfo { get; private set; }
        public WorldToCellResolver3D.ResolutionDebugInfo LastHoverDebugInfo { get; private set; }

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

            if (_runtimeHost != null && _runtimeHost.Mapper != null)
            {
                LayerMask resolvedGroundMask = ResolveGroundMask();
                if (_groundRaycast == null || _resolver == null || _resolvedGroundMaskValue != resolvedGroundMask.value)
                {
                    _resolvedGroundMaskValue = resolvedGroundMask.value;
                    _groundRaycast = new GroundRaycastService(resolvedGroundMask, _rayDistance);
                    _resolver = new WorldToCellResolver3D(_groundRaycast, _runtimeHost.Mapper);
                }
            }
        }

        private LayerMask ResolveGroundMask()
        {
            if (_groundMask.value != 0)
                return _groundMask;

            int namedLayer = LayerMask.NameToLayer(_groundLayerName);
            if (namedLayer >= 0)
            {
                _groundMask = 1 << namedLayer;
                return _groundMask;
            }

            _groundMask = ~0;
            return _groundMask;
        }

        private void UpdateHover()
        {
            bool hoveringSelectable = IsPointerOverSelectableWorldObject();
            if (hoveringSelectable)
            {
                HasHoveredCell = false;
                HoveredCell = default;
                return;
            }

            HasHoveredCell = TryRaycastCell(out var cell);
            HoveredCell = cell;

            if (!HasHoveredCell)
            {
                HasLastHoverDebugInfo = false;
                LastHoverDebugInfo = default;
            }
        }

        private void UpdateSelection()
        {
            if (!WasPressedThisFrame(_selectKey))
                return;

            if (TrySelectWorldEntity())
                return;

            if (!HasHoveredCell)
            {
                ClearSelection();
                return;
            }

            HasSelectedCell = true;
            SelectedCell = HoveredCell;
            SelectedBuilding = default;
            SelectedSite = default;

            if (_runtimeHost?.GridMap != null)
            {
                CellOccupancy occ = _runtimeHost.GridMap.Get(SelectedCell);
                if (occ.Kind == CellOccupancyKind.Building)
                    SelectedBuilding = occ.Building;
                else if (occ.Kind == CellOccupancyKind.Site)
                    SelectedSite = occ.Site;
            }
        }

        private bool TrySelectWorldEntity()
        {
            if (_camera == null)
                return false;

            Vector2 pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Ray ray = _camera.ScreenPointToRay(pointer);
            if (!Physics.Raycast(ray, out var hit, _rayDistance, ~0, QueryTriggerInteraction.Ignore))
                return false;

            SelectedEntityBridge3D bridge = hit.collider != null
                ? hit.collider.GetComponentInParent<SelectedEntityBridge3D>()
                : null;
            if (bridge == null || !bridge.IsSelectable)
                return false;

            ClearSelection();

            if (bridge.IsBuildSite && bridge.SiteId.Value != 0)
            {
                SelectedSite = bridge.SiteId;
                if (_gameplayBootstrap?.World != null && _gameplayBootstrap.World.Sites.Exists(bridge.SiteId))
                {
                    BuildSiteState state = _gameplayBootstrap.World.Sites.Get(bridge.SiteId);
                    SelectedCell = state.Anchor;
                    HasSelectedCell = true;
                }
                return true;
            }

            if (bridge.BuildingId.Value != 0)
            {
                SelectedBuilding = bridge.BuildingId;
                if (_gameplayBootstrap?.World != null && _gameplayBootstrap.World.Buildings.Exists(bridge.BuildingId))
                {
                    BuildingState state = _gameplayBootstrap.World.Buildings.Get(bridge.BuildingId);
                    SelectedCell = state.Anchor;
                    HasSelectedCell = true;
                }
                return true;
            }

            return false;
        }

        private void ClearSelection()
        {
            HasSelectedCell = false;
            SelectedCell = default;
            SelectedBuilding = default;
            SelectedSite = default;
        }

        public bool TryRaycastCell(out CellPos cell)
        {
            cell = default;
            if (_camera == null || _resolver == null)
            {
                HasLastHoverDebugInfo = false;
                LastHoverDebugInfo = default;
                return false;
            }

            Vector2 pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            bool ok = _resolver.TryResolveFromScreen(_camera, pointer, out cell, out _, out var debugInfo);
            HasLastHoverDebugInfo = ok;
            LastHoverDebugInfo = ok ? debugInfo : default;
            return ok;
        }

        private bool IsPointerOverSelectableWorldObject()
        {
            if (_camera == null)
                return false;

            Vector2 pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Ray ray = _camera.ScreenPointToRay(pointer);
            if (!Physics.Raycast(ray, out var hit, _rayDistance, ~0, QueryTriggerInteraction.Ignore))
                return false;

            SelectedEntityBridge3D bridge = hit.collider != null
                ? hit.collider.GetComponentInParent<SelectedEntityBridge3D>()
                : null;
            return bridge != null && bridge.IsSelectable;
        }

        public bool IsSelectionClickPressedThisFrame()
        {
            return WasPressedThisFrame(_selectKey);
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
