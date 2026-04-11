using UnityEngine;

namespace SeasonalBastion
{
    public sealed class FootprintOverlay3D : MonoBehaviour
    {
        [SerializeField] private Color _entryRoadColor = new(1f, 0.9f, 0.15f, 0.55f);
        [SerializeField] private float _heightOffset = 0.12f;
        [SerializeField] private float _cellFill = 0.9f;

        public GameObject CreateEntryMarker(Transform parent, string name)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            Renderer renderer = go.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
                renderer.sharedMaterial = new Material(shader);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        public void PlaceEntryMarker(GameObject marker, CellWorldMapper3D mapper, SeasonalBastion.Contracts.CellPos cell)
        {
            if (marker == null || mapper == null)
                return;

            Vector3 pos = mapper.CellToWorldCenter(cell);
            float cellSize = mapper.CellSize * _cellFill;
            marker.transform.position = pos + Vector3.up * _heightOffset;
            marker.transform.localScale = new Vector3(cellSize, 0.03f, cellSize);

            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial.color = _entryRoadColor;
        }
    }
}
