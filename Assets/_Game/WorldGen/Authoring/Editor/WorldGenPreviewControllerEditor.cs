using UnityEditor;
using SeasonalBastion.WorldGen.Authoring.MonoBehaviours;
using UnityEngine;

namespace SeasonalBastion.WorldGen.Authoring.Editor
{
    [CustomEditor(typeof(WorldGenPreviewController))]
    public sealed class WorldGenPreviewControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            WorldGenPreviewController previewController = (WorldGenPreviewController)target;

            if (DrawDefaultInspector() && previewController.autoUpdate)
            {
                previewController.DrawMapInEditor();
            }

            if (GUILayout.Button("Generate"))
            {
                previewController.DrawMapInEditor();
            }
        }
    }
}
