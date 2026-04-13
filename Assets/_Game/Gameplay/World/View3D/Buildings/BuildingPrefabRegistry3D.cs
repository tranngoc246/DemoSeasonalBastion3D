using System;
using System.Collections.Generic;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    [CreateAssetMenu(fileName = "BuildingPrefabRegistry3D", menuName = "SeasonalBastion/Gameplay/Building Prefab Registry 3D")]
    public sealed class BuildingPrefabRegistry3D : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string buildingDefId;
            public GameObject prefab;
        }

        [SerializeField] private GameObject _defaultBuildingPrefab;
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        private Dictionary<string, GameObject> _lookup;

        public GameObject DefaultBuildingPrefab => _defaultBuildingPrefab;

        public GameObject GetPrefab(BuildingDef def)
        {
            if (def == null)
                return _defaultBuildingPrefab;

            EnsureLookup();
            return _lookup.TryGetValue(def.DefId, out var prefab) && prefab != null
                ? prefab
                : _defaultBuildingPrefab;
        }

        private void EnsureLookup()
        {
            if (_lookup != null)
                return;

            _lookup = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];
                if (string.IsNullOrWhiteSpace(entry.buildingDefId) || entry.prefab == null)
                    continue;

                _lookup[entry.buildingDefId] = entry.prefab;
            }
        }
    }
}
