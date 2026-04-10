using UnityEngine;

namespace SeasonalBastion.WorldGen.Runtime.Generators
{
    public static class FalloffMapGenerator
    {
        public static float[,] GenerateFalloffMap(int size)
        {
            float[,] map = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float normalizedX = x / (float)size * 2f - 1f;
                    float normalizedY = y / (float)size * 2f - 1f;

                    float value = Mathf.Max(Mathf.Abs(normalizedX), Mathf.Abs(normalizedY));
                    map[x, y] = Evaluate(value);
                }
            }

            return map;
        }

        private static float Evaluate(float value)
        {
            const float a = 3f;
            const float b = 2.2f;
            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - (b * value), a));
        }
    }
}
