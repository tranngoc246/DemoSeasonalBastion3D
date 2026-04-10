using System.Collections.Generic;

namespace SeasonalBastion.Contracts
{
    public sealed class ZoneState
    {
        public int Id;
        public ResourceType Resource;
        public List<CellPos> Cells = new();
    }
}
