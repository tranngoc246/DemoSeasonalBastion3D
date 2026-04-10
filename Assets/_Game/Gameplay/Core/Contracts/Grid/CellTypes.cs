namespace SeasonalBastion.Contracts
{
    public readonly struct CellPos
    {
        public readonly int X;
        public readonly int Y;

        public CellPos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public enum Dir4
    {
        N,
        E,
        S,
        W
    }
}
