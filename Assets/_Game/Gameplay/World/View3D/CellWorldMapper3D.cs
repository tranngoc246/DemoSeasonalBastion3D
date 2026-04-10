using SeasonalBastion.Contracts;
using SeasonalBastion.WorldGen.Authoring.ScriptableObjects;
using SeasonalBastion.WorldGen.Runtime.Models;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class CellWorldMapper3D
    {
        private readonly WorldMeshSettings _meshSettings;
        private readonly WorldGenerationResult _world;
        private readonly Vector3 _origin;
        private readonly Vector3 _gridOriginOffset;

        public CellWorldMapper3D(WorldMeshSettings meshSettings, WorldGenerationResult world, Vector3 origin)
        {
            _meshSettings = meshSettings;
            _world = world;
            _origin = origin;

            float cellSize = CellSize;
            float halfWidth = ((_world?.Width ?? 0) - 1) * 0.5f * cellSize;
            float halfHeight = ((_world?.Height ?? 0) - 1) * 0.5f * cellSize;
            _gridOriginOffset = new Vector3(-halfWidth, 0f, halfHeight);
        }

        public float CellSize => _meshSettings != null ? _meshSettings.meshScale : 1f;
        public int Width => _world?.Width ?? 0;
        public int Height => _world?.Height ?? 0;

        public Vector3 CellToWorldCenter(CellPos cell)
        {
            float x = _origin.x + _gridOriginOffset.x + cell.X * CellSize;
            float z = _origin.z + _gridOriginOffset.z - cell.Y * CellSize;
            float y = _origin.y + GetHeightAtCell(cell);
            return new Vector3(x, y, z);
        }

        public CellPos WorldToCell(Vector3 world)
        {
            float localX = (world.x - _origin.x - _gridOriginOffset.x) / CellSize;
            float localY = (_origin.z + _gridOriginOffset.z - world.z) / CellSize;
            return new CellPos(Mathf.RoundToInt(localX), Mathf.RoundToInt(localY));
        }

        public float GetHeightAtCell(CellPos cell)
        {
            if (_world == null || _world.Cells == null)
                return 0f;

            if (!IsInside(cell))
                return 0f;

            TerrainCellData data = _world.Cells[cell.X, cell.Y];
            return data.Height;
        }

        public float GetAverageHeightForFootprint(CellPos anchor, int sizeX, int sizeY)
        {
            if (_world == null || _world.Cells == null)
                return 0f;

            int count = 0;
            float sum = 0f;
            for (int dy = 0; dy < sizeY; dy++)
            {
                for (int dx = 0; dx < sizeX; dx++)
                {
                    CellPos c = new(anchor.X + dx, anchor.Y + dy);
                    if (!IsInside(c))
                        continue;

                    sum += _world.Cells[c.X, c.Y].Height;
                    count++;
                }
            }

            return count > 0 ? sum / count : 0f;
        }

        public Vector3 FootprintToWorldCenter(CellPos anchor, int sizeX, int sizeY)
        {
            float avgHeight = GetAverageHeightForFootprint(anchor, sizeX, sizeY);
            float x = _origin.x + _gridOriginOffset.x + ((anchor.X + (sizeX - 1) * 0.5f) * CellSize);
            float z = _origin.z + _gridOriginOffset.z - ((anchor.Y + (sizeY - 1) * 0.5f) * CellSize);
            return new Vector3(x, _origin.y + avgHeight, z);
        }

        public bool IsInside(CellPos cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }
    }
}
