using System.Collections.Generic;
using Fsp.BattleRoyale;
using Fsp.Core;
using Fsp.Player;
using Fsp.Presentation;
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
        [SerializeField] private float fallbackRingRadius = 900f;

        private readonly List<GameObject> spawnedBots = new();
        public int SpawnedCount => spawnedBots.Count;

        public void ConfigureSpawnPoints(Transform[] points) => spawnPoints = points;

        public void FillToTarget(int humanPlayers = 1)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Keep Android release stable until authored bot prefabs replace procedural fallbacks.
            if (botPrefab == null) return;
#endif
            int botsNeeded = Mathf.Max(0, targetPopulation - Mathf.Max(0, humanPlayers));
            while (spawnedBots.Count < botsNeeded)
                if (!TrySpawnOne()) break;
        }

        public bool TrySpawnOne()
        {
            int index = spawnedBots.Count;
            Vector3 candidate;
            Quaternion rotation = Quaternion.identity;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform point = spawnPoints[index % spawnPoints.Length];
                if (point == null) return false;
                candidate = point.position + Random.insideUnitSphere * spawnRadius;
                candidate.y = point.position.y;
                rotation = point.rotation;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(2f, spawnRadius), NavMesh.AllAreas))
                    candidate = hit.position;
            }
            else
            {
                float angle = index * 137.508f * Mathf.Deg2Rad;
                float radius = Mathf.Max(150f, fallbackRingRadius * Mathf.Sqrt((index + 1f) / Mathf.Max(1f, targetPopulation)));
                candidate = transform.position + new Vector3(Mathf.Cos(angle) * radius, 1.05f, Mathf.Sin(angle) * radius);
            }

            GameObject bot = botPrefab != null ? Instantiate(botPrefab, candidate, rotation) : CreatePlaceholderBot(index, candidate, rotation);
            EnsureBattleRoyaleComponents(bot);
            spawnedBots.Add(bot);
            return true;
        }

        public void RemoveDestroyedBots() => spawnedBots.RemoveAll(x => x == null);

        private static GameObject CreatePlaceholderBot(int index, Vector3 position, Quaternion rotation)
        {
            var go = AndroidSafeMesh.CreateBox($"Bot_{index + 1:00}_Placeholder");
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.localScale = new Vector3(0.7f, 1.8f, 0.7f);

            var controller = go.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            go.AddComponent<PlayerVitals>();
            var participant = go.AddComponent<MatchParticipant>();
            participant.ConfigureAsBot($"Bot {index + 1}");
            go.AddComponent<PlayerDamageable>();
            go.AddComponent<FallbackBotAgent>();
            go.AddComponent<StarterProceduralCharacterVisual>();
            return go;
        }

        private static void EnsureBattleRoyaleComponents(GameObject bot)
        {
            if (bot == null) return;
            if (bot.GetComponent<PlayerDamageable>() == null) bot.AddComponent<PlayerDamageable>();
            if (bot.GetComponent<SafeZoneDamageApplier>() == null) bot.AddComponent<SafeZoneDamageApplier>();
            if (bot.GetComponent<FallbackBotAgent>() == null && bot.GetComponent<CharacterController>() != null) bot.AddComponent<FallbackBotAgent>();
        }
    }
}
