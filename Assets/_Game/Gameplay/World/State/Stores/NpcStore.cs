using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class NpcStore : EntityStore<NpcId, NpcState>, INpcStore
    {
        public override int ToInt(NpcId id) => id.Value;
        public override NpcId FromInt(int value) => new NpcId(value);
    }
}
