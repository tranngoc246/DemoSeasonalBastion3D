namespace SeasonalBastion.Contracts
{
    public interface IWorldOps
    {
        BuildingId CreateBuilding(string buildingDefId, CellPos anchor, Dir4 rotation);
        void DestroyBuilding(BuildingId id);
        NpcId CreateNpc(string npcDefId, CellPos spawn);
        void DestroyNpc(NpcId id);
        EnemyId CreateEnemy(string enemyDefId, CellPos spawn, int lane);
        void DestroyEnemy(EnemyId id);
        SiteId CreateBuildSite(string buildingDefId, CellPos anchor, Dir4 rotation);
        void DestroyBuildSite(SiteId id);
    }
}
