using Fsp.Player;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class SafeZoneDamageApplier : MonoBehaviour
    {
        [SerializeField] private SafeZoneController zone;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private float tickSeconds = 0.5f;
        private float nextTick;

        private void Awake()
        {
            if (zone == null) zone = FindFirstObjectByType<SafeZoneController>();
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
        }

        private void Update()
        {
            if (zone == null || vitals == null || !vitals.IsAlive || Time.time < nextTick) return;
            nextTick = Time.time + Mathf.Max(0.1f, tickSeconds);
            float dps = zone.OutsideDamagePerSecond(transform.position);
            if (dps > 0f) vitals.ApplyDamage(dps * tickSeconds);
        }
    }
}
