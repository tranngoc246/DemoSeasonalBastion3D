using System.Collections.Generic;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class BuildingViewFactory3D
    {
        private readonly CellWorldMapper3D _mapper;
        private readonly BuildingPrefabRegistry3D _registry;
        private readonly Vector3 _buildingVisualOffset;
        private readonly Vector3 _buildSiteVisualOffset;
        private readonly Vector3 _buildingScale;
        private readonly Vector3 _buildSiteScale;

        public BuildingViewFactory3D(
            CellWorldMapper3D mapper,
            BuildingPrefabRegistry3D registry,
            Vector3 buildingVisualOffset,
            Vector3 buildSiteVisualOffset,
            Vector3 buildingScale,
            Vector3 buildSiteScale)
        {
            _mapper = mapper;
            _registry = registry;
            _buildingVisualOffset = buildingVisualOffset;
            _buildSiteVisualOffset = buildSiteVisualOffset;
            _buildingScale = buildingScale;
            _buildSiteScale = buildSiteScale;
        }

        public void SyncBuildSites(WorldState world, DataRegistry data, Transform root, Dictionary<int, BuildingView3D> views)
        {
            HashSet<int> alive = new();
            foreach (var id in world.Sites.Ids)
            {
                int key = id.Value;
                alive.Add(key);
                BuildSiteState state = world.Sites.Get(id);
                data.TryGetBuilding(state.BuildingDefId, out var def);

                BuildingView3D view = GetOrCreateView(views, key, root, def, true);
                view.BindBuildSite(key, _mapper, def, state, _buildSiteVisualOffset, _buildSiteScale);
            }

            RemoveStale(views, alive);
        }

        public void SyncBuildings(WorldState world, DataRegistry data, Transform root, Dictionary<int, BuildingView3D> views)
        {
            HashSet<int> alive = new();
            foreach (var id in world.Buildings.Ids)
            {
                int key = id.Value;
                BuildingState state = world.Buildings.Get(id);
                if (!state.IsConstructed)
                    continue;

                alive.Add(key);
                data.TryGetBuilding(state.DefId, out var def);
                BuildingView3D view = GetOrCreateView(views, key, root, def, false);
                view.BindBuilding(key, _mapper, def, state, _buildingVisualOffset, _buildingScale);
            }

            RemoveStale(views, alive);
        }

        private BuildingView3D GetOrCreateView(Dictionary<int, BuildingView3D> views, int key, Transform root, BuildingDef def, bool isBuildSite)
        {
            if (views.TryGetValue(key, out var existing) && existing != null)
                return existing;

            GameObject prefab = isBuildSite ? null : _registry != null ? _registry.GetPrefab(def) : null;
            GameObject go = prefab != null
                ? Object.Instantiate(prefab, root)
                : GameObject.CreatePrimitive(isBuildSite ? PrimitiveType.Cylinder : PrimitiveType.Cube);

            go.transform.SetParent(root, true);

            BuildingView3D view = go.GetComponent<BuildingView3D>();
            if (view == null)
                view = go.AddComponent<BuildingView3D>();

            if (go.GetComponent<ConstructionVisualController3D>() == null)
                go.AddComponent<ConstructionVisualController3D>();

            views[key] = view;
            return view;
        }

        private static void RemoveStale(Dictionary<int, BuildingView3D> views, HashSet<int> alive)
        {
            List<int> stale = null;
            foreach (var kv in views)
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
                if (views.TryGetValue(key, out var view) && view != null)
                    Object.Destroy(view.gameObject);
                views.Remove(key);
            }
        }
    }
}
