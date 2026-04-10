using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class BuildingStore : EntityStore<BuildingId, BuildingState>, IBuildingStore
    {
        public override int ToInt(BuildingId id) => id.Value;
        public override BuildingId FromInt(int value) => new BuildingId(value);
    }
}
