using UnityEngine;
using SeasonalBastion.Core.Data;

namespace SeasonalBastion.WorldGen.Authoring.ScriptableObjects
{
    [CreateAssetMenu(fileName = "WorldMeshSettings", menuName = "SeasonalBastion/WorldGen/World Mesh Settings")]
    public sealed class WorldMeshSettings : UpdatableData
    {
        public const int NumSupportedLods = 5;
        public const int NumSupportedChunkSizes = 9;
        public const int NumSupportedFlatShadedChunkSizes = 3;
        public static readonly int[] SupportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

        public float meshScale = 2.5f;
        public bool useFlatShading;

        [Range(0, NumSupportedChunkSizes - 1)]
        public int chunkSizeIndex;

        [Range(0, NumSupportedFlatShadedChunkSizes - 1)]
        public int flatShadedChunkSizeIndex;

        public int NumVertsPerLine => SupportedChunkSizes[useFlatShading ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;
        public float MeshWorldSize => (NumVertsPerLine - 3) * meshScale;
    }
}
