using Fsp.Backend;
using Fsp.Inventory;
using UnityEngine;

namespace Fsp.World
{
    public sealed class PoiLootSpawner : MonoBehaviour
    {
        [SerializeField] private MapRuntimeCoordinator mapRuntime;
        [SerializeField] private InventoryItem[] lowTierItems;
        [SerializeField] private InventoryItem[] highTierItems;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Min(0f)] private float baseSpawnChance = 0.55f;

        private void Start()
        {
            if (mapRuntime == null || spawnPoints == null) return;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[i];
                if (point == null) continue;
                MapPoiDefinition poi = mapRuntime.FindPoi(point.position);
                if (poi == null) continue;

                var rng = new System.Random(StableSeed(MatchRoomState.MatchId, poi.poiId, i));
                float chance = Mathf.Clamp01(baseSpawnChance * poi.lootDensity);
                if (rng.NextDouble() > chance) continue;

                bool highTier = rng.NextDouble() < poi.highTierLootChance;
                InventoryItem[] pool = highTier && highTierItems != null && highTierItems.Length > 0 ? highTierItems : lowTierItems;
                if (pool == null || pool.Length == 0) continue;
                InventoryItem prefab = pool[rng.Next(pool.Length)];
                if (prefab == null) continue;

                InventoryItem item = Instantiate(prefab, point.position, point.rotation);
                var identity = item.GetComponent<NetworkLootIdentity>();
                if (identity != null) identity.AssignStableId($"{MatchRoomState.MatchId}:{poi.poiId}:loot:{i}");
            }
        }

        private static int StableSeed(string matchId, string poiId, int index)
        {
            unchecked
            {
                int hash = 23;
                string value = (matchId ?? string.Empty) + "|" + (poiId ?? string.Empty) + "|" + index;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }
    }
}
