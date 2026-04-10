namespace SeasonalBastion.Contracts
{
    public enum CellOccupancyKind { Empty, Road, Building, Site }

    public readonly struct CellOccupancy
    {
        public readonly CellOccupancyKind Kind;
        public readonly BuildingId Building;
        public readonly SiteId Site;

        public CellOccupancy(CellOccupancyKind kind, BuildingId building, SiteId site)
        {
            Kind = kind;
            Building = building;
            Site = site;
        }
    }
}
