#if UNITY_EDITOR
using System.IO;
using SeasonalBastion.WorldGen.Authoring.ScriptableObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SeasonalBastion.Editor
{
    public static class CreateDemoGameplayScene
    {
        private const string ScenePath = "Assets/Scenes/DemoGameplayScene.unity";
        private const string AssetRoot = "Assets/_Game/Generated";
        private const string MeshSettingsPath = AssetRoot + "/WorldMeshSettings.asset";
        private const string HeightSettingsPath = AssetRoot + "/WorldHeightSettings.asset";
        private const string PrefabCatalogPath = AssetRoot + "/PrefabCatalog3D.asset";

        [MenuItem("SeasonalBastion/Create Demo Gameplay Scene")]
        public static void CreateScene()
        {
            EnsureFolder("Assets/_Game");
            EnsureFolder(AssetRoot);
            EnsureFolder("Assets/_Game/Generated");

            var meshSettings = LoadOrCreateMeshSettings();
            var heightSettings = LoadOrCreateHeightSettings();
            var prefabCatalog = LoadOrCreatePrefabCatalog();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DemoGameplayScene";

            CreateDirectionalLight();
            var camera = CreateMainCamera();
            var terrainRoot = CreateTerrainRoot(meshSettings, heightSettings);
            var bootstrap = CreateGameplayBootstrap(terrainRoot);
            var worldView = CreateWorldView(prefabCatalog, terrainRoot, bootstrap);
            SetPrivate(bootstrap, "_worldView", worldView);
            var selectionBundle = CreateSelectionAndPreview(camera, terrainRoot, bootstrap, worldView);
            CreateInstaller(camera, terrainRoot, bootstrap, worldView, selectionBundle.selection, selectionBundle.highlight, selectionBundle.preview);
            CreateEventSystemIfMissing();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(ScenePath));
            Debug.Log($"[CreateDemoGameplayScene] Created clean gameplay scene at {ScenePath}");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static WorldMeshSettings LoadOrCreateMeshSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<WorldMeshSettings>(MeshSettingsPath);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<WorldMeshSettings>();
            asset.meshScale = 2.5f;
            asset.useFlatShading = false;
            asset.chunkSizeIndex = 2;
            asset.flatShadedChunkSizeIndex = 0;
            AssetDatabase.CreateAsset(asset, MeshSettingsPath);
            return asset;
        }

        private static WorldHeightSettings LoadOrCreateHeightSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<WorldHeightSettings>(HeightSettingsPath);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<WorldHeightSettings>();
            asset.heightMultiplier = 25f;
            asset.useFalloff = true;
            asset.noiseSettings.scale = 96f;
            asset.noiseSettings.octaves = 5;
            asset.noiseSettings.persistence = 0.5f;
            asset.noiseSettings.lacunarity = 2f;
            asset.noiseSettings.seed = 12345;
            AssetDatabase.CreateAsset(asset, HeightSettingsPath);
            return asset;
        }

        private static PrefabCatalog3D LoadOrCreatePrefabCatalog()
        {
            var asset = AssetDatabase.LoadAssetAtPath<PrefabCatalog3D>(PrefabCatalogPath);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<PrefabCatalog3D>();
            asset.buildingScale = new Vector3(1f, 2f, 1f);
            asset.actorScale = Vector3.one * 0.75f;
            AssetDatabase.CreateAsset(asset, PrefabCatalogPath);
            return asset;
        }

        private static void CreateDirectionalLight()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static Camera CreateMainCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderShadows = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            go.AddComponent<StrategyCameraController3D>();
            return camera;
        }

        private static TerrainGameplayRuntimeHost CreateTerrainRoot(WorldMeshSettings meshSettings, WorldHeightSettings heightSettings)
        {
            var go = new GameObject("TerrainRuntime");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.AddComponent<MeshCollider>();
            var host = go.AddComponent<TerrainGameplayRuntimeHost>();
            go.AddComponent<TerrainGameplayDebugGizmos>();
            SetPrivate(host, "_meshSettings", meshSettings);
            SetPrivate(host, "_heightSettings", heightSettings);
            return host;
        }

        private static GameplayRuntimeBootstrap CreateGameplayBootstrap(TerrainGameplayRuntimeHost host)
        {
            var go = new GameObject("GameplayRuntime");
            var bootstrap = go.AddComponent<GameplayRuntimeBootstrap>();
            SetPrivate(bootstrap, "_terrainHost", host);
            SetPrivate(bootstrap, "_seedDemoContent", true);
            return bootstrap;
        }

        private static WorldViewRoot3D CreateWorldView(PrefabCatalog3D prefabCatalog, TerrainGameplayRuntimeHost host, GameplayRuntimeBootstrap bootstrap)
        {
            var go = new GameObject("WorldView3D");
            var view = go.AddComponent<WorldViewRoot3D>();
            SetPrivate(view, "_runtimeHost", host);
            SetPrivate(view, "_gameplayBootstrap", bootstrap);
            SetPrivate(view, "_prefabCatalog", prefabCatalog);
            return view;
        }

        private static (WorldSelectionController3D selection, CellHighlightView3D highlight, PlacementPreviewController3D preview) CreateSelectionAndPreview(Camera camera, TerrainGameplayRuntimeHost host, GameplayRuntimeBootstrap bootstrap, WorldViewRoot3D worldView)
        {
            var selectionGo = new GameObject("WorldSelection");
            var selection = selectionGo.AddComponent<WorldSelectionController3D>();
            SetPrivate(selection, "_camera", camera);
            SetPrivate(selection, "_runtimeHost", host);
            SetPrivate(selection, "_gameplayBootstrap", bootstrap);

            var highlightGo = new GameObject("CellHighlight");
            var highlight = highlightGo.AddComponent<CellHighlightView3D>();
            SetPrivate(highlight, "_runtimeHost", host);
            SetPrivate(highlight, "_selection", selection);

            var previewGo = new GameObject("PlacementPreview");
            var preview = previewGo.AddComponent<PlacementPreviewController3D>();
            SetPrivate(preview, "_runtimeHost", host);
            SetPrivate(preview, "_gameplayBootstrap", bootstrap);
            SetPrivate(preview, "_selection", selection);
            SetPrivate(preview, "_worldView", worldView);
            SetPrivate(preview, "_placementMode", true);
            SetEnumPrivate(preview, "_confirmKey", KeyCode.Mouse1);
            return (selection, highlight, preview);
        }

        private static void CreateInstaller(Camera camera, TerrainGameplayRuntimeHost host, GameplayRuntimeBootstrap bootstrap, WorldViewRoot3D worldView, WorldSelectionController3D selection, CellHighlightView3D highlight, PlacementPreviewController3D preview)
        {
            var go = new GameObject("GameplaySceneInstaller3D");
            var installer = go.AddComponent<GameplaySceneInstaller3D>();
            SetPrivate(installer, "_camera", camera);
            SetPrivate(installer, "_terrainHost", host);
            SetPrivate(installer, "_bootstrap", bootstrap);
            SetPrivate(installer, "_worldView", worldView);
            SetPrivate(installer, "_selection", selection);
            SetPrivate(installer, "_highlight", highlight);
            SetPrivate(installer, "_preview", preview);
            var strategyCamera = camera != null ? camera.GetComponent<StrategyCameraController3D>() : null;
            if (strategyCamera != null)
                SetPrivate(installer, "_strategyCamera", strategyCamera);
        }

        private static void CreateEventSystemIfMissing()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static void SetPrivate(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(fieldName);
            if (property == null)
                return;
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string fieldName, bool value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(fieldName);
            if (property == null)
                return;
            property.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnumPrivate(Object target, string fieldName, KeyCode value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(fieldName);
            if (property == null)
                return;
            property.enumValueIndex = (int)value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
