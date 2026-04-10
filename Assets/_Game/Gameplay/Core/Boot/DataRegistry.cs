using System;
using System.Collections.Generic;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class DataRegistry : IDataRegistry
    {
        private readonly DefsCatalog _catalog;
        private readonly Dictionary<string, BuildingDef> _buildings = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, NpcDef> _npcs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TowerDef> _towers = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EnemyDef> _enemies = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, WaveDef> _waves = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RewardDef> _rewards = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RecipeDef> _recipes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BuildableNodeDef> _buildableNodes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, UpgradeEdgeDef> _upgradeEdgesById = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<UpgradeEdgeDef>> _upgradeEdgesFrom = new(StringComparer.OrdinalIgnoreCase);

        public DataRegistry(DefsCatalog catalog)
        {
            _catalog = catalog;
        }

        public void RegisterBuilding(BuildingDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _buildings[def.DefId] = def;
        }

        public void RegisterNpc(NpcDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _npcs[def.DefId] = def;
        }

        public void RegisterTower(TowerDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _towers[def.DefId] = def;
        }

        public void RegisterEnemy(EnemyDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _enemies[def.DefId] = def;
        }

        public void RegisterWave(WaveDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _waves[def.DefId] = def;
        }

        public void RegisterReward(RewardDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _rewards[def.DefId] = def;
        }

        public void RegisterRecipe(RecipeDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.DefId)) return;
            _recipes[def.DefId] = def;
        }

        public void RegisterBuildableNode(BuildableNodeDef node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.Id)) return;
            _buildableNodes[node.Id] = node;
        }

        public void RegisterUpgradeEdge(UpgradeEdgeDef edge)
        {
            if (edge == null || string.IsNullOrWhiteSpace(edge.Id)) return;
            _upgradeEdgesById[edge.Id] = edge;
            if (!_upgradeEdgesFrom.TryGetValue(edge.From ?? string.Empty, out var list))
            {
                list = new List<UpgradeEdgeDef>();
                _upgradeEdgesFrom[edge.From ?? string.Empty] = list;
            }
            list.Add(edge);
        }

        public T GetDef<T>(string id) where T : UnityEngine.Object
        {
            throw new NotSupportedException($"{nameof(DataRegistry)} does not support generic UnityEngine.Object lookup. Requested type={typeof(T).Name}, id='{id}'.");
        }

        public bool TryGetDef<T>(string id, out T def) where T : UnityEngine.Object
        {
            def = null;
            return false;
        }

        public BuildingDef GetBuilding(string id) => _buildings.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"BuildingDef not found: '{id}'");
        public bool TryGetBuilding(string id, out BuildingDef def) => _buildings.TryGetValue(id, out def);
        public EnemyDef GetEnemy(string id) => _enemies.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"EnemyDef not found: '{id}'");
        public bool TryGetEnemy(string id, out EnemyDef def) => _enemies.TryGetValue(id, out def);
        public WaveDef GetWave(string id) => _waves.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"WaveDef not found: '{id}'");
        public bool TryGetWave(string id, out WaveDef def) => _waves.TryGetValue(id, out def);
        public RewardDef GetReward(string id) => _rewards.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"RewardDef not found: '{id}'");
        public bool TryGetReward(string id, out RewardDef def) => _rewards.TryGetValue(id, out def);
        public RecipeDef GetRecipe(string id) => _recipes.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"RecipeDef not found: '{id}'");
        public bool TryGetRecipe(string id, out RecipeDef def) => _recipes.TryGetValue(id, out def);
        public NpcDef GetNpc(string id) => _npcs.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"NpcDef not found: '{id}'");
        public bool TryGetNpc(string id, out NpcDef def) => _npcs.TryGetValue(id, out def);
        public TowerDef GetTower(string id) => _towers.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"TowerDef not found: '{id}'");
        public bool TryGetTower(string id, out TowerDef def) => _towers.TryGetValue(id, out def);
        public bool TryGetBuildableNode(string id, out BuildableNodeDef node) => _buildableNodes.TryGetValue(id, out node);
        public IReadOnlyList<UpgradeEdgeDef> GetUpgradeEdgesFrom(string fromNodeId)
        {
            if (string.IsNullOrWhiteSpace(fromNodeId)) return Array.Empty<UpgradeEdgeDef>();
            return _upgradeEdgesFrom.TryGetValue(fromNodeId, out var list) ? list.AsReadOnly() : Array.Empty<UpgradeEdgeDef>();
        }
        public bool TryGetUpgradeEdge(string edgeId, out UpgradeEdgeDef edge) => _upgradeEdgesById.TryGetValue(edgeId, out edge);
        public bool IsPlaceableBuildable(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) return false;
            return !_buildableNodes.TryGetValue(nodeId, out var node) || node == null || node.Placeable;
        }
    }
}
