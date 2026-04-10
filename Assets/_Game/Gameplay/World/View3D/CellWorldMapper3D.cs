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

        public CellWorldMapper3D(WorldMeshSettings meshSettings, WorldGenerationResult world, Vector3 origin)
        {
            _meshSettings = meshSettings;
            _world = world;
            _origin = origin;
        }

        public float CellSize => _meshSettings != null ? _meshSettings.meshScale : 1f;
        public int Width => _world?.Width ?? 0;
        public int Height => _world?.Height ?? 0;

        public Vector3 CellToWorldCenter(CellPos cell)
        {
            float x = _origin.x + (cell.X + 0.5f) * CellSize;
            float z = _origin.z + (cell.Y + 0.5f) * CellSize;
            float y = _origin.y + GetHeightAtCell(cell);
            return new Vector3(x, y, z);
        }

        public CellPos WorldToCell(Vector3 world)
        {
            float localX = (world.x - _origin.x) / CellSize;
            float localZ = (world.z - _origin.z) / CellSize;
            return new CellPos(Mathf.FloorToInt(localX), Mathf.FloorToInt(localZ));
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

        public bool IsInside(CellPos cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }
    }
}
