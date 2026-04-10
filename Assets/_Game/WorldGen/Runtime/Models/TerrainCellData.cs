using UnityEngine;

namespace SeasonalBastion.WorldGen.Runtime.Models
{
    public readonly struct TerrainCellData
    {
        public TerrainCellData(Vector2Int gridPosition, float height, float slopeDegrees, bool isWater, bool isBuildable, TerrainType terrainType)
        {
            GridPosition = gridPosition;
            Height = height;
            SlopeDegrees = slopeDegrees;
            IsWater = isWater;
            IsBuildable = isBuildable;
            TerrainType = terrainType;
        }

        public Vector2Int GridPosition { get; }
        public float Height { get; }
        public float SlopeDegrees { get; }
        public bool IsWater { get; }
        public bool IsBuildable { get; }
        public TerrainType TerrainType { get; }
    }
}
