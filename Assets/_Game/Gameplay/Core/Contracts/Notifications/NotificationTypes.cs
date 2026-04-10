namespace SeasonalBastion.Contracts
{
    public enum NotificationSeverity { Info, Warning, Error }

    public readonly struct NotificationPayload
    {
        public readonly BuildingId Building;
        public readonly TowerId Tower;
        public readonly string Extra;

        public NotificationPayload(BuildingId building, TowerId tower, string extra)
        {
            Building = building;
            Tower = tower;
            Extra = extra;
        }
    }
}
