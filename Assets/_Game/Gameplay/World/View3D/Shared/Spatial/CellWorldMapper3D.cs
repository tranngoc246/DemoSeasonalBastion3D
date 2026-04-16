using SeasonalBastion.Contracts;
using SeasonalBastion.WorldGen.Runtime.Models;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class CellWorldMapper3D
    {
        private readonly GridWorldSettings _settings;
        private readonly WorldGenerationResult _world;
        private readonly Vector3 _gridOriginOffset;

        public CellWorldMapper3D(GridWorldSettings settings, WorldGenerationResult world)
        {
            _settings = settings ?? new GridWorldSettings(1f, Vector3.zero);
            _world = world;

            float cellSize = CellSize;
            float halfWidth = ((Width - 1) * 0.5f) * cellSize;
            float halfHeight = ((Height - 1) * 0.5f) * cellSize;
            _gridOriginOffset = new Vector3(-halfWidth, 0f, halfHeight);
        }

        public float CellSize => _settings.CellSize;
        public int Width => _world?.Width ?? 0;
        public int Height => _world?.Height ?? 0;
        public Vector3 Origin => _settings.Origin;

        public Vector3 CellToWorldCenter(CellPos cell)
        {
            Vector3 corner = CellToWorldCorner(cell);
            float y = Origin.y + GetHeightAtCell(cell);
            return new Vector3(
                corner.x + CellSize * 0.5f,
                y,
                corner.z + GetGridYWorldZSign() * (CellSize * 0.5f));
        }

        public Vector3 CellToWorldCorner(CellPos cell)
        {
            float x = Origin.x + _gridOriginOffset.x + cell.X * CellSize;
            float z = Origin.z + _gridOriginOffset.z + cell.Y * CellSize * GetGridYWorldZSign();
            return new Vector3(x, Origin.y, z);
        }

        public Vector3 CellToWorldCenterFlat(CellPos cell)
        {
            Vector3 corner = CellToWorldCorner(cell);
            return new Vector3(
                corner.x + CellSize * 0.5f,
                Origin.y,
                corner.z + GetGridYWorldZSign() * (CellSize * 0.5f));
        }

        public Bounds GetFootprintWorldBounds(CellPos anchor, int sizeX, int sizeY)
        {
            return GetFootprintWorldBounds(anchor, sizeX, sizeY, true);
        }

        public Bounds GetFootprintWorldBounds(CellPos anchor, int sizeX, int sizeY, bool includeTerrainHeight)
        {
            int clampedSizeX = Mathf.Max(1, sizeX);
            int clampedSizeY = Mathf.Max(1, sizeY);

            Vector3 minCorner = CellToWorldCorner(anchor);
            Vector3 maxCorner = CellToWorldCorner(new CellPos(anchor.X + clampedSizeX, anchor.Y + clampedSizeY));

            float minX = Mathf.Min(minCorner.x, maxCorner.x);
            float maxX = Mathf.Max(minCorner.x, maxCorner.x);
            float minZ = Mathf.Min(minCorner.z, maxCorner.z);
            float maxZ = Mathf.Max(minCorner.z, maxCorner.z);

            float centerY = includeTerrainHeight
                ? Origin.y + GetAverageHeightForFootprint(anchor, clampedSizeX, clampedSizeY)
                : Origin.y;

            Vector3 center = new Vector3((minX + maxX) * 0.5f, centerY, (minZ + maxZ) * 0.5f);
            Vector3 size = new Vector3(Mathf.Abs(maxX - minX), 0f, Mathf.Abs(maxZ - minZ));
            return new Bounds(center, size);
        }

        public bool TryWorldToCell(Vector3 world, out CellPos cell)
        {
            float localX = (world.x - Origin.x - _gridOriginOffset.x) / CellSize;
            float localY = ((world.z - Origin.z - _gridOriginOffset.z) / GetGridYWorldZSign()) / CellSize;

            cell = new CellPos(Mathf.FloorToInt(localX), Mathf.FloorToInt(localY));
            return IsInside(cell);
        }

        public CellPos WorldToCell(Vector3 world)
        {
            if (TryWorldToCell(world, out var cell))
                return cell;

            float localX = (world.x - Origin.x - _gridOriginOffset.x) / CellSize;
            float localY = ((world.z - Origin.z - _gridOriginOffset.z) / GetGridYWorldZSign()) / CellSize;
            return new CellPos(Mathf.FloorToInt(localX), Mathf.FloorToInt(localY));
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
            return GetFootprintWorldBounds(anchor, sizeX, sizeY).center;
        }

        public bool IsInside(CellPos cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }

        private float GetGridYWorldZSign()
        {
            return _settings.InvertGridYOnWorldZ ? -1f : 1f;
        }
    }
}
