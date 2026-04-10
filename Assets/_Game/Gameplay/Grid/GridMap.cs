using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class GridMap : IGridMap
    {
        private readonly int _width;
        private readonly int _height;
        private readonly CellOccupancy[] _cells;

        public GridMap(int width, int height)
        {
            _width = width;
            _height = height;
            _cells = new CellOccupancy[_width * _height];
        }

        public int Width => _width;
        public int Height => _height;

        public bool IsInside(CellPos c) => c.X >= 0 && c.Y >= 0 && c.X < _width && c.Y < _height;

        public CellOccupancy Get(CellPos c)
        {
            if (!IsInside(c))
                return new CellOccupancy(CellOccupancyKind.Empty, default, default);

            return _cells[ToIndex(c)];
        }

        public bool IsRoad(CellPos c) => Get(c).Kind == CellOccupancyKind.Road;

        public bool IsBlocked(CellPos c)
        {
            CellOccupancyKind kind = Get(c).Kind;
            return kind == CellOccupancyKind.Building || kind == CellOccupancyKind.Site;
        }

        public void SetRoad(CellPos c, bool isRoad)
        {
            if (!IsInside(c))
                return;

            _cells[ToIndex(c)] = isRoad
                ? new CellOccupancy(CellOccupancyKind.Road, default, default)
                : new CellOccupancy(CellOccupancyKind.Empty, default, default);
        }

        public void SetBuilding(CellPos c, BuildingId id)
        {
            if (!IsInside(c))
                return;

            _cells[ToIndex(c)] = new CellOccupancy(CellOccupancyKind.Building, id, default);
        }

        public void ClearBuilding(CellPos c)
        {
            if (!IsInside(c))
                return;

            _cells[ToIndex(c)] = new CellOccupancy(CellOccupancyKind.Empty, default, default);
        }

        public void SetSite(CellPos c, SiteId id)
        {
            if (!IsInside(c))
                return;

            _cells[ToIndex(c)] = new CellOccupancy(CellOccupancyKind.Site, default, id);
        }

        public void ClearSite(CellPos c)
        {
            if (!IsInside(c))
                return;

            int index = ToIndex(c);
            if (_cells[index].Kind != CellOccupancyKind.Site)
                return;

            _cells[index] = new CellOccupancy(CellOccupancyKind.Empty, default, default);
        }

        public void ClearAll()
        {
            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = default;
        }

        private int ToIndex(CellPos c) => c.Y * _width + c.X;
    }
}
