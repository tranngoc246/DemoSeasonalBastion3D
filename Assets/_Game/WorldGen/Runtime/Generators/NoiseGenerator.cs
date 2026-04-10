using UnityEngine;
using SeasonalBastion.WorldGen.Runtime.Models;

namespace SeasonalBastion.WorldGen.Runtime.Generators
{
    public static class NoiseGenerator
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new(settings.seed);
            Vector2[] octaveOffsets = new Vector2[settings.octaves];

            float maxPossibleHeight = 0f;
            float amplitude = 1f;
            float frequency = 1f;

            for (int i = 0; i < settings.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
                float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= settings.persistence;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;
            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    amplitude = 1f;
                    frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < settings.octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.persistence;
                        frequency *= settings.lacunarity;
                    }

                    maxLocalNoiseHeight = Mathf.Max(maxLocalNoiseHeight, noiseHeight);
                    minLocalNoiseHeight = Mathf.Min(minLocalNoiseHeight, noiseHeight);
                    noiseMap[x, y] = noiseHeight;

                    if (settings.normalizeMode == NormalizeMode.Global)
                    {
                        float normalizedHeight = (noiseMap[x, y] + 1f) / Mathf.Max(0.0001f, maxPossibleHeight / 0.9f);
                        noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue);
                    }
                }
            }

            if (settings.normalizeMode == NormalizeMode.Local)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                }
            }

            return noiseMap;
        }
    }
}
