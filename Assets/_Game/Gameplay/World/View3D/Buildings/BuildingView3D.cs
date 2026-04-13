using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class BuildingView3D : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private ConstructionVisualController3D _constructionVisual;

        public int RuntimeId { get; private set; }
        public bool IsBuildSite { get; private set; }

        public void BindBuilding(int runtimeId, CellWorldMapper3D mapper, BuildingDef def, BuildingState state, Vector3 visualOffset, Vector3 baseScale)
        {
            RuntimeId = runtimeId;
            IsBuildSite = false;
            ApplyCommon(mapper, def, state.Anchor, state.Rotation, visualOffset, baseScale, false, state.IsConstructed);
            gameObject.name = $"B_{runtimeId}_{state.DefId}";
        }

        public void BindBuildSite(int runtimeId, CellWorldMapper3D mapper, BuildingDef def, BuildSiteState state, Vector3 visualOffset, Vector3 baseScale)
        {
            RuntimeId = runtimeId;
            IsBuildSite = true;
            ApplyCommon(mapper, def, state.Anchor, state.Rotation, visualOffset, baseScale, true, false);
            gameObject.name = $"S_{runtimeId}_{state.BuildingDefId}";
        }

        private void ApplyCommon(CellWorldMapper3D mapper, BuildingDef def, CellPos anchor, Dir4 rotation, Vector3 visualOffset, Vector3 baseScale, bool isUnderConstruction, bool isConstructed)
        {
            if (mapper == null)
                return;

            GetFootprintSize(def, rotation, out int sizeX, out int sizeY);
            transform.position = mapper.FootprintToWorldCenter(anchor, sizeX, sizeY) + visualOffset;

            Vector3 targetScale = new(sizeX * mapper.CellSize * Mathf.Max(0.0001f, baseScale.x), Mathf.Max(1f, baseScale.y), sizeY * mapper.CellSize * Mathf.Max(0.0001f, baseScale.z));
            targetScale = _constructionVisual != null
                ? _constructionVisual.AdjustScale(targetScale, isUnderConstruction || !isConstructed)
                : targetScale;
            transform.localScale = targetScale;
            transform.rotation = Quaternion.identity;

            Renderer renderer = ResolveRenderer();
            if (_constructionVisual != null)
                _constructionVisual.Apply(renderer, isUnderConstruction || !isConstructed);
        }

        private Renderer ResolveRenderer()
        {
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();
            return _renderer;
        }

        private static void GetFootprintSize(BuildingDef def, Dir4 rotation, out int width, out int height)
        {
            int sizeX = def != null ? Mathf.Max(1, def.SizeX) : 1;
            int sizeY = def != null ? Mathf.Max(1, def.SizeY) : 1;
            bool swap = rotation == Dir4.E || rotation == Dir4.W;
            width = swap ? sizeY : sizeX;
            height = swap ? sizeX : sizeY;
        }
    }
}
