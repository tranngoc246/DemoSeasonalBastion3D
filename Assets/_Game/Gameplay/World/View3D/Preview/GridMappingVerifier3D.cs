using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class GridMappingVerifier3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private bool _drawHoveredCellBounds = true;
        [SerializeField] private bool _drawHoveredCellCenter = true;
        [SerializeField] private Color _boundsColor = new(0.2f, 1f, 0.35f, 1f);
        [SerializeField] private Color _centerColor = new(1f, 0.3f, 0.2f, 1f);
        [SerializeField] private float _debugHeightOffset = 0.15f;
        [SerializeField] private float _centerMarkerScale = 0.2f;

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
        }

        private void OnDrawGizmos()
        {
            if (!_drawHoveredCellBounds && !_drawHoveredCellCenter)
                return;

            ResolveRefs();
            if (_runtimeHost?.Mapper == null || _selection == null || !_selection.HasHoveredCell)
                return;

            CellPos hovered = _selection.HoveredCell;
            Bounds bounds = _runtimeHost.Mapper.GetFootprintWorldBounds(hovered, 1, 1);
            bounds.center += Vector3.up * _debugHeightOffset;

            if (_drawHoveredCellBounds)
            {
                Gizmos.color = _boundsColor;
                Gizmos.DrawWireCube(bounds.center, new Vector3(bounds.size.x, 0.02f, bounds.size.z));
            }

            if (_drawHoveredCellCenter)
            {
                Gizmos.color = _centerColor;
                Vector3 center = _runtimeHost.Mapper.CellToWorldCenter(hovered) + Vector3.up * _debugHeightOffset;
                Gizmos.DrawSphere(center, _centerMarkerScale);
            }
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_selection == null)
                _selection = FindObjectOfType<WorldSelectionController3D>();
        }
    }
}
