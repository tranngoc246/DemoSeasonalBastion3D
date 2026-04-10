using UnityEngine;
using SeasonalBastion.WorldGen.Authoring.ScriptableObjects;
using SeasonalBastion.WorldGen.Runtime.Models;

namespace SeasonalBastion.WorldGen.Runtime.Generators
{
    public static class HeightMapGenerator
    {
        public static HeightMapData GenerateHeightMap(int width, int height, WorldHeightSettings settings, Vector2 sampleCentre)
        {
            float[,] values = NoiseGenerator.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

            if (settings.useFalloff)
            {
                float[,] falloffMap = FalloffMapGenerator.GenerateFalloffMap(width);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        values[x, y] = Mathf.Clamp01(values[x, y] - falloffMap[x, y]);
                    }
                }
            }

            AnimationCurve heightCurveThreadSafe = new(settings.heightCurve.keys);

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    values[x, y] *= heightCurveThreadSafe.Evaluate(values[x, y]) * settings.heightMultiplier;
                    minValue = Mathf.Min(minValue, values[x, y]);
                    maxValue = Mathf.Max(maxValue, values[x, y]);
                }
            }

            return new HeightMapData(values, minValue, maxValue);
        }
    }
}
