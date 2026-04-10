using System.Linq;
using UnityEngine;
using SeasonalBastion.Core.Data;

namespace SeasonalBastion.WorldGen.Authoring.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TerrainLayerProfile", menuName = "SeasonalBastion/WorldGen/Terrain Layer Profile")]
    public sealed class TerrainLayerProfile : UpdatableData
    {
        private const int TextureSize = 512;
        private const TextureFormat TextureFormat = UnityEngine.TextureFormat.RGB565;

        public TerrainLayer[] layers;

        private float savedMinHeight;
        private float savedMaxHeight;

        public void ApplyToMaterial(Material material)
        {
            if (material == null || layers == null)
            {
                return;
            }

            material.SetInt("layerCount", layers.Length);
            material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
            material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
            material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
            material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());
            material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
            material.SetTexture("baseTextures", GenerateTextureArray(layers.Select(x => x.texture).ToArray()));

            UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            if (material == null)
            {
                return;
            }

            savedMinHeight = minHeight;
            savedMaxHeight = maxHeight;
            material.SetFloat("minHeight", minHeight);
            material.SetFloat("maxHeight", maxHeight);
        }

        private Texture2DArray GenerateTextureArray(Texture2D[] textures)
        {
            Texture2DArray textureArray = new(TextureSize, TextureSize, textures.Length, TextureFormat, true);
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null)
                {
                    continue;
                }

                textureArray.SetPixels(textures[i].GetPixels(), i);
            }

            textureArray.Apply();
            return textureArray;
        }
    }

    [System.Serializable]
    public class TerrainLayer
    {
        public Texture2D texture;
        public Color tint = Color.white;

        [Range(0f, 1f)]
        public float tintStrength = 1f;

        [Range(0f, 1f)]
        public float startHeight;

        [Range(0f, 1f)]
        public float blendStrength = 0.1f;

        public float textureScale = 1f;
    }
}
