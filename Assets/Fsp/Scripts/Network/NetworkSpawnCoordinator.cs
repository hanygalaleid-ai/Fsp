using System;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkSpawnCoordinator : MonoBehaviour
    {
        [SerializeField] private Transform localPlayer;
        [SerializeField] private Transform[] spawnPoints;

        private void Start()
        {
            if (localPlayer == null || spawnPoints == null || spawnPoints.Length == 0) return;
            int index = StableIndex(SupabaseSession.UserId, MatchRoomState.MatchId, spawnPoints.Length);
            Transform point = spawnPoints[index];
            localPlayer.SetPositionAndRotation(point.position, point.rotation);
        }

        private static int StableIndex(string playerId, string matchId, int count)
        {
            string seed = (playerId ?? string.Empty) + ":" + (matchId ?? string.Empty);
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < seed.Length; i++) hash = hash * 31 + seed[i];
                if (hash == int.MinValue) hash = 0;
                return Math.Abs(hash) % Mathf.Max(1, count);
            }
        }
    }
}
