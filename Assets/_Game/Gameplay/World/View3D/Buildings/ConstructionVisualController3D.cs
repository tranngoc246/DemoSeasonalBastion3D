using UnityEngine;

namespace SeasonalBastion
{
    public sealed class ConstructionVisualController3D : MonoBehaviour
    {
        [SerializeField] private Color _constructionColor = new(1f, 0.45f, 0.05f, 0.95f);
        [SerializeField] private Color _completedColor = new(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private float _constructionHeightScale = 0.6f;

        public void Apply(Renderer renderer, bool isUnderConstruction)
        {
            if (renderer == null)
                return;

            if (renderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                    renderer.sharedMaterial = new Material(shader);
            }

            if (renderer.sharedMaterial != null)
                renderer.sharedMaterial.color = isUnderConstruction ? _constructionColor : _completedColor;

            renderer.shadowCastingMode = isUnderConstruction
                ? UnityEngine.Rendering.ShadowCastingMode.Off
                : UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = !isUnderConstruction;
        }

        public Vector3 AdjustScale(Vector3 targetScale, bool isUnderConstruction)
        {
            if (!isUnderConstruction)
                return targetScale;

            return new Vector3(targetScale.x, Mathf.Max(0.1f, targetScale.y * _constructionHeightScale), targetScale.z);
        }
    }
}
