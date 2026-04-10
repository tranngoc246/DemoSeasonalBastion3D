using UnityEngine;
using SeasonalBastion.WorldGen.Authoring.ScriptableObjects;
using SeasonalBastion.WorldGen.Runtime.Generators;
using SeasonalBastion.WorldGen.Runtime.Meshes;
using SeasonalBastion.WorldGen.Runtime.Models;

namespace SeasonalBastion.WorldGen.Authoring.MonoBehaviours
{
    public sealed class WorldGenPreviewController : MonoBehaviour
    {
        public Renderer textureRender;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public DrawMode drawMode;
        public WorldMeshSettings meshSettings;
        public WorldHeightSettings heightSettings;
        public TerrainLayerProfile terrainLayerProfile;
        public Material terrainMaterial;

        [Range(0, WorldMeshSettings.NumSupportedLods - 1)]
        public int editorPreviewLod;

        public bool autoUpdate;

        public enum DrawMode
        {
            NoiseMap,
            Mesh,
            FalloffMap
        }

        public void DrawMapInEditor()
        {
            if (meshSettings == null || heightSettings == null)
            {
                Debug.LogWarning("WorldGenPreviewController needs meshSettings and heightSettings assigned.", this);
                return;
            }

            if (terrainLayerProfile != null && terrainMaterial != null)
            {
                terrainLayerProfile.ApplyToMaterial(terrainMaterial);
                terrainLayerProfile.UpdateMeshHeights(terrainMaterial, heightSettings.MinHeight, heightSettings.MaxHeight);
            }

            HeightMapData heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.NumVertsPerLine, meshSettings.NumVertsPerLine, heightSettings, Vector2.zero);

            switch (drawMode)
            {
                case DrawMode.NoiseMap:
                    DrawTexture(TextureFromHeightMap(heightMap));
                    break;
                case DrawMode.Mesh:
                    DrawMesh(TerrainMeshGenerator.GenerateTerrainMesh(heightMap.Values, meshSettings, editorPreviewLod));
                    break;
                case DrawMode.FalloffMap:
                    DrawTexture(TextureFromValues(FalloffMapGenerator.GenerateFalloffMap(meshSettings.NumVertsPerLine)));
                    break;
            }
        }

        public void DrawTexture(Texture2D texture)
        {
            if (textureRender == null)
            {
                Debug.LogWarning("Texture Renderer is not assigned.", this);
                return;
            }

            textureRender.sharedMaterial.mainTexture = texture;
            textureRender.transform.localScale = new Vector3(texture.width, 1f, texture.height) / 10f;
            textureRender.gameObject.SetActive(true);

            if (meshFilter != null)
            {
                meshFilter.gameObject.SetActive(false);
            }
        }

        public void DrawMesh(MeshData meshData)
        {
            if (meshFilter == null)
            {
                Debug.LogWarning("MeshFilter is not assigned.", this);
                return;
            }

            meshFilter.sharedMesh = meshData.CreateMesh();

            if (textureRender != null)
            {
                textureRender.gameObject.SetActive(false);
            }

            meshFilter.gameObject.SetActive(true);
        }

        private Texture2D TextureFromHeightMap(HeightMapData heightMap)
        {
            return TextureFromValues(heightMap.Values);
        }

        private Texture2D TextureFromValues(float[,] values)
        {
            int width = values.GetLength(0);
            int height = values.GetLength(1);
            Texture2D texture = new(width, height)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] colourMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = values[x, y];
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, value);
                }
            }

            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        private void OnValuesUpdated()
        {
            if (!Application.isPlaying)
            {
                DrawMapInEditor();
            }
        }

        private void OnTextureValuesUpdated()
        {
            if (terrainLayerProfile != null && terrainMaterial != null)
            {
                terrainLayerProfile.ApplyToMaterial(terrainMaterial);
            }
        }

        private void OnValidate()
        {
            if (meshSettings != null)
            {
                meshSettings.ValuesUpdated -= OnValuesUpdated;
                meshSettings.ValuesUpdated += OnValuesUpdated;
            }

            if (heightSettings != null)
            {
                heightSettings.ValuesUpdated -= OnValuesUpdated;
                heightSettings.ValuesUpdated += OnValuesUpdated;
            }

            if (terrainLayerProfile != null)
            {
                terrainLayerProfile.ValuesUpdated -= OnTextureValuesUpdated;
                terrainLayerProfile.ValuesUpdated += OnTextureValuesUpdated;
            }
        }
    }
}
