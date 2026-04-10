using UnityEngine;

namespace SeasonalBastion.WorldGen.Runtime.Models
{
    public sealed class WorldGenerationResult
    {
        public WorldGenerationResult(
            float[,] heightMap,
            bool[,] waterMap,
            float[,] slopeMap,
            bool[,] buildableMap,
            TerrainType[,] terrainTypes,
            TerrainCellData[,] cells,
            StartAreaDefinition startArea,
            float minHeight,
            float maxHeight)
        {
            HeightMap = heightMap;
            WaterMap = waterMap;
            SlopeMap = slopeMap;
            BuildableMap = buildableMap;
            TerrainTypes = terrainTypes;
            Cells = cells;
            StartArea = startArea;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
        }

        public float[,] HeightMap { get; }
        public bool[,] WaterMap { get; }
        public float[,] SlopeMap { get; }
        public bool[,] BuildableMap { get; }
        public TerrainType[,] TerrainTypes { get; }
        public TerrainCellData[,] Cells { get; }
        public StartAreaDefinition StartArea { get; }
        public float MinHeight { get; }
        public float MaxHeight { get; }

        public int Width => HeightMap?.GetLength(0) ?? 0;
        public int Height => HeightMap?.GetLength(1) ?? 0;
    }
}
