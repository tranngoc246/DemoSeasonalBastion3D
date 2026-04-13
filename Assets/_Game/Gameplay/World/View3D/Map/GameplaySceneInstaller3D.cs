using UnityEngine;

namespace SeasonalBastion
{
    public sealed class GameplaySceneInstaller3D : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private TerrainGameplayRuntimeHost _terrainHost;
        [SerializeField] private GameplayRuntimeBootstrap _bootstrap;
        [SerializeField] private WorldViewRoot3D _worldView;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private CellHighlightView3D _highlight;
        [SerializeField] private HoverCellDebug3D _hoverDebug;
        [SerializeField] private PlacementPreviewController3D _preview;
        [SerializeField] private GridOverlay3D _gridOverlay;
        [SerializeField] private PlacementHudView3D _hud;
        [SerializeField] private SelectionInspectHudView3D _inspectHud;
        [SerializeField] private SelectionActionDebug3D _selectionActions;
        [SerializeField] private StrategyCameraController3D _strategyCamera;
        [SerializeField] private CameraFocusController3D _cameraFocus;

        private void Awake()
        {
            Install();
        }

        [ContextMenu("Install Scene References")]
        public void Install()
        {
            if (_camera == null)
                _camera = Camera.main;
            if (_terrainHost == null)
                _terrainHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameplayRuntimeBootstrap>();
            if (_worldView == null)
                _worldView = FindFirstObjectByType<WorldViewRoot3D>();
            if (_selection == null)
                _selection = FindFirstObjectByType<WorldSelectionController3D>();
            if (_highlight == null)
                _highlight = FindFirstObjectByType<CellHighlightView3D>();
            if (_hoverDebug == null)
                _hoverDebug = FindFirstObjectByType<HoverCellDebug3D>();
            if (_preview == null)
                _preview = FindFirstObjectByType<PlacementPreviewController3D>();
            if (_gridOverlay == null)
                _gridOverlay = FindFirstObjectByType<GridOverlay3D>();
            if (_hud == null)
                _hud = FindFirstObjectByType<PlacementHudView3D>();
            if (_inspectHud == null)
                _inspectHud = FindFirstObjectByType<SelectionInspectHudView3D>();
            if (_selectionActions == null)
                _selectionActions = FindFirstObjectByType<SelectionActionDebug3D>();
            if (_strategyCamera == null)
                _strategyCamera = FindFirstObjectByType<StrategyCameraController3D>();
            if (_cameraFocus == null)
                _cameraFocus = FindFirstObjectByType<CameraFocusController3D>();

            if (_strategyCamera != null)
            {
                SetObjectField(_strategyCamera, "_camera", _camera);
                SetObjectField(_strategyCamera, "_runtimeHost", _terrainHost);
            }

            if (_bootstrap != null)
            {
                SetObjectField(_bootstrap, "_terrainHost", _terrainHost);
                SetObjectField(_bootstrap, "_worldView", _worldView);
            }

            if (_worldView != null)
            {
                SetObjectField(_worldView, "_runtimeHost", _terrainHost);
                SetObjectField(_worldView, "_gameplayBootstrap", _bootstrap);
            }

            if (_selection != null)
            {
                SetObjectField(_selection, "_camera", _camera);
                SetObjectField(_selection, "_runtimeHost", _terrainHost);
                SetObjectField(_selection, "_gameplayBootstrap", _bootstrap);
            }

            if (_highlight != null)
            {
                SetObjectField(_highlight, "_runtimeHost", _terrainHost);
                SetObjectField(_highlight, "_selection", _selection);
            }

            if (_hoverDebug != null)
            {
                SetObjectField(_hoverDebug, "_runtimeHost", _terrainHost);
                SetObjectField(_hoverDebug, "_selection", _selection);
            }

            if (_preview != null)
            {
                SetObjectField(_preview, "_runtimeHost", _terrainHost);
                SetObjectField(_preview, "_gameplayBootstrap", _bootstrap);
                SetObjectField(_preview, "_selection", _selection);
                SetObjectField(_preview, "_worldView", _worldView);
            }

            if (_gridOverlay != null)
                SetObjectField(_gridOverlay, "_runtimeHost", _terrainHost);

            if (_hud != null)
                SetObjectField(_hud, "_preview", _preview);

            if (_inspectHud != null)
            {
                SetObjectField(_inspectHud, "_runtimeHost", _terrainHost);
                SetObjectField(_inspectHud, "_bootstrap", _bootstrap);
                SetObjectField(_inspectHud, "_selection", _selection);
            }

            if (_selectionActions != null)
            {
                SetObjectField(_selectionActions, "_bootstrap", _bootstrap);
                SetObjectField(_selectionActions, "_selection", _selection);
            }

            if (_cameraFocus != null)
            {
                SetObjectField(_cameraFocus, "_strategyCamera", _strategyCamera);
                SetObjectField(_cameraFocus, "_runtimeHost", _terrainHost);
                SetObjectField(_cameraFocus, "_bootstrap", _bootstrap);
                SetObjectField(_cameraFocus, "_selection", _selection);
            }
        }

        private static void SetObjectField(Object target, string fieldName, Object value)
        {
            if (target == null)
                return;

            var type = target.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null)
                return;

            field.SetValue(target, value);
        }
    }
}
