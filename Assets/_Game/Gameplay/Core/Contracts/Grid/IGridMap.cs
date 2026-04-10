namespace SeasonalBastion.Contracts
{
    public interface IGridMap
    {
        int Width { get; }
        int Height { get; }

        bool IsInside(CellPos c);
        CellOccupancy Get(CellPos c);

        bool IsRoad(CellPos c);
        bool IsBlocked(CellPos c);

        void SetRoad(CellPos c, bool isRoad);
        void SetBuilding(CellPos c, BuildingId id);
        void ClearBuilding(CellPos c);
        void SetSite(CellPos c, SiteId id);
        void ClearSite(CellPos c);
        void ClearAll();
    }
}
