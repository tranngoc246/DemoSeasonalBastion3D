using UnityEngine;

namespace SeasonalBastion
{
    public sealed class SelectionHighlight3D : MonoBehaviour
    {
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private Color _selectedTint = new(1f, 0.92f, 0.2f, 1f);
        [SerializeField] private Color _siteTint = new(1f, 0.6f, 0.1f, 1f);

        private SelectedEntityBridge3D _bridge;
        private Renderer[] _renderers;
        private Color[] _baseColors;

        private void Awake()
        {
            ResolveRefs();
            CacheRenderers();
        }

        private void LateUpdate()
        {
            ResolveRefs();
            CacheRenderers();
            Refresh();
        }

        private void ResolveRefs()
        {
            if (_selection == null)
                _selection = FindFirstObjectByType<WorldSelectionController3D>();
            if (_bridge == null)
                _bridge = GetComponent<SelectedEntityBridge3D>();
        }

        private void CacheRenderers()
        {
            if (_renderers != null)
                return;

            _renderers = GetComponentsInChildren<Renderer>();
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                _baseColors[i] = renderer != null && renderer.sharedMaterial != null
                    ? renderer.sharedMaterial.color
                    : Color.white;
            }
        }

        private void Refresh()
        {
            if (_bridge == null || _renderers == null)
                return;

            bool selected = (!_bridge.IsBuildSite && _selection != null && _selection.SelectedBuilding.Value != 0 && _selection.SelectedBuilding.Value == _bridge.BuildingId.Value)
                || (_bridge.IsBuildSite && _selection != null && _selection.SelectedSite.Value != 0 && _selection.SelectedSite.Value == _bridge.SiteId.Value);

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null || renderer.sharedMaterial == null)
                    continue;

                renderer.sharedMaterial.color = selected
                    ? (_bridge.IsBuildSite ? _siteTint : _selectedTint)
                    : _baseColors[i];
            }
        }
    }
}
