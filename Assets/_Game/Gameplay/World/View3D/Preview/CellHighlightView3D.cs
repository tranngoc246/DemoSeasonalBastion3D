using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class CellHighlightView3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private Color _hoverColor = new(0.25f, 0.9f, 1f, 0.35f);
        [SerializeField] private Color _selectedColor = new(1f, 0.92f, 0.2f, 0.5f);
        [SerializeField] private float _heightOffset = 0.1f;
        [SerializeField] private float _sizeFactor = 0.9f;
        [SerializeField] private bool _hideSelectedCellForWorldObject = true;

        private GameObject _hoverMarker;
        private GameObject _selectedMarker;
        private Renderer _hoverRenderer;
        private Renderer _selectedRenderer;

        private void Awake()
        {
            ResolveRefs();
            EnsureMarkers();
        }

        private void LateUpdate()
        {
            ResolveRefs();
            EnsureMarkers();
            SyncHover();
            SyncSelected();
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_selection == null)
                _selection = FindObjectOfType<WorldSelectionController3D>();
        }

        private void EnsureMarkers()
        {
            if (_hoverMarker == null)
            {
                _hoverMarker = CreateMarker("HoverCellMarker", _hoverColor, out _hoverRenderer);
                _hoverMarker.transform.SetParent(transform, false);
            }

            if (_selectedMarker == null)
            {
                _selectedMarker = CreateMarker("SelectedCellMarker", _selectedColor, out _selectedRenderer);
                _selectedMarker.transform.SetParent(transform, false);
            }
        }

        private void SyncHover()
        {
            if (_hoverMarker == null)
                return;

            bool visible = _selection != null && _selection.HasHoveredCell && _runtimeHost != null && _runtimeHost.Mapper != null;
            _hoverMarker.SetActive(visible);
            if (!visible)
                return;

            PlaceMarker(_hoverMarker.transform, _selection.HoveredCell);
        }

        private void SyncSelected()
        {
            if (_selectedMarker == null)
                return;

            bool visible = _selection != null && _selection.HasSelectedCell && _runtimeHost != null && _runtimeHost.Mapper != null;
            if (visible && _hideSelectedCellForWorldObject && _selection.HasSelectedWorldObject)
                visible = false;
            _selectedMarker.SetActive(visible);
            if (!visible)
                return;

            PlaceMarker(_selectedMarker.transform, _selection.SelectedCell);
        }

        private void PlaceMarker(Transform marker, CellPos cell)
        {
            Vector3 pos = _runtimeHost.Mapper.CellToWorldCenter(cell);
            float cellSize = _runtimeHost.Mapper.CellSize * _sizeFactor;
            marker.position = pos + Vector3.up * _heightOffset;
            marker.localScale = new Vector3(cellSize, 0.02f, cellSize);
        }

        private static GameObject CreateMarker(string name, Color color, out Renderer rendererRef)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Collider col = go.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            Renderer renderer = go.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
                renderer.sharedMaterial = new Material(shader);
            renderer.sharedMaterial.color = color;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            rendererRef = renderer;
            return go;
        }
    }
}
