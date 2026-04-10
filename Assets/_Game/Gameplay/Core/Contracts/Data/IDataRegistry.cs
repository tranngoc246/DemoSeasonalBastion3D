using System.Collections.Generic;

namespace SeasonalBastion.Contracts
{
    public interface IDataRegistry
    {
        T GetDef<T>(string id) where T : UnityEngine.Object;
        bool TryGetDef<T>(string id, out T def) where T : UnityEngine.Object;

        BuildingDef GetBuilding(string id);
        bool TryGetBuilding(string id, out BuildingDef def);

        EnemyDef GetEnemy(string id);
        bool TryGetEnemy(string id, out EnemyDef def);

        WaveDef GetWave(string id);
        bool TryGetWave(string id, out WaveDef def);

        RewardDef GetReward(string id);
        bool TryGetReward(string id, out RewardDef def);

        RecipeDef GetRecipe(string id);
        bool TryGetRecipe(string id, out RecipeDef def);

        NpcDef GetNpc(string id);
        bool TryGetNpc(string id, out NpcDef def);

        TowerDef GetTower(string id);
        bool TryGetTower(string id, out TowerDef def);

        bool TryGetBuildableNode(string id, out BuildableNodeDef node);
        IReadOnlyList<UpgradeEdgeDef> GetUpgradeEdgesFrom(string fromNodeId);
        bool TryGetUpgradeEdge(string edgeId, out UpgradeEdgeDef edge);
        bool IsPlaceableBuildable(string nodeId);
    }
}
