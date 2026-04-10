namespace SeasonalBastion.Contracts
{
    public interface IJobBoard
    {
        JobId Enqueue(Job job);
        bool TryPeekForWorkplace(BuildingId workplace, out Job job);
        bool TryClaim(JobId id, NpcId npc);
        bool TryGet(JobId id, out Job job);
        void Update(Job job);
        void Cancel(JobId id);
        void ClearAll();
        int CountForWorkplace(BuildingId workplace);
    }
}
