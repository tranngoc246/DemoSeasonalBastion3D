using UnityEngine;

namespace SeasonalBastion.WorldGen.Runtime.Models
{
    public readonly struct StartAreaDefinition
    {
        public StartAreaDefinition(bool flattenEnabled, Vector2 center, float radius, float targetHeight)
        {
            FlattenEnabled = flattenEnabled;
            Center = center;
            Radius = radius;
            TargetHeight = targetHeight;
        }

        public bool FlattenEnabled { get; }
        public Vector2 Center { get; }
        public float Radius { get; }
        public float TargetHeight { get; }
    }
}
