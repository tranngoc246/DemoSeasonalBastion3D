namespace SeasonalBastion.Contracts
{
    public enum BuildOrderKind
    {
        PlaceNew = 0,
        Upgrade = 1,
        Repair = 2
    }

    public struct BuildOrder
    {
        public int OrderId;
        public BuildOrderKind Kind;
        public string BuildingDefId;
        public BuildingId TargetBuilding;
        public SiteId Site;
        public CostDef[] RequiredCost;
        public CostDef[] Delivered;
        public float WorkSecondsRequired;
        public float WorkSecondsDone;
        public bool Completed;
    }
}
