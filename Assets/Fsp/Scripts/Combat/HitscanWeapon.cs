using System;
using System.Collections;
using Fsp.Inventory;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.Combat
{
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponConfig config;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField, Min(0.1f)] private float reloadDuration = 1.65f;

        private int ammoInMagazine;
        private float nextShotTime;
        private bool reloading;

        public int AmmoInMagazine => ammoInMagazine;
        public bool IsReloading => reloading;
        public event Action<int> AmmoChanged;
        public event Action<Vector3, Vector3> ShotFired;
        public event Action<string, float, Vector3> NetworkPlayerHit;
        public event Action ReloadStarted;
        public event Action ReloadFinished;

        private void Awake()
        {
            ammoInMagazine = config != null ? config.magazineSize : 0;
            if (inventory == null) inventory = GetComponentInParent<PlayerInventory>();
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
            ShotFired?.Invoke(origin, direction);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, config.range, hitMask, QueryTriggerInteraction.Ignore))
            {
                var identity = hit.collider.GetComponentInParent<NetworkPlayerIdentity>();
                if (identity != null && !identity.IsLocalPlayer && !string.IsNullOrWhiteSpace(identity.PlayerId))
                {
                    NetworkPlayerHit?.Invoke(identity.PlayerId, config.damage, hit.point);
                }
                else
                {
                    var damageable = hit.collider.GetComponentInParent<IDamageable>();
                    damageable?.ApplyDamage(config.damage, hit.point, hit.normal, gameObject);
                }
            }

            return true;
        }

        public bool BeginReload()
        {
            if (reloading || config == null || ammoInMagazine >= config.magazineSize || inventory == null) return false;
            if (inventory.GetReserveAmmoForActiveWeapon() <= 0) return false;
            StartCoroutine(ReloadRoutine());
            return true;
        }

        public bool ReloadInstant()
        {
            if (config == null || ammoInMagazine >= config.magazineSize) return false;

            int needed = config.magazineSize - ammoInMagazine;
            int supplied = inventory != null ? inventory.ConsumeReserveAmmoFor(this, needed) : 0;
            if (supplied <= 0) return false;

            ammoInMagazine += supplied;
            AmmoChanged?.Invoke(ammoInMagazine);
            return true;
        }

        private IEnumerator ReloadRoutine()
        {
            reloading = true;
            ReloadStarted?.Invoke();
            yield return new WaitForSeconds(reloadDuration);
            ReloadInstant();
            reloading = false;
            ReloadFinished?.Invoke();
        }
    }

    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector3 point, Vector3 normal, GameObject source);
    }
}
