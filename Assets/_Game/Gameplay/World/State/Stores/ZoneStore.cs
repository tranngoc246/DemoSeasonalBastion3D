using System;
using System.Collections.Generic;
using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class ZoneStore : IZoneStore
    {
        private readonly List<ZoneState> _zones = new();

        public IReadOnlyList<ZoneState> Zones => _zones;

        public void Clear() => _zones.Clear();
        public void Add(ZoneState zone) => _zones.Add(zone);

        public ZoneState GetByResource(ResourceType rt)
        {
            for (int i = 0; i < _zones.Count; i++)
            {
                if (_zones[i].Resource == rt)
                    return _zones[i];
            }

            throw new Exception("Zone missing for " + rt);
        }

        public CellPos PickCell(ResourceType rt, CellPos preferNear)
        {
            ZoneState zone = GetByResource(rt);
            List<CellPos> cells = zone.Cells;
            if (cells == null || cells.Count == 0)
                return preferNear;

            int bestIndex = 0;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                CellPos c = cells[i];
                int dx = c.X - preferNear.X; if (dx < 0) dx = -dx;
                int dy = c.Y - preferNear.Y; if (dy < 0) dy = -dy;
                int distance = dx + dy;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return cells[bestIndex];
        }
    }
}
