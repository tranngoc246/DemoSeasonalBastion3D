using SeasonalBastion.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SeasonalBastion
{
    public sealed class CameraFocusController3D : MonoBehaviour
    {
        [SerializeField] private StrategyCameraController3D _strategyCamera;
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _bootstrap;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private KeyCode _focusSelectionKey = KeyCode.F;
        [SerializeField] private bool _focusOnSelectionChange;

        private BuildingId _lastBuilding;
        private SiteId _lastSite;

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
            HandleInput();
            HandleSelectionChangeFocus();
        }

        public bool FocusSelection()
        {
            if (_selection == null)
                return false;

            if (_selection.SelectedBuilding.Value != 0)
                return FocusBuilding(_selection.SelectedBuilding);

            if (_selection.SelectedSite.Value != 0)
                return FocusSite(_selection.SelectedSite);

            if (_selection.HasSelectedCell)
                return FocusCell(_selection.SelectedCell);

            return false;
        }

        public bool FocusCell(CellPos cell)
        {
            if (_runtimeHost?.Mapper == null || _strategyCamera == null)
                return false;

            Vector3 world = _runtimeHost.Mapper.CellToWorldCenter(cell);
            return _strategyCamera.TrySetFocusPoint(world);
        }

        public bool FocusBuilding(BuildingId buildingId)
        {
            if (_bootstrap?.World == null || !_bootstrap.World.Buildings.Exists(buildingId))
                return false;

            BuildingState state = _bootstrap.World.Buildings.Get(buildingId);
            return FocusCell(state.Anchor);
        }

        public bool FocusSite(SiteId siteId)
        {
            if (_bootstrap?.World == null || !_bootstrap.World.Sites.Exists(siteId))
                return false;

            BuildSiteState state = _bootstrap.World.Sites.Get(siteId);
            return FocusCell(state.Anchor);
        }

        private void ResolveRefs()
        {
            if (_strategyCamera == null)
                _strategyCamera = FindFirstObjectByType<StrategyCameraController3D>();
            if (_runtimeHost == null)
                _runtimeHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameplayRuntimeBootstrap>();
            if (_selection == null)
                _selection = FindFirstObjectByType<WorldSelectionController3D>();
        }

        private void HandleInput()
        {
            if (!WasPressedThisFrame(_focusSelectionKey))
                return;

            FocusSelection();
        }

        private void HandleSelectionChangeFocus()
        {
            if (!_focusOnSelectionChange || _selection == null)
                return;

            bool changed = _lastBuilding.Value != _selection.SelectedBuilding.Value
                || _lastSite.Value != _selection.SelectedSite.Value;

            _lastBuilding = _selection.SelectedBuilding;
            _lastSite = _selection.SelectedSite;

            if (changed)
                FocusSelection();
        }

        private static bool WasPressedThisFrame(KeyCode key)
        {
            if (Keyboard.current == null)
                return false;

            return key switch
            {
                KeyCode.F => Keyboard.current.fKey.wasPressedThisFrame,
                _ => false,
            };
        }
    }
}
