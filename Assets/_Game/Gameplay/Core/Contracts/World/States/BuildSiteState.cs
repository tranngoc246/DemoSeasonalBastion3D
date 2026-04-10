using System.Collections.Generic;

namespace SeasonalBastion.Contracts
{
    public struct BuildSiteState
    {
        public SiteId Id;
        public string BuildingDefId;
        public int TargetLevel;
        public CellPos Anchor;
        public Dir4 Rotation;
        public bool IsActive;
        public float WorkSecondsDone;
        public float WorkSecondsTotal;
        public List<CostDef> DeliveredSoFar;
        public List<CostDef> RemainingCosts;
        public byte Kind;
        public BuildingId TargetBuilding;
        public string FromDefId;
        public string EdgeId;

        public bool IsReadyToWork => RemainingCosts == null || RemainingCosts.Count == 0;
        public bool IsUpgrade => Kind == 1;
    }
}
