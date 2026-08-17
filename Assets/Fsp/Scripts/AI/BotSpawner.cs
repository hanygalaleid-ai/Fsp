using Fsp.BattleRoyale;
using UnityEngine;

namespace Fsp.Bots
{
    public sealed class BotSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject botPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Min(1)] private int targetParticipants = 32;
        [SerializeField] private float fallbackRadius = 40f;

        public int SpawnedBots { get; private set; }

        public void FillToTarget(int humanPlayers)
        {
            int desiredBots = Mathf.Max(0, targetParticipants - Mathf.Max(0, humanPlayers));
            for (int i = SpawnedBots; i < desiredBots; i++)
                SpawnBot(i);
        }

        private void SpawnBot(int index)
        {
            Vector3 position;
            Quaternion rotation = Quaternion.identity;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform point = spawnPoints[index % spawnPoints.Length];
                position = point != null ? point.position : transform.position;
                if (point != null) rotation = point.rotation;
            }
            else
            {
                float angle = (index * 137.508f) * Mathf.Deg2Rad;
                float radius = Mathf.Max(4f, fallbackRadius * Mathf.Sqrt((index + 1f) / Mathf.Max(1f, targetParticipants)));
                position = transform.position + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            GameObject instance = botPrefab != null
                ? Instantiate(botPrefab, position, rotation)
                : CreatePlaceholderBot(index, position, rotation);

            instance.name = $"Bot_{index + 1:00}";
            SpawnedBots++;
        }

        private static GameObject CreatePlaceholderBot(int index, Vector3 position, Quaternion rotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.SetPositionAndRotation(position, rotation);

            var primitiveCollider = go.GetComponent<Collider>();
            if (primitiveCollider != null) Object.Destroy(primitiveCollider);

            var controller = go.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            go.AddComponent<Fsp.Player.PlayerVitals>();
            var participant = go.AddComponent<MatchParticipant>();
            participant.ConfigureAsBot($"Bot {index + 1}");
            return go;
        }
    }
}
