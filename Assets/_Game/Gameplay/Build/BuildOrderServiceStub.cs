using System;
using System.Collections.Generic;
using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class BuildOrderServiceStub : IBuildOrderService
    {
        private readonly IWorldState _world;
        private readonly IGridMap _grid;
        private readonly IDataRegistry _data;
        private readonly IEventBus _bus;
        private readonly IWorldIndex _index;
        private readonly Dictionary<int, BuildOrder> _orders = new();
        private int _nextOrderId = 1;

        public BuildOrderServiceStub(IWorldState world, IGridMap grid, IDataRegistry data, IEventBus bus = null, IWorldIndex index = null)
        {
            _world = world;
            _grid = grid;
            _data = data;
            _bus = bus;
            _index = index;
        }

        public event Action<int> OnOrderCompleted;

        public int CreatePlaceOrder(string buildingDefId, CellPos anchor, Dir4 rotation)
        {
            if (!_data.TryGetBuilding(buildingDefId, out var def) || def == null)
                return 0;

            BuildingState bst = new()
            {
                DefId = buildingDefId,
                Anchor = anchor,
                Rotation = rotation,
                Level = Math.Max(1, def.BaseLevel),
                IsConstructed = false,
                MaxHP = Math.Max(1, def.MaxHp),
                HP = Math.Max(1, def.MaxHp)
            };

            BuildingId buildingId = _world.Buildings.Create(bst);
            bst.Id = buildingId;
            _world.Buildings.Set(buildingId, bst);

            BuildSiteState site = new()
            {
                BuildingDefId = buildingDefId,
                TargetLevel = bst.Level,
                Anchor = anchor,
                Rotation = rotation,
                IsActive = true,
                WorkSecondsDone = 0f,
                WorkSecondsTotal = Math.Max(0.1f, def.BuildChunksL1 <= 0 ? 1f : def.BuildChunksL1),
                Kind = (byte)BuildOrderKind.PlaceNew,
                TargetBuilding = buildingId,
                RemainingCosts = def.BuildCostsL1 != null ? new List<CostDef>(def.BuildCostsL1) : new List<CostDef>(),
                DeliveredSoFar = new List<CostDef>()
            };

            SiteId siteId = _world.Sites.Create(site);
            site.Id = siteId;
            _world.Sites.Set(siteId, site);
            _bus?.Publish(new BuildSitePlacedEvent(buildingDefId, siteId));
            _bus?.Publish(new WorldStateChangedEvent("BuildSite", siteId.Value));

            GetFootprintSize(def, rotation, out int w, out int h);
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    _grid.SetSite(new CellPos(anchor.X + dx, anchor.Y + dy), siteId);

            int orderId = _nextOrderId++;
            BuildOrder order = new()
            {
                OrderId = orderId,
                Kind = BuildOrderKind.PlaceNew,
                BuildingDefId = buildingDefId,
                TargetBuilding = buildingId,
                Site = siteId,
                RequiredCost = def.BuildCostsL1,
                Delivered = Array.Empty<CostDef>(),
                WorkSecondsRequired = site.WorkSecondsTotal,
                WorkSecondsDone = 0f,
                Completed = false
            };

            _orders[orderId] = order;
            return orderId;
        }

        public int CreateUpgradeOrder(BuildingId building)
        {
            if (!_world.Buildings.Exists(building))
                return 0;

            BuildingState existing = _world.Buildings.Get(building);
            if (!_data.TryGetBuilding(existing.DefId, out var def) || def == null)
                return 0;

            existing.Level = Math.Max(existing.Level + 1, existing.Level + 1);
            existing.IsConstructed = false;
            _world.Buildings.Set(building, existing);

            BuildSiteState site = new()
            {
                BuildingDefId = existing.DefId,
                TargetLevel = existing.Level,
                Anchor = existing.Anchor,
                Rotation = existing.Rotation,
                IsActive = true,
                WorkSecondsDone = 0f,
                WorkSecondsTotal = Math.Max(0.1f, def.BuildChunksL1 <= 0 ? 1f : def.BuildChunksL1),
                Kind = (byte)BuildOrderKind.Upgrade,
                TargetBuilding = building,
                FromDefId = existing.DefId,
                RemainingCosts = def.BuildCostsL1 != null ? new List<CostDef>(def.BuildCostsL1) : new List<CostDef>(),
                DeliveredSoFar = new List<CostDef>()
            };

            SiteId siteId = _world.Sites.Create(site);
            site.Id = siteId;
            _world.Sites.Set(siteId, site);
            _bus?.Publish(new BuildSitePlacedEvent(existing.DefId, siteId));
            _bus?.Publish(new WorldStateChangedEvent("BuildSite", siteId.Value));
            _bus?.Publish(new WorldStateChangedEvent("Building", building.Value));

            int orderId = _nextOrderId++;
            BuildOrder order = new()
            {
                OrderId = orderId,
                Kind = BuildOrderKind.Upgrade,
                BuildingDefId = existing.DefId,
                TargetBuilding = building,
                Site = siteId,
                RequiredCost = def.BuildCostsL1,
                Delivered = Array.Empty<CostDef>(),
                WorkSecondsRequired = site.WorkSecondsTotal,
                WorkSecondsDone = 0f,
                Completed = false
            };

            _orders[orderId] = order;
            return orderId;
        }
        public int CreateRepairOrder(BuildingId building) => 0;

        public bool TryGet(int orderId, out BuildOrder order) => _orders.TryGetValue(orderId, out order);

        public void Cancel(int orderId)
        {
            if (!_orders.TryGetValue(orderId, out var order))
                return;

            CleanupOrder(order);
            _orders.Remove(orderId);
        }

        public bool CancelBySite(SiteId siteId)
        {
            int orderId = FindOrderIdBySite(siteId);
            if (orderId <= 0)
                return false;

            Cancel(orderId);
            return true;
        }

        public bool CancelByBuilding(BuildingId buildingId)
        {
            int orderId = FindOrderIdByBuilding(buildingId);
            if (orderId <= 0)
                return false;

            Cancel(orderId);
            return true;
        }

        public void ClearAll()
        {
            foreach (var kv in _orders)
                CleanupOrder(kv.Value);
            _orders.Clear();
        }

        public void Tick(float dt)
        {
            if (_orders.Count == 0 || dt <= 0f)
                return;

            List<int> orderIds = new(_orders.Keys);
            List<int> completed = null;
            for (int i = 0; i < orderIds.Count; i++)
            {
                int orderId = orderIds[i];
                if (!_orders.TryGetValue(orderId, out BuildOrder order))
                    continue;

                if (order.Completed || order.Site.Value == 0 || order.TargetBuilding.Value == 0)
                    continue;
                if (!_world.Sites.Exists(order.Site) || !_world.Buildings.Exists(order.TargetBuilding))
                    continue;

                BuildSiteState site = _world.Sites.Get(order.Site);
                if (!site.IsActive)
                    continue;

                float required = Math.Max(0.1f, order.WorkSecondsRequired);
                order.WorkSecondsDone = Math.Min(required, order.WorkSecondsDone + dt);
                site.WorkSecondsDone = order.WorkSecondsDone;
                _world.Sites.Set(order.Site, site);

                if (order.WorkSecondsDone >= required)
                {
                    FinalizeOrder(order, site);
                    order.Completed = true;
                    completed ??= new List<int>();
                    completed.Add(orderId);
                }
                else
                {
                    _orders[orderId] = order;
                }
            }

            if (completed == null)
                return;

            for (int i = 0; i < completed.Count; i++)
            {
                int orderId = completed[i];
                _orders.Remove(orderId);
                OnOrderCompleted?.Invoke(orderId);
            }
        }

        private void FinalizeOrder(BuildOrder order, BuildSiteState site)
        {
            if (!_world.Buildings.Exists(order.TargetBuilding))
                return;

            BuildingState building = _world.Buildings.Get(order.TargetBuilding);
            building.IsConstructed = true;
            building.HP = Math.Max(1, building.MaxHP);
            _world.Buildings.Set(order.TargetBuilding, building);

            if (_data.TryGetBuilding(building.DefId, out var def) && def != null)
            {
                GetFootprintSize(def, building.Rotation, out int w, out int h);
                for (int dy = 0; dy < h; dy++)
                {
                    for (int dx = 0; dx < w; dx++)
                    {
                        CellPos c = new(building.Anchor.X + dx, building.Anchor.Y + dy);
                        _grid.ClearSite(c);
                        _grid.SetBuilding(c, building.Id);
                    }
                }
            }

            if (_world.Sites.Exists(site.Id))
                _world.Sites.Destroy(site.Id);

            if (site.IsUpgrade)
                _index?.OnBuildingDestroyed(building.Id);
            _index?.OnBuildingCreated(building.Id);
            _bus?.Publish(new BuildSiteCompletedEvent(building.DefId, site.Id, building.Id));
            _bus?.Publish(new BuildingPlacedEvent(building.DefId, building.Id));
            _bus?.Publish(new WorldStateChangedEvent("Building", building.Id.Value));
            _bus?.Publish(new WorldStateChangedEvent("BuildSite", site.Id.Value));
            _bus?.Publish(new RoadsDirtyEvent());
        }

        private void CleanupOrder(BuildOrder order)
        {
            if (_world.Sites.Exists(order.Site))
            {
                BuildSiteState site = _world.Sites.Get(order.Site);
                if (_data.TryGetBuilding(site.BuildingDefId, out var def) && def != null)
                {
                    GetFootprintSize(def, site.Rotation, out int w, out int h);
                    for (int dy = 0; dy < h; dy++)
                        for (int dx = 0; dx < w; dx++)
                            _grid.ClearSite(new CellPos(site.Anchor.X + dx, site.Anchor.Y + dy));
                }
                _world.Sites.Destroy(order.Site);
            }

            if (_world.Buildings.Exists(order.TargetBuilding))
            {
                BuildingState building = _world.Buildings.Get(order.TargetBuilding);
                if (_data.TryGetBuilding(building.DefId, out var buildingDef) && buildingDef != null)
                {
                    GetFootprintSize(buildingDef, building.Rotation, out int w, out int h);
                    for (int dy = 0; dy < h; dy++)
                        for (int dx = 0; dx < w; dx++)
                            _grid.ClearBuilding(new CellPos(building.Anchor.X + dx, building.Anchor.Y + dy));
                }
                _world.Buildings.Destroy(order.TargetBuilding);
            }
        }

        private static void GetFootprintSize(BuildingDef def, Dir4 rotation, out int w, out int h)
        {
            int sizeX = Math.Max(1, def.SizeX);
            int sizeY = Math.Max(1, def.SizeY);
            bool swap = rotation == Dir4.E || rotation == Dir4.W;
            w = swap ? sizeY : sizeX;
            h = swap ? sizeX : sizeY;
        }

        private int FindOrderIdBySite(SiteId siteId)
        {
            foreach (var kv in _orders)
                if (kv.Value.Site.Value == siteId.Value)
                    return kv.Key;
            return 0;
        }

        private int FindOrderIdByBuilding(BuildingId buildingId)
        {
            foreach (var kv in _orders)
                if (kv.Value.TargetBuilding.Value == buildingId.Value)
                    return kv.Key;
            return 0;
        }
    }
}
