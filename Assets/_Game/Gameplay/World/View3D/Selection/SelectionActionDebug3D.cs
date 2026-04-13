using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class SelectionActionDebug3D : MonoBehaviour
    {
        [SerializeField] private GameplayRuntimeBootstrap _bootstrap;
        [SerializeField] private WorldSelectionController3D _selection;
        [SerializeField] private bool _showOverlay = true;
        [SerializeField] private KeyCode _upgradeKey = KeyCode.U;
        [SerializeField] private KeyCode _destroyKey = KeyCode.Delete;
        [SerializeField] private Vector2 _panelOffset = new(16f, 180f);

        private string _lastAction = string.Empty;

        private void Awake()
        {
            ResolveRefs();
        }

        private void Update()
        {
            ResolveRefs();
            HandleInput();
        }

        private void OnGUI()
        {
            if (!_showOverlay)
                return;

            string selected = _selection == null
                ? "none"
                : _selection.SelectedBuilding.Value != 0
                    ? $"building {_selection.SelectedBuilding.Value}"
                    : _selection.SelectedSite.Value != 0
                        ? $"site {_selection.SelectedSite.Value}"
                        : "none";

            GUI.Label(new Rect(_panelOffset.x, _panelOffset.y, 560f, 120f),
                $"Selection Actions\nSelected: {selected}\n[{_upgradeKey}] upgrade building | [{_destroyKey}] destroy selected\n{_lastAction}");
        }

        private void ResolveRefs()
        {
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameplayRuntimeBootstrap>();
            if (_selection == null)
                _selection = FindFirstObjectByType<WorldSelectionController3D>();
        }

        private void HandleInput()
        {
            if (_bootstrap == null || _selection == null)
                return;

            if (Input.GetKeyDown(_upgradeKey))
                TryUpgradeSelectedBuilding();

            if (Input.GetKeyDown(_destroyKey))
                TryDestroySelected();
        }

        private void TryUpgradeSelectedBuilding()
        {
            if (_selection.SelectedBuilding.Value == 0 || _bootstrap.BuildOrders == null)
            {
                _lastAction = "Upgrade skipped: no building selected.";
                return;
            }

            int orderId = _bootstrap.BuildOrders.CreateUpgradeOrder(_selection.SelectedBuilding);
            _lastAction = orderId > 0
                ? $"Upgrade order created: {orderId}"
                : "Upgrade skipped: order was not created.";
        }

        private void TryDestroySelected()
        {
            if (_selection.SelectedSite.Value != 0)
            {
                _bootstrap.WorldOps?.DestroyBuildSite(_selection.SelectedSite);
                _lastAction = $"Destroyed build site {_selection.SelectedSite.Value}";
                return;
            }

            if (_selection.SelectedBuilding.Value != 0)
            {
                _bootstrap.WorldOps?.DestroyBuilding(_selection.SelectedBuilding);
                _lastAction = $"Destroyed building {_selection.SelectedBuilding.Value}";
                return;
            }

            _lastAction = "Destroy skipped: nothing selected.";
        }
    }
}
