namespace SeasonalBastion.WorldGen.Runtime.Models
{
    public readonly struct HeightMapData
    {
        public HeightMapData(float[,] values, float minValue, float maxValue)
        {
            Values = values;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public float[,] Values { get; }
        public float MinValue { get; }
        public float MaxValue { get; }
    }
}
