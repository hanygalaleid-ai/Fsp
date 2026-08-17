using System.Collections.Generic;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class SpawnPointDistributor : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        private int nextIndex;

        public void ResetDistribution()
        {
            nextIndex = 0;
            Shuffle(spawnPoints);
        }

        public bool TryGetNext(out Vector3 position, out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.identity;

            if (spawnPoints == null || spawnPoints.Length == 0) return false;

            for (int attempts = 0; attempts < spawnPoints.Length; attempts++)
            {
                Transform point = spawnPoints[nextIndex % spawnPoints.Length];
                nextIndex++;
                if (point == null) continue;

                position = point.position;
                rotation = point.rotation;
                return true;
            }

            return false;
        }

        private static void Shuffle(IList<Transform> list)
        {
            if (list == null) return;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
