using System.Collections.Generic;
using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class ResourcePileStore : EntityStore<PileId, ResourcePileState>, IResourcePileStore
    {
        private readonly Dictionary<int, PileId> _byKey = new();

        public override int ToInt(PileId id) => id.Value;
        public override PileId FromInt(int value) => new PileId(value);

        public override void ClearAll()
        {
            base.ClearAll();
            _byKey.Clear();
        }

        public PileId AddOrIncrease(CellPos cell, ResourceType rt, int delta, BuildingId owner)
        {
            int key = MakeKey(cell, rt, owner);
            if (_byKey.TryGetValue(key, out var id) && Exists(id))
            {
                ResourcePileState st = Get(id);
                st.Amount += delta;
                if (st.Amount < 0) st.Amount = 0;
                Set(id, st);
                return id;
            }

            ResourcePileState created = new()
            {
                Cell = cell,
                Resource = rt,
                Amount = delta < 0 ? 0 : delta,
                OwnerBuilding = owner
            };

            PileId newId = Create(created);
            created.Id = newId;
            Set(newId, created);
            _byKey[key] = newId;
            return newId;
        }

        public bool TryTake(PileId id, int want, out int taken)
        {
            taken = 0;
            if (!Exists(id)) return false;

            ResourcePileState st = Get(id);
            if (st.Amount <= 0) return false;

            taken = want <= st.Amount ? want : st.Amount;
            st.Amount -= taken;
            if (st.Amount <= 0)
            {
                Destroy(id);
                _byKey.Remove(MakeKey(st.Cell, st.Resource, st.OwnerBuilding));
            }
            else
            {
                Set(id, st);
            }

            return true;
        }

        public bool TryFindNonEmpty(ResourceType rt, BuildingId owner, out PileId id)
        {
            foreach (PileId pid in Ids)
            {
                ResourcePileState st = Get(pid);
                if (st.Resource != rt) continue;
                if (st.OwnerBuilding.Value != owner.Value) continue;
                if (st.Amount > 0)
                {
                    id = pid;
                    return true;
                }
            }

            id = default;
            return false;
        }

        void IResourcePileStore.Set(PileId id, in ResourcePileState st) => Set(id, st);

        private static int MakeKey(CellPos c, ResourceType rt, BuildingId owner)
        {
            int xy = c.X + (c.Y << 7);
            return xy + ((int)rt << 14) + (owner.Value << 18);
        }
    }
}
