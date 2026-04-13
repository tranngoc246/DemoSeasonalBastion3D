using System.Text;
using SeasonalBastion.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace SeasonalBastion
{
    public sealed class SelectionInspectHudView3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _bootstrap;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private Vector2 _panelSize = new(420f, 220f);
        [SerializeField] private Vector2 _panelOffset = new(-16f, -16f);
        [SerializeField] private int _fontSize = 18;

        private readonly StringBuilder _sb = new();
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
            if (_runtimeHost == null)
                _runtimeHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameplayRuntimeBootstrap>();
            if (_selection == null)
                _selection = FindFirstObjectByType<WorldSelectionController3D>();
        }

        private void EnsureUi()
        {
            if (_canvas != null && _label != null)
                return;

            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasGo = new("InspectHUDCanvas");
                canvasGo.transform.SetParent(transform, false);
                _canvas = canvasGo.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (_label == null)
            {
                Transform existing = _canvas.transform.Find("InspectLabel");
                if (existing != null)
                    _label = existing.GetComponent<Text>();
            }

            if (_label == null)
            {
                GameObject labelGo = new("InspectLabel");
                labelGo.transform.SetParent(_canvas.transform, false);
                _label = labelGo.AddComponent<Text>();
                _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _label.fontSize = _fontSize;
                _label.alignment = TextAnchor.UpperRight;
                _label.horizontalOverflow = HorizontalWrapMode.Wrap;
                _label.verticalOverflow = VerticalWrapMode.Overflow;

                RectTransform rect = _label.rectTransform;
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = _panelOffset;
                rect.sizeDelta = _panelSize;
            }
        }

        private void Refresh()
        {
            if (_label == null)
                return;

            _sb.Clear();
            _sb.AppendLine("Inspect");

            if (_selection == null)
            {
                _sb.AppendLine("Selection unavailable");
                _label.text = _sb.ToString();
                return;
            }

            AppendCell("Hover", _selection.HasHoveredCell, _selection.HoveredCell);
            AppendCell("Selected", _selection.HasSelectedCell, _selection.SelectedCell);

            if (_selection.HasSelectedCell && _runtimeHost?.GridMap != null)
            {
                CellOccupancy occ = _runtimeHost.GridMap.Get(_selection.SelectedCell);
                _sb.Append("Occupancy: ").AppendLine(occ.Kind.ToString());

                if (_bootstrap?.World != null)
                {
                    if (_selection.SelectedBuilding.Value != 0)
                        AppendBuilding(_selection.SelectedBuilding);
                    else if (_selection.SelectedSite.Value != 0)
                        AppendSite(_selection.SelectedSite);
                    else if (occ.Kind == CellOccupancyKind.Building && occ.Building.Value != 0)
                        AppendBuilding(occ.Building);
                    else if (occ.Kind == CellOccupancyKind.Site && occ.Site.Value != 0)
                        AppendSite(occ.Site);
                }
            }
            else
            {
                _sb.AppendLine("Occupancy: none");
            }

            _label.text = _sb.ToString();
        }

        private void AppendCell(string label, bool hasCell, CellPos cell)
        {
            _sb.Append(label).Append(": ");
            if (!hasCell)
            {
                _sb.AppendLine("none");
                return;
            }

            _sb.Append('(').Append(cell.X).Append(',').Append(cell.Y).AppendLine(")");
        }

        private void AppendBuilding(BuildingId id)
        {
            BuildingState st = _bootstrap.World.Buildings.Get(id);
            _sb.AppendLine("Type: Building");
            _sb.Append("Id: ").AppendLine(id.Value.ToString());
            _sb.Append("Def: ").AppendLine(st.DefId);
            _sb.Append("Level: ").AppendLine(st.Level.ToString());
            _sb.Append("HP: ").Append(st.HP).Append('/').AppendLine(st.MaxHP.ToString());
            _sb.Append("Constructed: ").AppendLine(st.IsConstructed ? "yes" : "no");
            _sb.Append("Rotation: ").AppendLine(st.Rotation.ToString());
        }

        private void AppendSite(SiteId id)
        {
            BuildSiteState st = _bootstrap.World.Sites.Get(id);
            _sb.AppendLine("Type: Build Site");
            _sb.Append("Id: ").AppendLine(id.Value.ToString());
            _sb.Append("Def: ").AppendLine(st.BuildingDefId);
            _sb.Append("Target Level: ").AppendLine(st.TargetLevel.ToString());
            _sb.Append("Progress: ").Append(st.WorkSecondsDone.ToString("0.0")).Append('/').AppendLine(st.WorkSecondsTotal.ToString("0.0"));
            _sb.Append("Ready: ").AppendLine(st.IsReadyToWork ? "yes" : "no");
            _sb.Append("Rotation: ").AppendLine(st.Rotation.ToString());
        }
    }
}
