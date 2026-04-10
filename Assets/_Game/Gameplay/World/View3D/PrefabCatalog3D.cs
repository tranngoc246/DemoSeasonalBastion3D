using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    [CreateAssetMenu(fileName = "PrefabCatalog3D", menuName = "SeasonalBastion/Gameplay/Prefab Catalog 3D")]
    public sealed class PrefabCatalog3D : ScriptableObject
    {
        public GameObject defaultBuildingPrefab;
        public GameObject defaultNpcPrefab;
        public GameObject defaultEnemyPrefab;
        public Vector3 buildingScale = Vector3.one;
        public Vector3 actorScale = Vector3.one * 0.75f;

        public GameObject GetBuildingPrefab(BuildingDef def)
        {
            return defaultBuildingPrefab;
        }

        public GameObject GetNpcPrefab(NpcDef def)
        {
            return defaultNpcPrefab;
        }

        public GameObject GetEnemyPrefab(EnemyDef def)
        {
            return defaultEnemyPrefab;
        }
    }
}
