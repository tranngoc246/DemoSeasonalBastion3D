namespace SeasonalBastion.Contracts
{
    public enum JobArchetype
    {
        Leisure,
        Inspect,
        Harvest,
        HaulBasic,
        HaulToForge,
        BuildDeliver,
        BuildWork,
        RepairWork,
        CraftAmmo,
        HaulAmmoToArmory,
        ResupplyTower
    }

    public enum JobStatus { Created, Claimed, InProgress, Completed, Failed, Cancelled }

    public struct Job
    {
        public JobId Id;
        public JobArchetype Archetype;
        public JobStatus Status;
        public NpcId ClaimedBy;
        public BuildingId Workplace;
        public BuildingId SourceBuilding;
        public BuildingId DestBuilding;
        public SiteId Site;
        public TowerId Tower;
        public ResourceType ResourceType;
        public int Amount;
        public CellPos TargetCell;
        public float CreatedAt;
    }
}
