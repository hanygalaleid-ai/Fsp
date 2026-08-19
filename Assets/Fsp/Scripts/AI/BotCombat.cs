using System;
using Fsp.Combat;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.AI
{
    public sealed class BotCombat : MonoBehaviour
    {
        [SerializeField] private WeaponConfig weapon;
        [SerializeField] private Transform muzzle;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private float aimErrorDegrees = 2.2f;
        [SerializeField] private float burstMin = 0.18f;
        [SerializeField] private float burstMax = 0.65f;
        [SerializeField] private float burstPauseMin = 0.2f;
        [SerializeField] private float burstPauseMax = 0.75f;

        private float nextShotTime;
        private float burstEndsAt;
        private float nextBurstAt;

        public event Action<BotCombat, string, float, Vector3> NetworkPlayerHit;

        public void ConfigureDifficulty(BotDifficultyProfile profile)
        {
            if (profile == null) return;
            aimErrorDegrees = profile.aimErrorDegrees;
            float aggression = Mathf.Max(0.5f, profile.aggression);
            burstPauseMin = 0.2f / aggression;
            burstPauseMax = 0.75f / aggression;
        }

        public void SetWeapon(WeaponConfig config)
        {
            if (config != null) weapon = config;
        }

        public void TryAttack(Transform target)
        {
            if (target == null || weapon == null || Time.time < nextBurstAt) return;
            if (Time.time > burstEndsAt) burstEndsAt = Time.time + UnityEngine.Random.Range(burstMin, burstMax);
            if (Time.time > burstEndsAt)
            {
                nextBurstAt = Time.time + UnityEngine.Random.Range(burstPauseMin, burstPauseMax);
                return;
            }
            if (Time.time < nextShotTime) return;
            nextShotTime = Time.time + 1f / Mathf.Max(0.01f, weapon.roundsPerSecond);

            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.4f;
            Vector3 aimPoint = target.position + Vector3.up * 1.1f;
            Vector3 direction = (aimPoint - origin).normalized;
            direction = Quaternion.Euler(UnityEngine.Random.Range(-aimErrorDegrees, aimErrorDegrees), UnityEngine.Random.Range(-aimErrorDegrees, aimErrorDegrees), 0f) * direction;

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, weapon.range, hitMask, QueryTriggerInteraction.Ignore)) return;

            NetworkPlayerIdentity identity = hit.collider.GetComponentInParent<NetworkPlayerIdentity>();
            if (identity != null && !identity.IsLocalPlayer && !string.IsNullOrWhiteSpace(identity.PlayerId))
            {
                NetworkPlayerHit?.Invoke(this, identity.PlayerId, weapon.damage, hit.point);
                return;
            }

            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            damageable?.ApplyDamage(weapon.damage, hit.point, hit.normal, gameObject);
        }
    }
}
