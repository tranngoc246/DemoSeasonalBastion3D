namespace SeasonalBastion.Contracts
{
    public readonly struct BuildOrderAutoRoadCreatedEvent
    {
        public readonly int OrderId;
        public readonly CellPos RoadCell;

        public BuildOrderAutoRoadCreatedEvent(int orderId, CellPos roadCell)
        {
            OrderId = orderId;
            RoadCell = roadCell;
        }
    }
}
