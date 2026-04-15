using System.Collections.Generic;
using SeasonalBastion.Contracts;
using SeasonalBastion.WorldGen.Runtime.Models;
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
        [SerializeField] private bool _seedDemoRoadCross = true;
        [SerializeField] private bool _autoAdvanceDemoBuildOrders = true;
        [SerializeField] private float _demoBuildTickMultiplier = 8f;
        [SerializeField] private int _demoRoadHalfLength = 8;

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
            float tickDt = Time.deltaTime;
            if (_autoAdvanceDemoBuildOrders)
                tickDt *= Mathf.Max(1f, _demoBuildTickMultiplier);
            BuildOrders?.Tick(tickDt);
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
            IntRect buildableRect = DeriveBuildableRect(host);
            CellPos hqCell = DeriveHqCell(host, buildableRect);

            RunStartRuntime runtime = new()
            {
                Seed = 0,
                MapWidth = width,
                MapHeight = height,
                OpeningQualityBand = "prototype",
                ResourceGenerationModeRequested = "terrain-bridge",
                ResourceGenerationModeApplied = "terrain-bridge",
                BuildableRect = buildableRect
            };

            AddStartZones(runtime, hqCell, buildableRect);
            AddSpawnLanes(runtime, host, hqCell, buildableRect);
            runtime.LockedInvariants.Add("terrain-derived-buildable-rect");
            runtime.LockedInvariants.Add("terrain-derived-start-zone");
            runtime.LockedInvariants.Add("terrain-derived-spawn-lanes");
            return runtime;
        }

        private static CellPos DeriveHqCell(TerrainGameplayRuntimeHost host, IntRect buildableRect)
        {
            var world = host.GeneratedWorld;
            StartAreaDefinition startArea = host.Bridge != null ? host.Bridge.GetStartArea() : default;
            int desiredX = Mathf.RoundToInt(startArea.Center.x);
            int desiredY = Mathf.RoundToInt(startArea.Center.y);

            if (world?.BuildableMap == null)
                return new CellPos(Mathf.Max(buildableRect.XMin, desiredX), Mathf.Max(buildableRect.YMin, desiredY));

            CellPos best = new(
                Mathf.Clamp(desiredX, buildableRect.XMin, buildableRect.XMax),
                Mathf.Clamp(desiredY, buildableRect.YMin, buildableRect.YMax));

            float bestDistSq = float.MaxValue;
            for (int y = buildableRect.YMin; y <= buildableRect.YMax; y++)
            {
                for (int x = buildableRect.XMin; x <= buildableRect.XMax; x++)
                {
                    if (!world.BuildableMap[x, y])
                        continue;

                    float dx = x - desiredX;
                    float dy = y - desiredY;
                    float distSq = dx * dx + dy * dy;
                    if (distSq >= bestDistSq)
                        continue;

                    best = new CellPos(x, y);
                    bestDistSq = distSq;
                }
            }

            return best;
        }

        private static void AddStartZones(RunStartRuntime runtime, CellPos hqCell, IntRect buildableRect)
        {
            int zoneRadiusX = Mathf.Max(4, (buildableRect.XMax - buildableRect.XMin) / 8);
            int zoneRadiusY = Mathf.Max(4, (buildableRect.YMax - buildableRect.YMin) / 8);
            IntRect startZone = new(
                Mathf.Max(buildableRect.XMin, hqCell.X - zoneRadiusX),
                Mathf.Max(buildableRect.YMin, hqCell.Y - zoneRadiusY),
                Mathf.Min(buildableRect.XMax, hqCell.X + zoneRadiusX),
                Mathf.Min(buildableRect.YMax, hqCell.Y + zoneRadiusY));

            int cellCount = Mathf.Max(0, (startZone.XMax - startZone.XMin + 1) * (startZone.YMax - startZone.YMin + 1));
            runtime.Zones["start"] = new ZoneRect("start", "build", "hq_l1", startZone, cellCount, "terrain-start-area");
            runtime.Zones["hq_core"] = new ZoneRect("hq_core", "hq", "hq_l1", startZone, cellCount, "terrain-start-area");
        }

        private static void AddSpawnLanes(RunStartRuntime runtime, TerrainGameplayRuntimeHost host, CellPos hqCell, IntRect buildableRect)
        {
            var world = host.GeneratedWorld;
            if (world?.BuildableMap == null)
                return;

            List<(CellPos cell, Dir4 dir)> candidates = new(4)
            {
                (FindEdgeBuildableCell(world.BuildableMap, buildableRect, Edge.Right, hqCell), Dir4.W),
                (FindEdgeBuildableCell(world.BuildableMap, buildableRect, Edge.Left, hqCell), Dir4.E),
                (FindEdgeBuildableCell(world.BuildableMap, buildableRect, Edge.Top, hqCell), Dir4.S),
                (FindEdgeBuildableCell(world.BuildableMap, buildableRect, Edge.Bottom, hqCell), Dir4.N),
            };

            int laneId = 0;
            foreach (var candidate in candidates)
            {
                if (!IsValidCell(candidate.cell, world.Width, world.Height))
                    continue;

                runtime.SpawnGates.Add(new SpawnGate(laneId, candidate.cell, candidate.dir));
                runtime.Lanes[laneId] = new LaneRuntime(laneId, candidate.cell, candidate.dir, hqCell);
                laneId++;
            }
        }

        private static IntRect DeriveBuildableRect(TerrainGameplayRuntimeHost host)
        {
            var world = host.GeneratedWorld;
            if (world?.BuildableMap == null)
                return default;

            int width = world.Width;
            int height = world.Height;
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!world.BuildableMap[x, y])
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
                return default;

            return new IntRect(minX, minY, maxX, maxY);
        }

        private static CellPos FindEdgeBuildableCell(bool[,] buildableMap, IntRect rect, Edge edge, CellPos target)
        {
            CellPos best = default;
            float bestDistSq = float.MaxValue;
            bool found = false;

            switch (edge)
            {
                case Edge.Left:
                    for (int y = rect.YMin; y <= rect.YMax; y++)
                        Consider(rect.XMin, y);
                    break;
                case Edge.Right:
                    for (int y = rect.YMin; y <= rect.YMax; y++)
                        Consider(rect.XMax, y);
                    break;
                case Edge.Top:
                    for (int x = rect.XMin; x <= rect.XMax; x++)
                        Consider(x, rect.YMax);
                    break;
                case Edge.Bottom:
                    for (int x = rect.XMin; x <= rect.XMax; x++)
                        Consider(x, rect.YMin);
                    break;
            }

            return found ? best : default;

            void Consider(int x, int y)
            {
                if (!buildableMap[x, y])
                    return;

                float dx = x - target.X;
                float dy = y - target.Y;
                float distSq = dx * dx + dy * dy;
                if (distSq >= bestDistSq)
                    return;

                best = new CellPos(x, y);
                bestDistSq = distSq;
                found = true;
            }
        }

        private static bool IsValidCell(CellPos cell, int width, int height)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < width && cell.Y < height;
        }

        private enum Edge
        {
            Left,
            Right,
            Top,
            Bottom,
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

            if (_seedDemoRoadCross)
                SeedDemoRoads(center);

            WorldOps.CreateNpc("worker", npcCell);
            WorldOps.CreateEnemy("slime", enemyCell, 0);
        }

        private void SeedDemoRoads(CellPos center)
        {
            if (Placement == null || _terrainHost?.Bridge == null)
                return;

            int halfLength = Mathf.Max(2, _demoRoadHalfLength);
            for (int dx = -halfLength; dx <= halfLength; dx++)
            {
                TryPlaceDemoRoad(new CellPos(center.X + dx, center.Y));
            }

            for (int dy = -halfLength; dy <= halfLength; dy++)
            {
                TryPlaceDemoRoad(new CellPos(center.X, center.Y + dy));
            }
        }

        private void TryPlaceDemoRoad(CellPos cell)
        {
            if (_terrainHost?.GridMap == null || !_terrainHost.GridMap.IsInside(cell))
                return;
            if (_terrainHost?.Bridge != null && !_terrainHost.Bridge.IsBuildable(cell))
                return;

            if (!_terrainHost.GridMap.IsRoad(cell))
                _terrainHost.GridMap.SetRoad(cell, true);
        }
    }
}
