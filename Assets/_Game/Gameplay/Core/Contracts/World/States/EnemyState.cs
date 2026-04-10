namespace SeasonalBastion.Contracts
{
    public struct EnemyState
    {
        public EnemyId Id;
        public string DefId;
        public CellPos Cell;
        public int Hp;
        public int Lane;
        public float MoveProgress01;
        public string WaveId;
        public int WaveYear;
        public int WaveDay;
        public Season WaveSeason;
    }
}
