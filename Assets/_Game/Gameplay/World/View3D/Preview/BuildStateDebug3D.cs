using System.Collections.Generic;
using System.Text;
using SeasonalBastion.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace SeasonalBastion
{
    public sealed class BuildStateDebug3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _bootstrap;
        [SerializeField] private bool _showOverlay = true;
        [SerializeField] private bool _showWorldLabels = true;
        [SerializeField] private KeyCode _toggleOverlayKey = KeyCode.J;
        [SerializeField] private KeyCode _toggleLabelsKey = KeyCode.K;
        [SerializeField] private Vector2 _panelOffset = new(16f, 260f);
        [SerializeField] private Vector2 _panelSize = new(520f, 260f);
        [SerializeField] private int _fontSize = 16;
        [SerializeField] private Vector3 _labelOffset = new(0f, 1.25f, 0f);

        private readonly StringBuilder _sb = new();
        private readonly Dictionary<int, string> _buildingLabels = new();
        private readonly Dictionary<int, string> _siteLabels = new();
        private Canvas _canvas;
        private Text _label;

        private void Awake()
        {
            ResolveRefs();
            EnsureUi();
        }

        private void LateUpdate()
        {
            ResolveRefs();
            EnsureUi();
            HandleInput();
            RefreshOverlay();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !_showWorldLabels || _runtimeHost?.Mapper == null || _bootstrap?.World == null)
                return;

            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.85f);
            foreach (var id in _bootstrap.World.Buildings.Ids)
            {
                if (!_bootstrap.World.Buildings.Exists(id))
                    continue;

                BuildingState st = _bootstrap.World.Buildings.Get(id);
                Vector3 pos = _runtimeHost.Mapper.CellToWorldCenter(st.Anchor) + _labelOffset;
                Gizmos.DrawSphere(pos, 0.12f);
            }

            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.85f);
            foreach (var id in _bootstrap.World.Sites.Ids)
            {
                if (!_bootstrap.World.Sites.Exists(id))
                    continue;

                BuildSiteState st = _bootstrap.World.Sites.Get(id);
                Vector3 pos = _runtimeHost.Mapper.CellToWorldCenter(st.Anchor) + _labelOffset * 0.8f;
                Gizmos.DrawCube(pos, Vector3.one * 0.2f);
            }
        }

        private void ResolveRefs()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameplayRuntimeBootstrap>();
        }

        private void EnsureUi()
        {
            if (_canvas != null && _label != null)
                return;

            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasGo = new("BuildStateDebugCanvas");
                canvasGo.transform.SetParent(transform, false);
                _canvas = canvasGo.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (_label == null)
            {
                Transform existing = _canvas.transform.Find("BuildStateDebugLabel");
                if (existing != null)
                    _label = existing.GetComponent<Text>();
            }

            if (_label == null)
            {
                GameObject labelGo = new("BuildStateDebugLabel");
                labelGo.transform.SetParent(_canvas.transform, false);
                _label = labelGo.AddComponent<Text>();
                _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _label.fontSize = _fontSize;
                _label.alignment = TextAnchor.UpperLeft;
                _label.horizontalOverflow = HorizontalWrapMode.Wrap;
                _label.verticalOverflow = VerticalWrapMode.Overflow;

                RectTransform rect = _label.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = _panelOffset;
                rect.sizeDelta = _panelSize;
            }
        }

        private void HandleInput()
        {
            if (WasPressedThisFrame(_toggleOverlayKey))
                _showOverlay = !_showOverlay;
            if (WasPressedThisFrame(_toggleLabelsKey))
                _showWorldLabels = !_showWorldLabels;
        }

        private void RefreshOverlay()
        {
            if (_label == null)
                return;

            _label.enabled = _showOverlay;
            if (!_showOverlay)
                return;

            _sb.Clear();
            _sb.AppendLine("Build State Debug");

            if (_bootstrap?.World == null)
            {
                _sb.AppendLine("Runtime unavailable");
                _label.text = _sb.ToString();
                return;
            }

            _sb.Append("Buildings: ").AppendLine(_bootstrap.World.Buildings.Ids.Count.ToString());
            foreach (var id in _bootstrap.World.Buildings.Ids)
            {
                if (!_bootstrap.World.Buildings.Exists(id))
                    continue;

                BuildingState st = _bootstrap.World.Buildings.Get(id);
                _sb.Append("B#").Append(id.Value)
                    .Append(" ").Append(st.DefId)
                    .Append(" cell(").Append(st.Anchor.X).Append(',').Append(st.Anchor.Y).Append(')')
                    .Append(" lvl ").Append(st.Level)
                    .Append(" hp ").Append(st.HP).Append('/').Append(st.MaxHP)
                    .Append(" built=").Append(st.IsConstructed ? 'Y' : 'N')
                    .AppendLine();
            }

            _sb.Append("Sites: ").AppendLine(_bootstrap.World.Sites.Ids.Count.ToString());
            foreach (var id in _bootstrap.World.Sites.Ids)
            {
                if (!_bootstrap.World.Sites.Exists(id))
                    continue;

                BuildSiteState st = _bootstrap.World.Sites.Get(id);
                _sb.Append("S#").Append(id.Value)
                    .Append(" ").Append(st.BuildingDefId)
                    .Append(" cell(").Append(st.Anchor.X).Append(',').Append(st.Anchor.Y).Append(')')
                    .Append(" lvl->").Append(st.TargetLevel)
                    .Append(" work ").Append(st.WorkSecondsDone.ToString("0.0")).Append('/').Append(st.WorkSecondsTotal.ToString("0.0"))
                    .Append(" kind=").Append(st.IsUpgrade ? "upgrade" : "place")
                    .AppendLine();
            }

            _sb.AppendLine("[J] overlay  [K] world labels");
            _label.text = _sb.ToString();
        }

        private static bool WasPressedThisFrame(KeyCode key)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null)
                return false;

            return key switch
            {
                KeyCode.J => keyboard.jKey.wasPressedThisFrame,
                KeyCode.K => keyboard.kKey.wasPressedThisFrame,
                _ => false,
            };
        }
    }
}
