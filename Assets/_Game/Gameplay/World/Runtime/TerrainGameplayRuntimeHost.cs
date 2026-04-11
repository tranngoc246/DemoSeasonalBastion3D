using SeasonalBastion.Contracts;
using SeasonalBastion.WorldGen.Authoring.MonoBehaviours;
using SeasonalBastion.WorldGen.Authoring.ScriptableObjects;
using SeasonalBastion.WorldGen.Runtime.Generators;
using SeasonalBastion.WorldGen.Runtime.Meshes;
using SeasonalBastion.WorldGen.Runtime.Models;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SeasonalBastion
{
    public sealed class TerrainGameplayRuntimeHost : MonoBehaviour
    {
        [Header("Optional preview source")]
        [SerializeField] private WorldGenPreviewController _previewController;

        [Header("Required worldgen settings")]
        [SerializeField] private WorldMeshSettings _meshSettings;
        [SerializeField] private WorldHeightSettings _heightSettings;

        [Header("World placement")]
        [SerializeField] private Vector3 _worldOrigin = Vector3.zero;

        [Header("Optional runtime terrain output")]
        [SerializeField] private MeshFilter _runtimeMeshFilter;
        [SerializeField] private MeshRenderer _runtimeMeshRenderer;
        [SerializeField] private MeshCollider _runtimeMeshCollider;
        [SerializeField] private Material _runtimeTerrainMaterial;

        public WorldGenerationResult GeneratedWorld { get; private set; }
        public GridMap GridMap { get; private set; }
        public GridWorldSettings GridWorldSettings { get; private set; }
        public CellWorldMapper3D Mapper { get; private set; }
        public TerrainGameplayBridge Bridge { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        [ContextMenu("Initialize Terrain Gameplay Runtime")]
        public void Initialize()
        {
            ResolveSettings();
            if (_meshSettings == null || _heightSettings == null)
            {
                Debug.LogWarning("[TerrainGameplayRuntimeHost] Missing meshSettings or heightSettings.", this);
                return;
            }

            GeneratedWorld = GenerateWorld();
            if (GeneratedWorld == null)
            {
                Debug.LogWarning("[TerrainGameplayRuntimeHost] Failed to generate world result.", this);
                return;
            }

            GridMap = new GridMap(GeneratedWorld.Width, GeneratedWorld.Height);
            GridWorldSettings = new GridWorldSettings(_meshSettings.meshScale, _worldOrigin, invertGridYOnWorldZ: true);
            Mapper = new CellWorldMapper3D(GridWorldSettings, GeneratedWorld);
            Bridge = new TerrainGameplayBridge(GeneratedWorld, GridMap);
            Bridge.ApplyEmptyGameplayGridFromTerrain();
            BuildRuntimeTerrainMesh();
        }

        private void ResolveSettings()
        {
            if (_previewController == null)
                _previewController = FindObjectOfType<WorldGenPreviewController>();

            if (_previewController != null)
            {
                if (_meshSettings == null)
                    _meshSettings = _previewController.meshSettings;
                if (_heightSettings == null)
                    _heightSettings = _previewController.heightSettings;
            }

            ResolveGeneratedAssetFallbacks();
        }

        private void ResolveGeneratedAssetFallbacks()
        {
            if (_meshSettings != null && _heightSettings != null)
                return;

#if UNITY_EDITOR
            if (_meshSettings == null)
                _meshSettings = AssetDatabase.LoadAssetAtPath<WorldMeshSettings>("Assets/_Game/Generated/WorldMeshSettings.asset");
            if (_heightSettings == null)
                _heightSettings = AssetDatabase.LoadAssetAtPath<WorldHeightSettings>("Assets/_Game/Generated/WorldHeightSettings.asset");
#endif

            if (_meshSettings == null)
                _meshSettings = Resources.Load<WorldMeshSettings>("WorldMeshSettings");
            if (_heightSettings == null)
                _heightSettings = Resources.Load<WorldHeightSettings>("WorldHeightSettings");
        }

        private void BuildRuntimeTerrainMesh()
        {
            if (_runtimeMeshFilter == null)
                _runtimeMeshFilter = GetComponent<MeshFilter>();
            if (_runtimeMeshRenderer == null)
                _runtimeMeshRenderer = GetComponent<MeshRenderer>();
            if (_runtimeMeshCollider == null)
                _runtimeMeshCollider = GetComponent<MeshCollider>();

            if (_runtimeMeshFilter == null || _meshSettings == null || GeneratedWorld?.HeightMap == null)
                return;

            MeshData meshData = TerrainMeshGenerator.GenerateTerrainMesh(GeneratedWorld.HeightMap, _meshSettings, 0);
            Mesh mesh = meshData.CreateMesh();
            _runtimeMeshFilter.sharedMesh = mesh;

            if (_runtimeMeshCollider != null)
                _runtimeMeshCollider.sharedMesh = mesh;

            if (_runtimeMeshRenderer != null)
            {
                Material material = ResolveRuntimeTerrainMaterial();
                if (material != null)
                    _runtimeMeshRenderer.sharedMaterial = material;
            }
        }

        private Material ResolveRuntimeTerrainMaterial()
        {
            if (_runtimeTerrainMaterial != null)
                return _runtimeTerrainMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                return null;

            _runtimeTerrainMaterial = new Material(shader)
            {
                name = "RuntimeTerrainMaterial"
            };
            _runtimeTerrainMaterial.color = new Color(0.42f, 0.48f, 0.42f, 1f);
            return _runtimeTerrainMaterial;
        }

        private WorldGenerationResult GenerateWorld()
        {
            int size = _meshSettings.NumVertsPerLine;
            HeightMapData heightMap = HeightMapGenerator.GenerateHeightMap(size, size, _heightSettings, Vector2.zero);

            bool[,] waterMap = new bool[size, size];
            float[,] slopeMap = new float[size, size];
            bool[,] buildableMap = new bool[size, size];
            TerrainType[,] terrainTypes = new TerrainType[size, size];
            TerrainCellData[,] cells = new TerrainCellData[size, size];

            float waterThreshold = Mathf.Lerp(heightMap.MinValue, heightMap.MaxValue, 0.28f);
            float slopeBuildLimit = 35f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float h = heightMap.Values[x, y];
                    float slope = EstimateSlope(heightMap.Values, x, y, _meshSettings.meshScale);
                    bool isWater = h <= waterThreshold;
                    bool isBuildable = !isWater && slope <= slopeBuildLimit;
                    TerrainType type = ResolveTerrainType(h, slope, waterThreshold, heightMap.MinValue, heightMap.MaxValue);

                    waterMap[x, y] = isWater;
                    slopeMap[x, y] = slope;
                    buildableMap[x, y] = isBuildable;
                    terrainTypes[x, y] = type;
                    cells[x, y] = new TerrainCellData(new Vector2Int(x, y), h, slope, isWater, isBuildable, type);
                }
            }

            StartAreaDefinition startArea = new(false, new Vector2(size * 0.5f, size * 0.5f), size * 0.12f, 0f);
            return new WorldGenerationResult(heightMap.Values, waterMap, slopeMap, buildableMap, terrainTypes, cells, startArea, heightMap.MinValue, heightMap.MaxValue);
        }

        private static float EstimateSlope(float[,] values, int x, int y, float cellSize)
        {
            int width = values.GetLength(0);
            int height = values.GetLength(1);
            int x0 = Mathf.Max(0, x - 1);
            int x1 = Mathf.Min(width - 1, x + 1);
            int y0 = Mathf.Max(0, y - 1);
            int y1 = Mathf.Min(height - 1, y + 1);

            float dx = (values[x1, y] - values[x0, y]) / Mathf.Max(0.0001f, (x1 - x0) * cellSize);
            float dy = (values[x, y1] - values[x, y0]) / Mathf.Max(0.0001f, (y1 - y0) * cellSize);
            float gradient = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Atan(gradient) * Mathf.Rad2Deg;
        }

        private static TerrainType ResolveTerrainType(float height, float slope, float waterThreshold, float minHeight, float maxHeight)
        {
            if (height <= waterThreshold * 0.85f)
                return TerrainType.DeepWater;
            if (height <= waterThreshold)
                return TerrainType.ShallowWater;
            if (slope >= 45f)
                return TerrainType.Cliff;

            float normalized = Mathf.InverseLerp(minHeight, maxHeight, height);
            if (normalized < 0.35f)
                return TerrainType.Sand;
            if (normalized < 0.70f)
                return TerrainType.Grass;
            if (normalized < 0.90f)
                return TerrainType.Rock;
            return TerrainType.Plateau;
        }
    }
}
