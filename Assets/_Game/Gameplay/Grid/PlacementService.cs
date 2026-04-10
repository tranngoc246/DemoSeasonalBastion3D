using System;
using SeasonalBastion.Contracts;

namespace SeasonalBastion
{
    public sealed class PlacementService : IPlacementService
    {
        private readonly IGridMap _grid;
        private readonly IWorldState _world;
        private readonly IDataRegistry _data;
        private readonly IWorldIndex _index;
        private readonly IEventBus _bus;

        private RunStartRuntime _runStart;
        private IBuildOrderService _buildOrders;
        private TerrainGameplayBridge _terrainBridge;

        public PlacementService(IGridMap grid, IWorldState world, IDataRegistry data, IWorldIndex index, IEventBus bus)
        {
            _grid = grid;
            _world = world;
            _data = data;
            _index = index;
            _bus = bus;
        }

        public void BindBuildOrders(IBuildOrderService buildOrders)
        {
            _buildOrders = buildOrders;
        }

        public void BindRunStart(RunStartRuntime runStart)
        {
            _runStart = runStart;
        }

        public void BindTerrainBridge(TerrainGameplayBridge terrainBridge)
        {
            _terrainBridge = terrainBridge;
        }

        public bool CanPlaceRoad(CellPos c)
        {
            if (!_grid.IsInside(c)) return false;
            if (!IsInBuildable(c)) return false;
            if (_grid.IsBlocked(c)) return false;
            if (_grid.IsRoad(c)) return false;
            if (!HasAnyRoad()) return true;
            return HasRoadInCross(c);
        }

        public void PlaceRoad(CellPos c)
        {
            if (!CanPlaceRoad(c)) return;
            _grid.SetRoad(c, true);
            _bus.Publish(new RoadPlacedEvent(c));
            _bus.Publish(new RoadsDirtyEvent());
        }

        public bool CanRemoveRoad(CellPos c)
        {
            return _grid.IsInside(c) && _grid.IsRoad(c);
        }

        public void RemoveRoad(CellPos c)
        {
            if (!CanRemoveRoad(c)) return;
            _grid.SetRoad(c, false);
            _bus.Publish(new RoadsDirtyEvent());
        }

        public PlacementResult ValidateBuilding(string buildingDefId, CellPos anchor, Dir4 rotation)
        {
            if (rotation != Dir4.N && rotation != Dir4.E && rotation != Dir4.S && rotation != Dir4.W)
                return new PlacementResult(false, PlacementFailReason.InvalidRotation, anchor);

            if (!_data.TryGetBuilding(buildingDefId, out var def) || def == null)
                return new PlacementResult(false, PlacementFailReason.Unknown, anchor);

            GetFootprintSize(def, rotation, out int w, out int h);
            CellPos entry = ComputeEntryCell(anchor, w, h, rotation);

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    CellPos c = new(anchor.X + dx, anchor.Y + dy);
                    if (!_grid.IsInside(c)) return new PlacementResult(false, PlacementFailReason.OutOfBounds, entry);
                    if (!IsInBuildable(c)) return new PlacementResult(false, PlacementFailReason.OutOfBounds, entry);
                    if (!IsTerrainBuildable(c)) return new PlacementResult(false, PlacementFailReason.OutOfBounds, entry);
                    if (_grid.IsRoad(c)) return new PlacementResult(false, PlacementFailReason.Overlap, entry);

                    CellOccupancy occ = _grid.Get(c);
                    if (occ.Kind == CellOccupancyKind.Site) return new PlacementResult(false, PlacementFailReason.BlockedBySite, entry);
                    if (occ.Kind == CellOccupancyKind.Building) return new PlacementResult(false, PlacementFailReason.Overlap, entry);
                }
            }

            if (!IsInBuildable(entry)) return new PlacementResult(false, PlacementFailReason.OutOfBounds, entry);
            if (!_grid.IsInside(entry)) return new PlacementResult(false, PlacementFailReason.NoRoadConnection, entry);
            if (!IsTerrainTraversable(entry)) return new PlacementResult(false, PlacementFailReason.NoRoadConnection, entry);
            if (_grid.IsRoad(entry)) return new PlacementResult(true, PlacementFailReason.None, entry);

