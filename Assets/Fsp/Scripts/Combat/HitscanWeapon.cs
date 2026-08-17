using System;
using UnityEngine;

namespace Fsp.Combat
{
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponConfig config;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private LayerMask hitMask = ~0;

        private int ammoInMagazine;
        private float nextShotTime;
        private bool reloading;

        public int AmmoInMagazine => ammoInMagazine;
        public bool IsReloading => reloading;
        public event Action<int> AmmoChanged;

        private void Awake()
        {
            ammoInMagazine = config != null ? config.magazineSize : 0;
        }

        public bool TryFire()
        {
            if (config == null || aimCamera == null || reloading || Time.time < nextShotTime || ammoInMagazine <= 0)
                return false;

            ammoInMagazine--;
            AmmoChanged?.Invoke(ammoInMagazine);
            nextShotTime = Time.time + (1f / Mathf.Max(0.01f, config.roundsPerSecond));

            Vector3 direction = aimCamera.transform.forward;
            direction = Quaternion.Euler(
                UnityEngine.Random.Range(-config.spreadDegrees, config.spreadDegrees),
                UnityEngine.Random.Range(-config.spreadDegrees, config.spreadDegrees),
                0f) * direction;

            Vector3 origin = aimCamera.transform.position;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, config.range, hitMask, QueryTriggerInteraction.Ignore))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.ApplyDamage(config.damage, hit.point, hit.normal, gameObject);
            }

            return true;
        }

        public void ReloadInstant()
        {
            if (config == null) return;
            ammoInMagazine = config.magazineSize;
            reloading = false;
            AmmoChanged?.Invoke(ammoInMagazine);
        }
    }

    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector3 point, Vector3 normal, GameObject source);
    }
}
