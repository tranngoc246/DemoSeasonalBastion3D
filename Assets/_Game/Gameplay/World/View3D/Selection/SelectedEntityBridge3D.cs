using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class SelectedEntityBridge3D : MonoBehaviour
    {
        [SerializeField] private BuildingId _buildingId;
        [SerializeField] private SiteId _siteId;
        [SerializeField] private bool _isBuildSite;

        public BuildingId BuildingId => _buildingId;
        public SiteId SiteId => _siteId;
        public bool IsBuildSite => _isBuildSite;

        public void BindBuilding(BuildingId buildingId)
        {
            _buildingId = buildingId;
            _siteId = default;
            _isBuildSite = false;
        }

        public void BindSite(SiteId siteId)
        {
            _buildingId = default;
            _siteId = siteId;
            _isBuildSite = true;
        }
    }
}
