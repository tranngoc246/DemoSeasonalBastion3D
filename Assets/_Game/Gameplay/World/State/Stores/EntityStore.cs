using System.Collections.Generic;
using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public abstract class EntityStore<TId, TState> : IEntityStore<TId, TState>
    {
        protected readonly Dictionary<int, TState> Map = new();
        protected readonly List<int> IdList = new();
        protected int NextId = 1;
        protected int VersionValue;

        public abstract int ToInt(TId id);
        public abstract TId FromInt(int value);

        public bool Exists(TId id) => Map.ContainsKey(ToInt(id));
        public TState Get(TId id) => Map[ToInt(id)];

        public void Set(TId id, TState state)
        {
            Map[ToInt(id)] = state;
            VersionValue++;
        }

        public TId Create(TState state)
        {
            TId id = FromInt(NextId++);
            int key = ToInt(id);
            Map[key] = state;
            IdList.Add(key);
            VersionValue++;
            return id;
        }

        public void Destroy(TId id)
        {
            int key = ToInt(id);
            if (!Map.Remove(key))
                return;

            IdList.Remove(key);
            VersionValue++;
        }

        public virtual void ClearAll()
        {
            Map.Clear();
            IdList.Clear();
            NextId = 1;
            VersionValue++;
        }

        public int Count => Map.Count;
        public int Version => VersionValue;

        public IEnumerable<TId> Ids
        {
            get
            {
                for (int i = 0; i < IdList.Count; i++)
                    yield return FromInt(IdList[i]);
            }
        }
    }
}
