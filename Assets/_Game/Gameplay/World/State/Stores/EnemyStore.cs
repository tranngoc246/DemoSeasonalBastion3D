using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class EnemyStore : EntityStore<EnemyId, EnemyState>, IEnemyStore
    {
        public override int ToInt(EnemyId id) => id.Value;
        public override EnemyId FromInt(int value) => new EnemyId(value);
    }
}
