using UnityEngine;
using UnityEngine.UI;

namespace SeasonalBastion
{
    public sealed class PlacementHudView3D : MonoBehaviour
    {
        [SerializeField] private PlacementPreviewController3D _preview;
        [SerializeField] private Color _okColor = Color.white;
        [SerializeField] private Color _failColor = new(1f, 0.8f, 0.8f, 1f);
        [SerializeField] private Vector2 _panelSize = new(620f, 80f);
        [SerializeField] private Vector2 _panelOffset = new(16f, -16f);
        [SerializeField] private int _fontSize = 18;

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
            Refresh();
        }

        private void ResolveRefs()
        {
            if (_preview == null)
                _preview = FindFirstObjectByType<PlacementPreviewController3D>();
        }

        private void EnsureUi()
        {
            if (_canvas != null && _label != null)
                return;

            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasGo = new("PlacementHUDCanvas");
                canvasGo.transform.SetParent(transform, false);
                _canvas = canvasGo.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (_label == null)
            {
                Transform existing = _canvas.transform.Find("PlacementLabel");
                if (existing != null)
                    _label = existing.GetComponent<Text>();
            }

            if (_label == null)
            {
                GameObject labelGo = new("PlacementLabel");
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

        private void Refresh()
        {
            if (_label == null)
                return;

            string text = _preview != null ? _preview.DebugPlacementText : string.Empty;
            _label.text = text;
            _label.enabled = !string.IsNullOrEmpty(text);
            _label.color = _preview != null && _preview.LastPlacementOk ? _okColor : _failColor;
        }
    }
}
