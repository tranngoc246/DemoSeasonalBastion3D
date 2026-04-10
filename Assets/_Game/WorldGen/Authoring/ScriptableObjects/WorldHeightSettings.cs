using UnityEngine;
using SeasonalBastion.Core.Data;
using SeasonalBastion.WorldGen.Runtime.Models;

namespace SeasonalBastion.WorldGen.Authoring.ScriptableObjects
{
    [CreateAssetMenu(fileName = "WorldHeightSettings", menuName = "SeasonalBastion/WorldGen/World Height Settings")]
    public sealed class WorldHeightSettings : UpdatableData
    {
        public NoiseSettings noiseSettings = new();
        public bool useFalloff = true;
        public float heightMultiplier = 20f;
        public AnimationCurve heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float MinHeight => heightMultiplier * heightCurve.Evaluate(0f);
        public float MaxHeight => heightMultiplier * heightCurve.Evaluate(1f);

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            noiseSettings?.ValidateValues();
            if (heightCurve == null || heightCurve.length == 0)
            {
                heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }

            base.OnValidate();
        }
#endif
    }
}
