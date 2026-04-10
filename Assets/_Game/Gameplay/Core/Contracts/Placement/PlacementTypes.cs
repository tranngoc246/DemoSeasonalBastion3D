namespace SeasonalBastion.Contracts
{
    public enum PlacementFailReason
    {
        None,
        OutOfBounds,
        Overlap,
        NoRoadConnection,
        InvalidRotation,
        BlockedBySite,
        Unknown
    }

    public readonly struct PlacementResult
    {
        public readonly bool Ok;
        public readonly PlacementFailReason FailReason;
        public readonly CellPos SuggestedRoadCell;

        public PlacementResult(bool ok, PlacementFailReason failReason, CellPos suggestedRoadCell)
        {
            Ok = ok;
            FailReason = failReason;
            SuggestedRoadCell = suggestedRoadCell;
        }
    }
}
