using System;
using Fsp.Backend;
using Fsp.Player;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class HotZoneController : MonoBehaviour
    {
        [SerializeField] private bool enabledForMatch = true;
        [SerializeField] private Transform zoneVisual;
        [SerializeField] private float worldHalfExtent = 1200f;
        [SerializeField] private float radius = 145f;
        [SerializeField] private float startDelay = 150f;
        [SerializeField] private float duration = 55f;
        [SerializeField] private float damagePerSecond = 2.5f;
        [SerializeField] private PlayerVitals localVitals;
        [SerializeField] private Transform localPlayer;
        [SerializeField] private float tickSeconds = 0.75f;

        public Vector3 Center { get; private set; }
        public float Radius => radius;
        public bool IsActive { get; private set; }

        public event Action<bool> ActiveChanged;

        private float elapsed;
        private float nextTick;

        private void Awake()
        {
            if (localPlayer == null && localVitals != null) localPlayer = localVitals.transform;
            PickCenter();
            RefreshVisual();
        }

        private void Update()
        {
            if (!enabledForMatch) return;
            elapsed += Time.deltaTime;
            bool active = elapsed >= startDelay && elapsed < startDelay + duration;
            if (active != IsActive)
            {
                IsActive = active;
                RefreshVisual();
                ActiveChanged?.Invoke(IsActive);
            }

            if (!IsActive || localPlayer == null || localVitals == null || !localVitals.IsAlive || Time.time < nextTick) return;
            nextTick = Time.time + Mathf.Max(0.1f, tickSeconds);
            Vector2 delta = new(localPlayer.position.x - Center.x, localPlayer.position.z - Center.z);
            if (delta.sqrMagnitude <= radius * radius)
                localVitals.ApplyDamage(damagePerSecond * tickSeconds);
        }

        private void PickCenter()
        {
            string matchId = MatchRoomState.HasMatch ? MatchRoomState.MatchId : "offline";
            var random = new System.Random(StableHash(matchId + ":hot-zone"));
            float edge = Mathf.Max(0f, worldHalfExtent - radius - 80f);
            Center = new Vector3(
                Mathf.Lerp(-edge, edge, (float)random.NextDouble()),
                0f,
                Mathf.Lerp(-edge, edge, (float)random.NextDouble()));
        }

        private void RefreshVisual()
        {
            if (zoneVisual == null) return;
            zoneVisual.gameObject.SetActive(IsActive);
            zoneVisual.position = Center;
            float diameter = radius * 2f;
            zoneVisual.localScale = new Vector3(diameter, zoneVisual.localScale.y, diameter);
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in value ?? string.Empty) hash = hash * 31 + c;
                return hash;
            }
        }
    }
}
