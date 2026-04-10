using System.Collections.Generic;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class WorldViewRoot3D : MonoBehaviour
    {
        [Header("Runtime references")]
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;
        [SerializeField] private GameplayRuntimeBootstrap _gameplayBootstrap;
        [SerializeField] private PrefabCatalog3D _prefabCatalog;

        [Header("View options")]
        [SerializeField] private Vector3 _buildingVisualOffset = Vector3.zero;
        [SerializeField] private Vector3 _buildSiteVisualOffset = Vector3.up * 0.05f;
        [SerializeField] private Vector3 _actorVisualOffset = Vector3.up * 0.25f;

        private readonly Dictionary<int, GameObject> _buildingViews = new();
        private readonly Dictionary<int, GameObject> _buildSiteViews = new();
        private readonly Dictionary<int, GameObject> _npcViews = new();
        private readonly Dictionary<int, GameObject> _enemyViews = new();

        private Transform _buildingsRoot;
        private Transform _buildSitesRoot;
        private Transform _npcsRoot;
        private Transform _enemiesRoot;

        private WorldState _world;
        private DataRegistry _data;

        private void Awake()
        {
            EnsureRoots();
            ResolveRuntime();
            RefreshAll();
        }

        private void LateUpdate()
        {
            RefreshAll();
        }

        public void BindRuntime(GameplayRuntimeBootstrap bootstrap)
        {
            _gameplayBootstrap = bootstrap;
            ResolveRuntime();
            RefreshAll();
        }

        [ContextMenu("Refresh 3D World Views")]
        public void RefreshAll()
        {
            if (_runtimeHost == null || _runtimeHost.Mapper == null)
                return;

            if (_world == null)
                return;

            SyncBuildSites();
            SyncBuildings();
            SyncNpcs();
            SyncEnemies();
        }

        private void ResolveRuntime()
        {
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
            if (_runtimeHost == null)
                return;
            if (_runtimeHost.GridMap == null)
                _runtimeHost.Initialize();

            if (_gameplayBootstrap == null)
                _gameplayBootstrap = FindObjectOfType<GameplayRuntimeBootstrap>();
            if (_gameplayBootstrap == null)
                return;

            if (_gameplayBootstrap.World == null)
                _gameplayBootstrap.Initialize();

            _world = _gameplayBootstrap.World;
            _data = _gameplayBootstrap.Data;
        }

        private void SyncBuildSites()
        {
            HashSet<int> alive = new();
            foreach (var id in _world.Sites.Ids)
            {
                int key = id.Value;
                alive.Add(key);
                BuildSiteState state = _world.Sites.Get(id);

                if (!_buildSiteViews.TryGetValue(key, out var view) || view == null)
                {
                    view = CreateView(_buildSitesRoot, $"S_{key}", null, PrimitiveType.Cylinder);
                    ApplyBuildSiteMaterial(view);
                    _buildSiteViews[key] = view;
                }

                _data.TryGetBuilding(state.BuildingDefId, out var buildingDef);
                int sizeX = buildingDef != null ? Mathf.Max(1, buildingDef.SizeX) : 1;
                int sizeY = buildingDef != null ? Mathf.Max(1, buildingDef.SizeY) : 1;
                PositionFootprintView(view.transform, state.Anchor, sizeX, sizeY, _buildSiteVisualOffset);
                view.transform.localScale = new Vector3(sizeX * _runtimeHost.Mapper.CellSize * 0.75f, 0.2f, sizeY * _runtimeHost.Mapper.CellSize * 0.75f);
            }

            RemoveStale(_buildSiteViews, alive);
        }

        private void SyncBuildings()
        {
            HashSet<int> alive = new();
            foreach (var id in _world.Buildings.Ids)
            {
                int key = id.Value;
                alive.Add(key);
                BuildingState state = _world.Buildings.Get(id);

                if (!state.IsConstructed)
                    continue;

                if (!_buildingViews.TryGetValue(key, out var view) || view == null)
                {
                    _data.TryGetBuilding(state.DefId, out var def);
                    view = CreateView(_buildingsRoot, $"B_{key}", _prefabCatalog != null ? _prefabCatalog.GetBuildingPrefab(def) : null, PrimitiveType.Cube);
                    _buildingViews[key] = view;
                }

                _data.TryGetBuilding(state.DefId, out var buildingDef);
                int sizeX = buildingDef != null ? Mathf.Max(1, buildingDef.SizeX) : 1;
                int sizeY = buildingDef != null ? Mathf.Max(1, buildingDef.SizeY) : 1;
                PositionFootprintView(view.transform, state.Anchor, sizeX, sizeY, _buildingVisualOffset);
                view.transform.localScale = ComputeBuildingScale(sizeX, sizeY);
            }

            RemoveUnconstructedBuildingViews(alive);
            RemoveStale(_buildingViews, alive);
        }

        private void SyncNpcs()
        {
            HashSet<int> alive = new();
            foreach (var id in _world.Npcs.Ids)
            {
                int key = id.Value;
                alive.Add(key);
                NpcState state = _world.Npcs.Get(id);

                if (!_npcViews.TryGetValue(key, out var view) || view == null)
                {
                    _data.TryGetNpc(state.DefId, out var def);
                    view = CreateView(_npcsRoot, $"N_{key}", _prefabCatalog != null ? _prefabCatalog.GetNpcPrefab(def) : null, PrimitiveType.Capsule);
                    _npcViews[key] = view;
                }

                view.transform.position = _runtimeHost.Mapper.CellToWorldCenter(state.Cell) + _actorVisualOffset;
                view.transform.localScale = _prefabCatalog != null ? _prefabCatalog.actorScale : Vector3.one * 0.75f;
            }

            RemoveStale(_npcViews, alive);
        }

        private void SyncEnemies()
        {
            HashSet<int> alive = new();
            foreach (var id in _world.Enemies.Ids)
            {
                int key = id.Value;
                alive.Add(key);
                EnemyState state = _world.Enemies.Get(id);

                if (!_enemyViews.TryGetValue(key, out var view) || view == null)
                {
                    _data.TryGetEnemy(state.DefId, out var def);
                    view = CreateView(_enemiesRoot, $"E_{key}", _prefabCatalog != null ? _prefabCatalog.GetEnemyPrefab(def) : null, PrimitiveType.Sphere);
                    _enemyViews[key] = view;
                }

                view.transform.position = _runtimeHost.Mapper.CellToWorldCenter(state.Cell) + _actorVisualOffset;
                view.transform.localScale = _prefabCatalog != null ? _prefabCatalog.actorScale : Vector3.one * 0.75f;
            }

            RemoveStale(_enemyViews, alive);
        }

        private void PositionFootprintView(Transform target, CellPos anchor, int sizeX, int sizeY, Vector3 extraOffset)
        {
            float avgHeight = _runtimeHost.Mapper.GetAverageHeightForFootprint(anchor, sizeX, sizeY);
            float cellSize = _runtimeHost.Mapper.CellSize;
            Vector3 pos = new(
                (anchor.X + sizeX * 0.5f) * cellSize,
                avgHeight,
                (anchor.Y + sizeY * 0.5f) * cellSize);

            target.position = pos + extraOffset;
        }

        private Vector3 ComputeBuildingScale(int sizeX, int sizeY)
        {
            float cellSize = _runtimeHost.Mapper.CellSize;
            Vector3 baseScale = _prefabCatalog != null ? _prefabCatalog.buildingScale : Vector3.one;
            return new Vector3(sizeX * cellSize, Mathf.Max(1f, baseScale.y), sizeY * cellSize);
        }

        private static GameObject CreateView(Transform parent, string name, GameObject prefab, PrimitiveType fallbackType)
        {
            GameObject go = prefab != null
                ? Instantiate(prefab, parent)
                : GameObject.CreatePrimitive(fallbackType);

            go.name = name;
            go.transform.SetParent(parent, true);
            return go;
        }

        private static void ApplyBuildSiteMaterial(GameObject go)
        {
            if (go == null)
                return;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == null)
                return;

            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial.color = new Color(1f, 0.6f, 0.1f, 0.65f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void RemoveUnconstructedBuildingViews(HashSet<int> constructedAlive)
        {
            List<int> stale = null;
            foreach (var kv in _buildingViews)
            {
                if (constructedAlive.Contains(kv.Key))
                    continue;
                stale ??= new List<int>();
                stale.Add(kv.Key);
            }

            if (stale == null)
                return;

            for (int i = 0; i < stale.Count; i++)
            {
                int key = stale[i];
                if (mapTryGetAndDestroy(_buildingViews, key)) { }
            }
        }

        private static bool mapTryGetAndDestroy(Dictionary<int, GameObject> map, int key)
        {
            if (!map.TryGetValue(key, out var go) || go == null)
            {
                map.Remove(key);
                return false;
            }

            Destroy(go);
            map.Remove(key);
            return true;
        }

        private static void RemoveStale(Dictionary<int, GameObject> map, HashSet<int> alive)
        {
            List<int> stale = null;
            foreach (var kv in map)
            {
                if (alive.Contains(kv.Key))
                    continue;
                stale ??= new List<int>();
                stale.Add(kv.Key);
            }

            if (stale == null)
                return;

            for (int i = 0; i < stale.Count; i++)
            {
                int key = stale[i];
                if (map.TryGetValue(key, out var go) && go != null)
                    Destroy(go);
                map.Remove(key);
            }
        }

        private void EnsureRoots()
        {
            _buildingsRoot = GetOrCreateChild("BuildingsRoot");
            _buildSitesRoot = GetOrCreateChild("BuildSitesRoot");
            _npcsRoot = GetOrCreateChild("NpcsRoot");
            _enemiesRoot = GetOrCreateChild("EnemiesRoot");
        }

        private Transform GetOrCreateChild(string name)
        {
            Transform t = transform.Find(name);
            if (t != null)
                return t;

            GameObject go = new(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }
    }
}
