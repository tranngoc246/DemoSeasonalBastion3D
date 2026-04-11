using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class HoverCellDebug3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private bool _showOverlay = true;
        [SerializeField] private bool _logOnCellChange;
        [SerializeField] private Vector2 _overlayOffset = new(16f, 96f);

        private bool _hadLastCell;
        private CellPos _lastCell;
        private string _overlayText = "Hover: none";

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
            RefreshDebugState();
        }

        private void OnGUI()
        {
            if (!_showOverlay)
                return;

            GUI.Label(new Rect(_overlayOffset.x, _overlayOffset.y, 420f, 80f), _overlayText);
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_selection == null)
                _selection = FindObjectOfType<WorldSelectionController3D>();
        }

        private void RefreshDebugState()
        {
            if (_selection == null || !_selection.HasHoveredCell)
            {
                _overlayText = "Hover: none";
                _hadLastCell = false;
                return;
            }

            CellPos hovered = _selection.HoveredCell;
            Vector3 world = _runtimeHost != null && _runtimeHost.Mapper != null
                ? _runtimeHost.Mapper.CellToWorldCenter(hovered)
                : Vector3.zero;

            _overlayText = $"Hover: cell=({hovered.X},{hovered.Y}) world=({world.x:F2}, {world.y:F2}, {world.z:F2})";

            bool changed = !_hadLastCell || hovered.X != _lastCell.X || hovered.Y != _lastCell.Y;
            if (changed)
            {
                _lastCell = hovered;
                _hadLastCell = true;

                if (_logOnCellChange)
                    Debug.Log($"[HoverCellDebug3D] Hovered cell changed to ({hovered.X}, {hovered.Y})", this);
            }
        }
    }
}
