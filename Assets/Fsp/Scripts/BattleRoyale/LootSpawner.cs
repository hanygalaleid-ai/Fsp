using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class LootSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] lootPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Range(0f, 1f)] private float spawnChance = 0.75f;

        public void SpawnLoot()
        {
            if (lootPrefabs == null || lootPrefabs.Length == 0 || spawnPoints == null) return;

            foreach (Transform point in spawnPoints)
            {
                if (point == null || Random.value > spawnChance) continue;
                GameObject prefab = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
                if (prefab != null) Instantiate(prefab, point.position, point.rotation);
            }
        }
    }
}
