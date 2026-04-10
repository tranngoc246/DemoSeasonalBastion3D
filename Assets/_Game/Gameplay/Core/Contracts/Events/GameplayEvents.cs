namespace SeasonalBastion.Contracts
{
    public readonly struct RoadPlacedEvent
    {
        public readonly CellPos Cell;
        public RoadPlacedEvent(CellPos cell) { Cell = cell; }
    }

    public readonly struct BuildingPlacedEvent
    {
        public readonly string DefId;
        public readonly BuildingId BuildingId;
        public BuildingPlacedEvent(string defId, BuildingId buildingId) { DefId = defId; BuildingId = buildingId; }
    }

    public readonly struct BuildingDestroyedEvent
    {
        public readonly string DefId;
        public readonly BuildingId BuildingId;
        public BuildingDestroyedEvent(string defId, BuildingId buildingId) { DefId = defId; BuildingId = buildingId; }
    }

    public readonly struct BuildSitePlacedEvent
    {
        public readonly string DefId;
        public readonly SiteId SiteId;
        public BuildSitePlacedEvent(string defId, SiteId siteId) { DefId = defId; SiteId = siteId; }
    }

    public readonly struct BuildSiteCompletedEvent
    {
        public readonly string DefId;
        public readonly SiteId SiteId;
        public readonly BuildingId BuildingId;
        public BuildSiteCompletedEvent(string defId, SiteId siteId, BuildingId buildingId) { DefId = defId; SiteId = siteId; BuildingId = buildingId; }
    }

    public readonly struct WorldStateChangedEvent
    {
        public readonly string Kind;
        public readonly int Id;
        public WorldStateChangedEvent(string kind, int id) { Kind = kind; Id = id; }
    }
}
