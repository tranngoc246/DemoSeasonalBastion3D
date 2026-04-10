using UnityEngine;
using SeasonalBastion.WorldGen.Authoring.ScriptableObjects;

namespace SeasonalBastion.WorldGen.Runtime.Models
{
    public readonly struct WorldGenerationRequest
    {
        public WorldGenerationRequest(int seed, Vector2 sampleCenter, int width, int height, WorldHeightSettings heightSettings, WorldMeshSettings meshSettings)
        {
            Seed = seed;
            SampleCenter = sampleCenter;
            Width = width;
            Height = height;
            HeightSettings = heightSettings;
            MeshSettings = meshSettings;
        }

        public int Seed { get; }
        public Vector2 SampleCenter { get; }
        public int Width { get; }
        public int Height { get; }
        public WorldHeightSettings HeightSettings { get; }
        public WorldMeshSettings MeshSettings { get; }
    }
}
