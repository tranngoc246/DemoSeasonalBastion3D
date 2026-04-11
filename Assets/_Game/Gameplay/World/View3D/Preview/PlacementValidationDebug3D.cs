using System.Text;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class PlacementValidationDebug3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _bootstrap;
        [SerializeField] private PlacementPreviewController3D _preview;
        [SerializeField] private bool _showOverlay = true;
        [SerializeField] private Vector2 _overlayOffset = new(16f, 132f);

        private readonly StringBuilder _text = new();
        private string _overlayText = string.Empty;

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
            RefreshText();
        }

        private void OnGUI()
        {
            if (!_showOverlay || string.IsNullOrEmpty(_overlayText))
                return;

            GUI.Label(new Rect(_overlayOffset.x, _overlayOffset.y, 700f, 120f), _overlayText);
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_bootstrap == null)
                _bootstrap = FindObjectOfType<GameplayRuntimeBootstrap>();
            if (_preview == null)
                _preview = FindObjectOfType<PlacementPreviewController3D>();
        }

        private void RefreshText()
        {
            _overlayText = string.Empty;
            if (_runtimeHost?.Mapper == null || _runtimeHost.GridMap == null || _bootstrap == null || _preview == null)
                return;

            string previewText = _preview.DebugPlacementText;
            if (string.IsNullOrEmpty(previewText))
                return;

            CellPos? entry = TryExtractCell(previewText, "entry=(");
            CellPos? anchor = TryExtractCell(previewText, "anchor=(");

            _text.Clear();
            _text.Append(previewText);

            if (anchor.HasValue)
            {
                Vector3 anchorWorld = _runtimeHost.Mapper.CellToWorldCenter(anchor.Value);
                _text.Append("\nFootprint anchor world=");
                _text.Append('(').Append(anchorWorld.x.ToString("F2")).Append(", ").Append(anchorWorld.y.ToString("F2")).Append(", ").Append(anchorWorld.z.ToString("F2")).Append(')');
            }

            if (entry.HasValue)
            {
                CellPos e = entry.Value;
                bool inside = _runtimeHost.GridMap.IsInside(e);
                bool isRoad = inside && _runtimeHost.GridMap.IsRoad(e);
                bool roadN = HasRoad(e, 0, 1);
                bool roadE = HasRoad(e, 1, 0);
                bool roadS = HasRoad(e, 0, -1);
                bool roadW = HasRoad(e, -1, 0);

                _text.Append("\nDriveway cell=");
                _text.Append('(').Append(e.X).Append(',').Append(e.Y).Append(')');
                _text.Append(" inside=").Append(inside ? "yes" : "no");
                _text.Append(" selfRoad=").Append(isRoad ? "yes" : "no");
                _text.Append(" crossRoads[N,E,S,W]=[")
                    .Append(roadN ? 'Y' : 'n').Append(',')
                    .Append(roadE ? 'Y' : 'n').Append(',')
                    .Append(roadS ? 'Y' : 'n').Append(',')
                    .Append(roadW ? 'Y' : 'n').Append(']');
            }

            _overlayText = _text.ToString();
        }

        private bool HasRoad(CellPos origin, int dx, int dy)
        {
            CellPos c = new(origin.X + dx, origin.Y + dy);
            return _runtimeHost.GridMap.IsInside(c) && _runtimeHost.GridMap.IsRoad(c);
        }

        private static CellPos? TryExtractCell(string text, string prefix)
        {
            int start = text.IndexOf(prefix);
            if (start < 0)
                return null;

            start += prefix.Length;
            int end = text.IndexOf(')', start);
            if (end < 0)
                return null;

            string[] parts = text.Substring(start, end - start).Split(',');
            if (parts.Length != 2)
                return null;
            if (!int.TryParse(parts[0], out int x))
                return null;
            if (!int.TryParse(parts[1], out int y))
                return null;

            return new CellPos(x, y);
        }
    }
}
