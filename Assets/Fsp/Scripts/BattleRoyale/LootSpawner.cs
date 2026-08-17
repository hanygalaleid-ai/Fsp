using Fsp.Backend;
using Fsp.Inventory;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class LootSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] lootPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Range(0f, 1f)] private float spawnChance = 0.75f;
        [SerializeField] private int mapSeed = 17321;

        public void SpawnLoot()
        {
            if (lootPrefabs == null || lootPrefabs.Length == 0 || spawnPoints == null) return;

            string matchId = MatchRoomState.HasMatch ? MatchRoomState.MatchId : "offline";
            int seed = StableHash(matchId) ^ mapSeed;
            var random = new System.Random(seed);

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[i];
                if (point == null || random.NextDouble() > spawnChance) continue;

                GameObject prefab = lootPrefabs[random.Next(0, lootPrefabs.Length)];
                if (prefab == null) continue;

                GameObject go = Instantiate(prefab, point.position, point.rotation);
                LootPickup pickup = go.GetComponent<LootPickup>();
                if (pickup != null)
                    pickup.SetLootId($"world:{matchId}:{i}");
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }
    }
}
