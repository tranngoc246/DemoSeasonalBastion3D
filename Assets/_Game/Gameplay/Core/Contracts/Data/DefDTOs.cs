using System;

namespace SeasonalBastion.Contracts
{
    [Flags]
    public enum WorkRoleFlags
    {
        None = 0,
        Harvest = 1 << 0,
        HaulBasic = 1 << 1,
        Build = 1 << 2,
        Craft = 1 << 3,
        Armory = 1 << 4,
    }

    public sealed class BuildingDef
    {
        public string DefId = "";
        public int SizeX = 1;
        public int SizeY = 1;
        public int BaseLevel = 1;
        public int MaxHp = 1;
        public bool IsHQ;
        public bool IsWarehouse;
        public bool IsProducer;
        public bool IsHouse;
        public bool IsForge;
        public bool IsArmory;
        public bool IsTower;
        public WorkRoleFlags WorkRoles = WorkRoleFlags.None;
        public StorageCapsByLevel CapWood;
        public StorageCapsByLevel CapFood;
        public StorageCapsByLevel CapStone;
        public StorageCapsByLevel CapIron;
        public StorageCapsByLevel CapAmmo;
        public CostDef[] BuildCostsL1;
        public int BuildChunksL1;
    }

    [Serializable]
    public struct StorageCapsByLevel
    {
        public int L1;
        public int L2;
        public int L3;

        public int Get(int level)
        {
            return level switch
            {
                1 => L1,
                2 => L2,
                3 => L3,
                _ => L1
            };
        }
    }

    public sealed class EnemyDef
    {
        public string DefId = "";
        public int MaxHp = 1;
        public float MoveSpeed = 1f;
        public int DamageToHQ = 1;
        public int DamageToBuildings = 1;
        public float Range;
        public bool IsBoss;
        public int BossYear;
        public Season BossSeason = Season.Spring;
        public int BossDay;
        public float AuraSlowRofPct;
    }

    public sealed class WaveDef
    {
        public string DefId = "";
        public int WaveIndex;
        public int Year = 1;
        public Season Season = Season.Autumn;
        public int Day = 1;
        public bool IsBoss;
        public bool IsFinalWave;
        public WaveEntryDef[] Entries;
    }

    public sealed class RewardDef
    {
        public string DefId = "";
        public string Title = "";
    }

    public sealed class RecipeDef
    {
        public string DefId = "";
        public ResourceType InputType;
        public int InputAmount = 1;
        public ResourceType OutputType;
        public int OutputAmount = 1;
        public CostDef[] ExtraInputs;
        public float CraftTimeSec;
    }

    [Serializable]
    public struct UnlockDef
    {
        public int Year;
        public Season Season;
        public int Day;
    }

    [Serializable]
    public struct WaveEntryDef
    {
        public string EnemyId;
        public int Count;
    }

    public sealed class NpcDef
    {
        public string DefId = "";
        public string Role = "";
        public float BaseMoveSpeed = 1f;
        public float RoadSpeedMultiplier = 1f;
        public StorageCapsByLevel CarryCore;
    }

    public sealed class TowerDef
    {
        public string DefId = "";
        public int Tier = 1;
        public int MaxHp = 1;
        public float Range = 1f;
        public float Rof = 1f;
        public int Damage = 1;
        public float SlowPct;
        public float SlowSec;
        public string Aoe = "";
        public int DotDps;
        public int DotSec;
        public int AmmoMax;
        public int AmmoPerShot = 1;
        public float NeedsAmmoThresholdPct = 0.25f;
        public CostDef[] BuildCost;
        public int BuildChunks = 1;
        public UnlockDef Unlock;
    }

    public sealed class CostDef
    {
        public ResourceType Resource;
        public int Amount;
    }
}
