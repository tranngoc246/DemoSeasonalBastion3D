using UnityEditor;
using SeasonalBastion.Core.Data;
using UnityEngine;

namespace SeasonalBastion.WorldGen.Authoring.Editor
{
    [CustomEditor(typeof(UpdatableData), true)]
    public sealed class UpdatableDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdatableData data = (UpdatableData)target;
            if (GUILayout.Button("Update"))
            {
                data.NotifyOfUpdatedValues();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
