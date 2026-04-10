using System.Collections.Generic;
using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class WorldIndexService : IWorldIndex
    {
        private readonly IWorldState _world;
        private readonly IDataRegistry _data;
        private readonly List<BuildingId> _warehouses = new();
        private readonly List<BuildingId> _producers = new();
        private readonly List<BuildingId> _houses = new();
        private readonly List<BuildingId> _forges = new();
        private readonly List<BuildingId> _armories = new();
        private readonly List<TowerId> _towers = new();
        private readonly HashSet<int> _warehouseSet = new();
        private readonly HashSet<int> _producerSet = new();
        private readonly HashSet<int> _houseSet = new();
        private readonly HashSet<int> _forgeSet = new();
        private readonly HashSet<int> _armorySet = new();
        private readonly HashSet<int> _towerSet = new();

        public IReadOnlyList<BuildingId> Warehouses => _warehouses;
        public IReadOnlyList<BuildingId> Producers => _producers;
        public IReadOnlyList<BuildingId> Houses => _houses;
        public IReadOnlyList<BuildingId> Forges => _forges;
        public IReadOnlyList<BuildingId> Armories => _armories;
        public IReadOnlyList<TowerId> Towers => _towers;

        public WorldIndexService(IWorldState world, IDataRegistry data)
        {
            _world = world;
            _data = data;
        }

        public void RebuildAll()
        {
            ClearAll();
            foreach (BuildingId id in _world.Buildings.Ids)
                OnBuildingCreated(id);
            foreach (TowerId id in _world.Towers.Ids)
                OnTowerCreated(id);
        }

        public void OnBuildingCreated(BuildingId id)
        {
            if (!_world.Buildings.Exists(id)) return;
            BuildingState st = _world.Buildings.Get(id);
            if (!st.IsConstructed) return;
            if (!_data.TryGetBuilding(st.DefId, out var def) || def == null) return;

            ResolveTags(st.DefId, def, out bool isHQ, out bool isWarehouse, out bool isProducer, out bool isHouse, out bool isForge, out bool isArmory);
            if (isHQ) isWarehouse = true;
            if (isWarehouse) AddUnique(_warehouses, _warehouseSet, id.Value, id);
            if (isProducer) AddUnique(_producers, _producerSet, id.Value, id);
            if (isHouse) AddUnique(_houses, _houseSet, id.Value, id);
            if (isForge) AddUnique(_forges, _forgeSet, id.Value, id);
            if (isArmory) AddUnique(_armories, _armorySet, id.Value, id);
        }

        public void OnBuildingDestroyed(BuildingId id)
        {
            Remove(_warehouses, _warehouseSet, id.Value);
            Remove(_producers, _producerSet, id.Value);
            Remove(_houses, _houseSet, id.Value);
            Remove(_forges, _forgeSet, id.Value);
            Remove(_armories, _armorySet, id.Value);
        }

        public void OnTowerCreated(TowerId id)
        {
            if (id.Value == 0) return;
            if (_world.Towers == null || !_world.Towers.Exists(id)) return;
            AddUnique(_towers, _towerSet, id.Value, id);
        }

        public void OnTowerDestroyed(TowerId id)
        {
            Remove(_towers, _towerSet, id.Value);
        }

        private void ClearAll()
        {
            _warehouses.Clear(); _warehouseSet.Clear();
            _producers.Clear(); _producerSet.Clear();
            _houses.Clear(); _houseSet.Clear();
            _forges.Clear(); _forgeSet.Clear();
            _armories.Clear(); _armorySet.Clear();
            _towers.Clear(); _towerSet.Clear();
        }

        private static void AddUnique(List<BuildingId> list, HashSet<int> set, int key, BuildingId id)
        {
            if (!set.Add(key)) return;
            list.Add(id);
            list.Sort((a, b) => a.Value.CompareTo(b.Value));
        }

        private static void AddUnique(List<TowerId> list, HashSet<int> set, int key, TowerId id)
        {
            if (!set.Add(key)) return;
            list.Add(id);
            list.Sort((a, b) => a.Value.CompareTo(b.Value));
        }

        private static void Remove(List<BuildingId> list, HashSet<int> set, int key)
        {
            if (!set.Remove(key)) return;
            list.RemoveAll(x => x.Value == key);
        }

        private static void Remove(List<TowerId> list, HashSet<int> set, int key)
        {
            if (!set.Remove(key)) return;
            list.RemoveAll(x => x.Value == key);
        }

        private static void ResolveTags(string defId, BuildingDef def, out bool isHQ, out bool isWarehouse, out bool isProducer, out bool isHouse, out bool isForge, out bool isArmory)
        {
            bool anyTagged = def.IsHQ || def.IsWarehouse || def.IsProducer || def.IsHouse || def.IsForge || def.IsArmory || def.IsTower;
            if (anyTagged)
            {
                isHQ = def.IsHQ;
                isWarehouse = def.IsWarehouse;
                isProducer = def.IsProducer;
                isHouse = def.IsHouse;
                isForge = def.IsForge;
                isArmory = def.IsArmory;
                return;
            }

            isHQ = DefIdTierUtil.IsBase(defId, "bld_hq");
            isWarehouse = DefIdTierUtil.IsBase(defId, "bld_warehouse");
            isProducer = DefIdTierUtil.IsBase(defId, "bld_farmhouse") || DefIdTierUtil.IsBase(defId, "bld_lumbercamp") || DefIdTierUtil.IsBase(defId, "bld_quarry") || DefIdTierUtil.IsBase(defId, "bld_ironhut") || DefIdTierUtil.IsBase(defId, "bld_forge");
            isHouse = DefIdTierUtil.IsBase(defId, "bld_house");
            isForge = DefIdTierUtil.IsBase(defId, "bld_forge");
            isArmory = DefIdTierUtil.IsBase(defId, "bld_armory");
        }
    }
}
