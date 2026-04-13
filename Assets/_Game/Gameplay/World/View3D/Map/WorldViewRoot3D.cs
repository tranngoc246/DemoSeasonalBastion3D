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
        [SerializeField] private BuildingPrefabRegistry3D _buildingPrefabRegistry;

        [Header("View options")]
        [SerializeField] private Vector3 _buildingVisualOffset = Vector3.zero;
        [SerializeField] private Vector3 _buildSiteVisualOffset = Vector3.up * 0.05f;
        [SerializeField] private Vector3 _actorVisualOffset = Vector3.up * 0.25f;

        private readonly Dictionary<int, BuildingView3D> _buildingViews = new();
        private readonly Dictionary<int, BuildingView3D> _buildSiteViews = new();
        private readonly Dictionary<int, GameObject> _npcViews = new();
        private readonly Dictionary<int, GameObject> _enemyViews = new();

        private Transform _buildingsRoot;
        private Transform _buildSitesRoot;
        private Transform _npcsRoot;
        private Transform _enemiesRoot;

        private WorldState _world;
        private DataRegistry _data;
        private BuildingViewFactory3D _buildingViewFactory;

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

            EnsureFactories();
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
            EnsureFactories();
        }

        private void EnsureFactories()
        {
            if (_runtimeHost?.Mapper == null)
                return;

            if (_buildingViewFactory != null)
                return;

            Vector3 buildingScale = _prefabCatalog != null ? _prefabCatalog.buildingScale : Vector3.one;
            Vector3 buildSiteScale = new(0.75f, 0.2f, 0.75f);
            _buildingViewFactory = new BuildingViewFactory3D(
                _runtimeHost.Mapper,
                _buildingPrefabRegistry,
                _buildingVisualOffset,
                _buildSiteVisualOffset,
                buildingScale,
                buildSiteScale);
        }

        private void SyncBuildSites()
        {
            _buildingViewFactory?.SyncBuildSites(_world, _data, _buildSitesRoot, _buildSiteViews);
        }

        private void SyncBuildings()
        {
            _buildingViewFactory?.SyncBuildings(_world, _data, _buildingsRoot, _buildingViews);
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

        private static GameObject CreateView(Transform parent, string name, GameObject prefab, PrimitiveType fallbackType)
        {
            GameObject go = prefab != null
                ? Instantiate(prefab, parent)
                : GameObject.CreatePrimitive(fallbackType);

            go.name = name;
            go.transform.SetParent(parent, true);
            return go;
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
