using UnityEngine;

namespace SeasonalBastion.WorldGen.Runtime.Models
{
    [System.Serializable]
    public class NoiseSettings
    {
        public NormalizeMode normalizeMode = NormalizeMode.Local;

        [Min(0.01f)]
        public float scale = 50f;

        [Min(1)]
        public int octaves = 6;

        [Range(0f, 1f)]
        public float persistence = 0.6f;

        [Min(1f)]
        public float lacunarity = 2f;

        public int seed;
        public Vector2 offset;

        public void ValidateValues()
        {
            scale = Mathf.Max(0.01f, scale);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(1f, lacunarity);
            persistence = Mathf.Clamp01(persistence);
        }
    }

    public enum NormalizeMode
    {
        Local,
        Global
    }
}
