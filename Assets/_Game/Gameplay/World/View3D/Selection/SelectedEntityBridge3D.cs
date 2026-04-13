using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class SelectedEntityBridge3D : MonoBehaviour
    {
        [SerializeField] private BuildingId _buildingId;
        [SerializeField] private SiteId _siteId;
        [SerializeField] private SelectableWorldObject3D _kind;

        public BuildingId BuildingId => _buildingId;
        public SiteId SiteId => _siteId;
        public bool IsBuildSite => _kind == SelectableWorldObject3D.BuildSite;
        public bool IsSelectable => _kind != SelectableWorldObject3D.None;
        public SelectableWorldObject3D Kind => _kind;

        public void BindBuilding(BuildingId buildingId)
        {
            _buildingId = buildingId;
            _siteId = default;
            _kind = SelectableWorldObject3D.Building;
        }

        public void BindSite(SiteId siteId)
        {
            _buildingId = default;
            _siteId = siteId;
            _kind = SelectableWorldObject3D.BuildSite;
        }
    }
}
