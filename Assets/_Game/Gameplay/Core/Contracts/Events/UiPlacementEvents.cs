namespace SeasonalBastion.Contracts
{
    public enum UiToolMode
    {
        None,
        Road,
        RemoveRoad
    }

    public readonly struct UiBeginPlaceBuildingEvent
    {
        public readonly string DefId;
        public UiBeginPlaceBuildingEvent(string defId) { DefId = defId; }
    }

    public readonly struct UiToolModeRequestedEvent
    {
        public readonly UiToolMode Mode;
        public UiToolModeRequestedEvent(UiToolMode mode) { Mode = mode; }
    }
}