            CellOccupancy entryOcc = _grid.Get(entry);
            if (entryOcc.Kind == CellOccupancyKind.Site) return new PlacementResult(false, PlacementFailReason.BlockedBySite, entry);
            if (entryOcc.Kind == CellOccupancyKind.Building) return new PlacementResult(false, PlacementFailReason.Overlap, entry);
            if (entryOcc.Kind != CellOccupancyKind.Empty) return new PlacementResult(false, PlacementFailReason.Overlap, entry);
            if (!HasRoadInCross(entry)) return new PlacementResult(false, PlacementFailReason.NoRoadConnection, entry);

            return new PlacementResult(true, PlacementFailReason.None, entry);
        }

        public BuildingId CommitBuilding(string buildingDefId, CellPos anchor, Dir4 rotation)
        {
            PlacementResult vr = ValidateBuilding(buildingDefId, anchor, rotation);
            if (!vr.Ok) return default;
            if (_buildOrders == null) return default;

            int orderId = _buildOrders.CreatePlaceOrder(buildingDefId, anchor, rotation);
            if (orderId <= 0) return default;
            if (!_buildOrders.TryGet(orderId, out var order) || order.TargetBuilding.Value == 0) return default;

            CellPos driveway = vr.SuggestedRoadCell;
            bool drivewayWasCreated = false;
            if (_grid.IsInside(driveway) && !_grid.IsRoad(driveway))
            {
                CellOccupancy occ = _grid.Get(driveway);
                if (occ.Kind == CellOccupancyKind.Empty)
                {
                    _grid.SetRoad(driveway, true);
                    _bus.Publish(new RoadPlacedEvent(driveway));
                    _bus.Publish(new RoadsDirtyEvent());
                    drivewayWasCreated = true;
                }
            }

            if (drivewayWasCreated)
                _bus.Publish(new BuildOrderAutoRoadCreatedEvent(orderId, driveway));

            return order.TargetBuilding;
        }

        private static CellPos Add(CellPos a, int dx, int dy) => new(a.X + dx, a.Y + dy);

        private static void GetFootprintSize(BuildingDef def, Dir4 rotation, out int w, out int h)
        {
            int sizeX = Math.Max(1, def.SizeX);
            int sizeY = Math.Max(1, def.SizeY);
            bool swap = rotation == Dir4.E || rotation == Dir4.W;
            w = swap ? sizeY : sizeX;
            h = swap ? sizeX : sizeY;
        }

        private CellPos ComputeEntryCell(CellPos anchor, int w, int h, Dir4 rot)
        {
            int cx = (w - 1) / 2;
            int cy = (h - 1) / 2;
            return rot switch
            {
                Dir4.N => new CellPos(anchor.X + cx, anchor.Y + h),
                Dir4.S => new CellPos(anchor.X + cx, anchor.Y - 1),
                Dir4.E => new CellPos(anchor.X + w, anchor.Y + cy),
                Dir4.W => new CellPos(anchor.X - 1, anchor.Y + cy),
                _ => new CellPos(anchor.X + cx, anchor.Y + h),
            };
        }

        private bool HasBuildableRect()
        {
            if (_runStart == null) return false;
            return _runStart.BuildableRect.XMax != 0 || _runStart.BuildableRect.YMax != 0;
        }

        private bool IsInBuildable(CellPos c)
        {
            return !HasBuildableRect() || _runStart.BuildableRect.Contains(c);
        }

        private bool IsTerrainBuildable(CellPos c)
        {
            return _terrainBridge == null || _terrainBridge.IsBuildable(c);
        }

        private bool IsTerrainTraversable(CellPos c)
        {
            if (_terrainBridge == null)
                return true;

            return _terrainBridge.IsBuildable(c) && !_terrainBridge.IsWater(c);
        }

        private bool HasAnyRoad()
        {
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                    if (_grid.IsRoad(new CellPos(x, y)))
                        return true;
            return false;
        }

        private bool HasRoadInCross(CellPos entry)
        {
            CellPos n = Add(entry, 0, 1);
            CellPos e = Add(entry, 1, 0);
            CellPos s = Add(entry, 0, -1);
            CellPos w = Add(entry, -1, 0);
            return (_grid.IsInside(n) && _grid.IsRoad(n))
                || (_grid.IsInside(e) && _grid.IsRoad(e))
                || (_grid.IsInside(s) && _grid.IsRoad(s))
                || (_grid.IsInside(w) && _grid.IsRoad(w));
        }
    }
}
