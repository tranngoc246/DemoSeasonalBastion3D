using UnityEngine;

namespace SeasonalBastion.Core.Data
{
    public abstract class UpdatableData : ScriptableObject
    {
        public event System.Action ValuesUpdated;

        [SerializeField] private bool autoUpdate = true;

        public bool AutoUpdate => autoUpdate;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (autoUpdate)
            {
                UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
                UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
            }
        }

        public void NotifyOfUpdatedValues()
        {
            UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
            ValuesUpdated?.Invoke();
        }
#endif
    }
}
