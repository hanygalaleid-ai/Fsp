using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Fsp.Bots
{
    public sealed class BotSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject botPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Min(1)] private int targetPopulation = 32;
        [SerializeField, Min(0f)] private float spawnRadius = 8f;

        private readonly List<GameObject> spawnedBots = new();

        public int SpawnedCount => spawnedBots.Count;

        public void FillToTarget(int humanPlayers = 1)
        {
            int botsNeeded = Mathf.Max(0, targetPopulation - Mathf.Max(0, humanPlayers));
            while (spawnedBots.Count < botsNeeded)
            {
                if (!TrySpawnOne()) break;
            }
        }

        public bool TrySpawnOne()
        {
            if (botPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
                return false;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 candidate = point.position + Random.insideUnitSphere * spawnRadius;
            candidate.y = point.position.y;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(2f, spawnRadius), NavMesh.AllAreas))
                candidate = hit.position;

            GameObject bot = Instantiate(botPrefab, candidate, point.rotation);
            spawnedBots.Add(bot);
            return true;
        }

        public void RemoveDestroyedBots()
        {
            spawnedBots.RemoveAll(x => x == null);
        }
    }
}
