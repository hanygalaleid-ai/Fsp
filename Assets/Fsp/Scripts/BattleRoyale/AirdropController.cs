using System;
using System.Collections;
using System.Collections.Generic;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class AirdropController : MonoBehaviour
    {
        [SerializeField] private GameObject cratePrefab;
        [SerializeField] private Transform[] dropPoints;
        [SerializeField] private float firstDropDelay = 180f;
        [SerializeField] private float repeatDelay = 240f;
        [SerializeField] private float spawnHeight = 120f;

        private Coroutine loop;
        private bool authoritativeClock;
        private readonly HashSet<int> spawnedDrops = new();

        private void OnEnable()
        {
            if (!authoritativeClock) loop = StartCoroutine(DropLoop());
        }

        private void OnDisable()
        {
            if (loop != null) StopCoroutine(loop);
            loop = null;
        }

        private IEnumerator DropLoop()
        {
            yield return new WaitForSeconds(firstDropDelay);
            while (enabled && !authoritativeClock)
            {
                SpawnAirdrop();
                yield return new WaitForSeconds(repeatDelay);
            }
        }

        public void ApplyAuthoritativeElapsed(float elapsedSeconds)
        {
            if (!authoritativeClock)
            {
                authoritativeClock = true;
                if (loop != null) StopCoroutine(loop);
                loop = null;
            }

            if (cratePrefab == null || dropPoints == null || dropPoints.Length == 0) return;
            float elapsed = Mathf.Max(0f, elapsedSeconds);
            if (elapsed < firstDropDelay) return;

            int latestIndex = Mathf.FloorToInt((elapsed - firstDropDelay) / Mathf.Max(1f, repeatDelay));
            for (int i = 0; i <= latestIndex; i++)
            {
                if (spawnedDrops.Contains(i)) continue;
                SpawnAuthoritativeDrop(i);
            }
        }

        public void SpawnAirdrop()
        {
            if (cratePrefab == null || dropPoints == null || dropPoints.Length == 0) return;
            Transform point = dropPoints[UnityEngine.Random.Range(0, dropPoints.Length)];
            if (point == null) return;
            Instantiate(cratePrefab, point.position + Vector3.up * spawnHeight, point.rotation);
        }

        private void SpawnAuthoritativeDrop(int dropIndex)
        {
            if (dropPoints == null || dropPoints.Length == 0) return;
            string matchId = MatchRoomState.HasMatch ? MatchRoomState.MatchId : "offline";
            int pointIndex = PositiveMod(StableHash(matchId + ":airdrop:" + dropIndex), dropPoints.Length);
            Transform point = dropPoints[pointIndex];
            if (point == null) return;

            GameObject crate = Instantiate(cratePrefab, point.position + Vector3.up * spawnHeight, point.rotation);
            crate.name = $"Airdrop_{dropIndex:000}";
            spawnedDrops.Add(dropIndex);
        }

        private static int PositiveMod(int value, int modulo)
        {
            if (modulo <= 0) return 0;
            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in value ?? string.Empty) hash = hash * 31 + c;
                return hash;
            }
        }
    }
}
