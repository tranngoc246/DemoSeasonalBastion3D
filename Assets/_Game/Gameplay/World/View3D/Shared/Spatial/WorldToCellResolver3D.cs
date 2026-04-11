using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class WorldToCellResolver3D
    {
        private readonly GroundRaycastService _groundRaycast;
        private readonly CellWorldMapper3D _mapper;

        public WorldToCellResolver3D(GroundRaycastService groundRaycast, CellWorldMapper3D mapper)
        {
            _groundRaycast = groundRaycast;
            _mapper = mapper;
        }

        public bool TryResolveFromScreen(Camera camera, Vector2 screenPosition, out CellPos cell, out RaycastHit hit)
        {
            cell = default;
            hit = default;

            if (_groundRaycast == null || _mapper == null)
                return false;

            if (!_groundRaycast.TryRaycast(camera, screenPosition, out hit))
                return false;

            if (!_mapper.TryWorldToCell(hit.point, out cell))
                return false;

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
