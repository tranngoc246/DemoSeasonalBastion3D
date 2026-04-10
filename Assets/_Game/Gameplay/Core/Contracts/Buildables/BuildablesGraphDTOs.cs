using System;

namespace SeasonalBastion.Contracts
{
    [Serializable]
    public sealed class BuildableNodeDef
    {
        public string Id = "";
        public int Level = 1;
        public bool Placeable = true;
    }

    [Serializable]
    public sealed class UpgradeEdgeDef
    {
        public string Id = "";
        public string From = "";
        public string To = "";
        public CostDef[] Cost;
        public int WorkChunks;
        public string RequiresUnlocked = "";
    }
}
