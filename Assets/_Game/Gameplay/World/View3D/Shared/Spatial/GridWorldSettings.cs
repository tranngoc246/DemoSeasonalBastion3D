using UnityEngine;

namespace SeasonalBastion
{
    public sealed class GridWorldSettings
    {
        public GridWorldSettings(float cellSize, Vector3 origin, bool invertGridYOnWorldZ = true)
        {
            CellSize = Mathf.Max(0.0001f, cellSize);
            Origin = origin;
            InvertGridYOnWorldZ = invertGridYOnWorldZ;
        }

        public float CellSize { get; }
        public Vector3 Origin { get; }

        // Current project convention: larger grid Y maps toward smaller world Z.
        public bool InvertGridYOnWorldZ { get; }
    }
}
