using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class TerrainGameplayDebugGizmos : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _host;
        [SerializeField] private bool _drawBuildableCells = true;
        [SerializeField] private bool _drawWaterCells = true;
        [SerializeField] private bool _drawCellCenters;
        [SerializeField, Range(1, 128)] private int _step = 8;
        [SerializeField] private float _cubeSize = 0.2f;

        private void OnDrawGizmosSelected()
        {
            if (_host == null)
                _host = GetComponent<TerrainGameplayRuntimeHost>();

            if (_host == null || _host.Mapper == null || _host.Bridge == null)
                return;

            int width = _host.GeneratedWorld != null ? _host.GeneratedWorld.Width : 0;
            int height = _host.GeneratedWorld != null ? _host.GeneratedWorld.Height : 0;
            if (width <= 0 || height <= 0)
                return;

            int step = Mathf.Max(1, _step);
            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    CellPos cell = new(x, y);
                    Vector3 pos = _host.Mapper.CellToWorldCenter(cell);

                    if (_drawBuildableCells && _host.Bridge.IsBuildable(cell))
                    {
                        Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.45f);
                        Gizmos.DrawCube(pos + Vector3.up * 0.1f, Vector3.one * _cubeSize);
                    }
                    else if (_drawWaterCells && _host.Bridge.IsWater(cell))
                    {
                        Gizmos.color = new Color(0.15f, 0.45f, 1f, 0.45f);
                        Gizmos.DrawCube(pos + Vector3.up * 0.1f, Vector3.one * _cubeSize);
                    }
                    else if (_drawCellCenters)
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
                        Gizmos.DrawSphere(pos, _cubeSize * 0.4f);
                    }
                }
            }
        }
    }
}
