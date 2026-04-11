using UnityEngine;

namespace SeasonalBastion
{
    public sealed class MinimalGameplaySceneSetup3D : MonoBehaviour
    {
        [SerializeField] private TerrainGameplayRuntimeHost _terrainHost;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameplaySceneInstaller3D _installer;
        [SerializeField] private string _expectedGroundObjectName = "TerrainRuntime";

        private void Awake()
        {
            ResolveRefs();
            EnsureRuntimeGroundSetup();
            _installer?.Install();
        }

        [ContextMenu("Apply Minimal 3D Scene Setup")]
        public void ApplySetup()
        {
            ResolveRefs();
            EnsureRuntimeGroundSetup();
            _installer?.Install();
        }

        private void ResolveRefs()
        {
            if (_terrainHost == null)
                _terrainHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            if (_installer == null)
                _installer = FindFirstObjectByType<GameplaySceneInstaller3D>();
        }

        private void EnsureRuntimeGroundSetup()
        {
            if (_terrainHost == null)
                return;

            if (!string.IsNullOrEmpty(_expectedGroundObjectName))
                _terrainHost.gameObject.name = _expectedGroundObjectName;

            MeshCollider collider = _terrainHost.GetComponent<MeshCollider>();
            if (collider == null)
                collider = _terrainHost.gameObject.AddComponent<MeshCollider>();

            MeshFilter filter = _terrainHost.GetComponent<MeshFilter>();
            if (filter == null)
                filter = _terrainHost.gameObject.AddComponent<MeshFilter>();

            MeshRenderer renderer = _terrainHost.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = _terrainHost.gameObject.AddComponent<MeshRenderer>();
        }
    }
}
