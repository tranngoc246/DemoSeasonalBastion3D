using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class WorldToCellResolver3D
    {
        public readonly struct ResolutionDebugInfo
        {
            public ResolutionDebugInfo(Vector3 hitPoint, Vector3 flatCellCenter, Vector3 terrainCellCenter, Vector3 hitToFlatDelta, Vector3 hitToTerrainDelta)
            {
                HitPoint = hitPoint;
                FlatCellCenter = flatCellCenter;
                TerrainCellCenter = terrainCellCenter;
                HitToFlatDelta = hitToFlatDelta;
                HitToTerrainDelta = hitToTerrainDelta;
            }

            public Vector3 HitPoint { get; }
            public Vector3 FlatCellCenter { get; }
            public Vector3 TerrainCellCenter { get; }
            public Vector3 HitToFlatDelta { get; }
            public Vector3 HitToTerrainDelta { get; }
        }

        private readonly GroundRaycastService _groundRaycast;
        private readonly CellWorldMapper3D _mapper;

        public WorldToCellResolver3D(GroundRaycastService groundRaycast, CellWorldMapper3D mapper)
        {
            _groundRaycast = groundRaycast;
            _mapper = mapper;
        }

        public bool TryResolveFromScreen(Camera camera, Vector2 screenPosition, out CellPos cell, out RaycastHit hit)
        {
            return TryResolveFromScreen(camera, screenPosition, out cell, out hit, out _);
        }

        public bool TryResolveFromScreen(Camera camera, Vector2 screenPosition, out CellPos cell, out RaycastHit hit, out ResolutionDebugInfo debugInfo)
        {
            cell = default;
            hit = default;
            debugInfo = default;

            if (_groundRaycast == null || _mapper == null)
                return false;

            if (!_groundRaycast.TryRaycast(camera, screenPosition, out hit, out _))
                return false;

            if (!_mapper.TryWorldToCell(hit.point, out cell))
                return false;

            Vector3 flatCenter = _mapper.CellToWorldCenterFlat(cell);
            Vector3 terrainCenter = _mapper.CellToWorldCenter(cell);
            debugInfo = new ResolutionDebugInfo(
                hit.point,
                flatCenter,
                terrainCenter,
                hit.point - flatCenter,
                hit.point - terrainCenter);

            return true;
        }

        public bool TryResolveWorld(Vector3 worldPosition, out CellPos cell)
        {
            cell = default;
            if (_mapper == null)
                return false;

            return _mapper.TryWorldToCell(worldPosition, out cell);
        }
    }
}
