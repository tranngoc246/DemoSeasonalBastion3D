using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class GameplayRuntimeBootstrap : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TerrainGameplayRuntimeHost _terrainHost;
        [SerializeField] private WorldViewRoot3D _worldView;

        [Header("Demo content")]
        [SerializeField] private bool _seedDemoContent = true;

        public DataRegistry Data { get; private set; }
        public WorldState World { get; private set; }
        public EventBus Events { get; private set; }
        public WorldIndexService WorldIndex { get; private set; }
        public BuildOrderServiceStub BuildOrders { get; private set; }
        public PlacementService Placement { get; private set; }
        public WorldOps WorldOps { get; private set; }
        public RunStartRuntime RunStart { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            BuildOrders?.Tick(Time.deltaTime);
        }

        [ContextMenu("Initialize Gameplay Runtime")]
        public void Initialize()
        {
            if (_terrainHost == null)
                _terrainHost = FindObjectOfType<TerrainGameplayRuntimeHost>();

            if (_terrainHost == null)
            {
                Debug.LogWarning("[GameplayRuntimeBootstrap] Missing TerrainGameplayRuntimeHost.", this);
                return;
            }

            if (_terrainHost.GridMap == null)
                _terrainHost.Initialize();

            Data = BuildRegistry();
            World = new WorldState();
            Events = new EventBus();
            WorldIndex = new WorldIndexService(World, Data);
            Placement = new PlacementService(_terrainHost.GridMap, World, Data, WorldIndex, Events);
            RunStart = BuildRunStart(_terrainHost);
            BuildOrders = new BuildOrderServiceStub(World, _terrainHost.GridMap, Data, Events, WorldIndex);
            Placement.BindBuildOrders(BuildOrders);
            Placement.BindRunStart(RunStart);
            Placement.BindTerrainBridge(_terrainHost.Bridge);
            WorldOps = new WorldOps(World, Events, Data, WorldIndex, null, _terrainHost.GridMap, BuildOrders);
            WorldIndex.RebuildAll();

            if (_seedDemoContent)
                SeedDemoContent();

            if (_worldView == null)
                _worldView = FindObjectOfType<WorldViewRoot3D>();
            if (_worldView != null)
                _worldView.BindRuntime(this);
        }

        private DataRegistry BuildRegistry()
        {
            DataRegistry registry = new(null);
            registry.RegisterBuilding(new BuildingDef
            {
                DefId = "hq_l1",
                SizeX = 3,
                SizeY = 3,
                BaseLevel = 1,
                MaxHp = 250,
                IsHQ = true,
                IsWarehouse = true,
                BuildChunksL1 = 6
            });
            registry.RegisterBuilding(new BuildingDef
            {
                DefId = "tower_arrow_l1",
                SizeX = 2,
                SizeY = 2,
                BaseLevel = 1,
                MaxHp = 120,
                IsTower = true,
                BuildChunksL1 = 4
            });
            registry.RegisterNpc(new NpcDef { DefId = "worker", Role = "Worker", BaseMoveSpeed = 1f, RoadSpeedMultiplier = 1.25f });
            registry.RegisterEnemy(new EnemyDef { DefId = "slime", MaxHp = 20, MoveSpeed = 1f, DamageToHQ = 1, DamageToBuildings = 1 });
            registry.RegisterTower(new TowerDef { DefId = "tower_arrow_l1", Tier = 1, MaxHp = 120, Range = 5f, Rof = 1f, Damage = 8, AmmoMax = 20, AmmoPerShot = 1, BuildChunks = 4 });
            return registry;
        }

        private RunStartRuntime BuildRunStart(TerrainGameplayRuntimeHost host)
        {
            int width = host.GeneratedWorld != null ? host.GeneratedWorld.Width : 0;
            int height = host.GeneratedWorld != null ? host.GeneratedWorld.Height : 0;
            int marginX = Mathf.Max(2, Mathf.RoundToInt(width * 0.1f));
            int marginY = Mathf.Max(2, Mathf.RoundToInt(height * 0.1f));

            RunStartRuntime runtime = new()
            {
                Seed = 0,
                MapWidth = width,
                MapHeight = height,
                OpeningQualityBand = "prototype",
                ResourceGenerationModeRequested = "terrain-bridge",
                ResourceGenerationModeApplied = "terrain-bridge",
                BuildableRect = new IntRect(marginX, marginY, Mathf.Max(marginX, width - marginX - 1), Mathf.Max(marginY, height - marginY - 1))
            };

            CellPos center = new(width / 2, height / 2);
            runtime.SpawnGates.Add(new SpawnGate(0, new CellPos(width - 3, center.Y), Dir4.W));
            runtime.Lanes[0] = new LaneRuntime(0, new CellPos(width - 3, center.Y), Dir4.W, center);
            return runtime;
        }

        private void SeedDemoContent()
        {
            if (World == null || _terrainHost?.GeneratedWorld == null)
                return;

            foreach (var id in World.Buildings.Ids)
                return;

            CellPos center = new(_terrainHost.GeneratedWorld.Width / 2, _terrainHost.GeneratedWorld.Height / 2);
            CellPos towerCell = new(center.X + 5, center.Y + 2);
            CellPos npcCell = new(center.X - 2, center.Y - 2);
            CellPos enemyCell = new(center.X + 8, center.Y - 6);

            WorldOps.CreateBuilding("hq_l1", center, Dir4.N);
            WorldOps.CreateBuilding("tower_arrow_l1", towerCell, Dir4.N);
            WorldOps.CreateNpc("worker", npcCell);
            WorldOps.CreateEnemy("slime", enemyCell, 0);
        }
    }
}
