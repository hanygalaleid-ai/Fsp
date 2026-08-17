using Fsp.Backend;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.World
{
    public sealed class PoiVehicleSpawner : MonoBehaviour
    {
        [SerializeField] private MapRuntimeCoordinator mapRuntime;
        [SerializeField] private SimpleVehicleController[] vehiclePrefabs;
        [SerializeField] private Transform[] spawnPoints;

        private void Start()
        {
            if (mapRuntime == null || vehiclePrefabs == null || vehiclePrefabs.Length == 0 || spawnPoints == null) return;

            var budgets = new System.Collections.Generic.Dictionary<string, int>();
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[i];
                if (point == null) continue;
                MapPoiDefinition poi = mapRuntime.FindPoi(point.position);
                if (poi == null || poi.vehicleSpawnBudget <= 0) continue;

                budgets.TryGetValue(poi.poiId, out int used);
                if (used >= poi.vehicleSpawnBudget) continue;

                var rng = new System.Random(StableSeed(MatchRoomState.MatchId, poi.poiId, i));
                if (rng.NextDouble() > 0.68) continue;

                SimpleVehicleController prefab = vehiclePrefabs[rng.Next(vehiclePrefabs.Length)];
                if (prefab == null) continue;
                Instantiate(prefab, point.position, point.rotation);
                budgets[poi.poiId] = used + 1;
            }
        }

        private static int StableSeed(string matchId, string poiId, int index)
        {
            unchecked
            {
                int hash = 29;
                string value = (matchId ?? string.Empty) + "|" + (poiId ?? string.Empty) + "|vehicle|" + index;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }
    }
}
