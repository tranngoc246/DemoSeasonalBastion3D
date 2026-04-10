using SeasonalBastion.Contracts;
using SeasonalBastion.WorldGen.Runtime.Models;

namespace SeasonalBastion
{
    public sealed class TerrainGameplayBridge
    {
        private readonly WorldGenerationResult _world;
        private readonly IGridMap _grid;

        public TerrainGameplayBridge(WorldGenerationResult world, IGridMap grid)
        {
            _world = world;
            _grid = grid;
        }

        public int Width => _world?.Width ?? 0;
        public int Height => _world?.Height ?? 0;

        public bool CanInitialize => _world != null && _grid != null && _world.Width == _grid.Width && _world.Height == _grid.Height;

        public void ApplyEmptyGameplayGridFromTerrain()
        {
            if (!CanInitialize)
                return;

            _grid.ClearAll();
        }

        public bool IsBuildable(CellPos cell)
        {
            if (!IsInside(cell) || _world.BuildableMap == null)
                return false;

            return _world.BuildableMap[cell.X, cell.Y];
        }

        public bool IsWater(CellPos cell)
        {
            if (!IsInside(cell) || _world.WaterMap == null)
                return false;

            return _world.WaterMap[cell.X, cell.Y];
        }

        public float GetSlope(CellPos cell)
        {
            if (!IsInside(cell) || _world.SlopeMap == null)
                return 0f;

            return _world.SlopeMap[cell.X, cell.Y];
        }

        public TerrainType GetTerrainType(CellPos cell)
        {
            if (!IsInside(cell) || _world.TerrainTypes == null)
                return TerrainType.Grass;

            return _world.TerrainTypes[cell.X, cell.Y];
        }

        public StartAreaDefinition GetStartArea()
        {
            return _world != null ? _world.StartArea : default;
        }

        public bool IsInside(CellPos cell)
        {
            return _world != null && cell.X >= 0 && cell.Y >= 0 && cell.X < _world.Width && cell.Y < _world.Height;
        }
    }
}
