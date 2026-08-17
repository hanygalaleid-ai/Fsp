using Fsp.Backend;
using Fsp.Combat;
using UnityEngine;

namespace Fsp.Networking
{
    [RequireComponent(typeof(HitscanWeapon))]
    public sealed class NetworkWeaponRelay : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private int weaponSlot;

        private INetworkTransport transport;
        private HitscanWeapon weapon;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            weapon = GetComponent<HitscanWeapon>();
        }

        private void OnEnable()
        {
            if (weapon == null) return;
            weapon.ShotFired += HandleShot;
            weapon.NetworkPlayerHit += HandleNetworkHit;
        }

        private void OnDisable()
        {
            if (weapon == null) return;
            weapon.ShotFired -= HandleShot;
            weapon.NetworkPlayerHit -= HandleNetworkHit;
        }

        private void HandleShot(Vector3 origin, Vector3 direction)
        {
            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn) return;
            transport.SendFire(new NetworkFireEvent
            {
                playerId = SupabaseSession.UserId,
                origin = origin,
                direction = direction.normalized,
                weaponSlot = weaponSlot,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
        }

        private void HandleNetworkHit(string targetId, float damage, Vector3 point)
        {
            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn) return;
            transport.SendDamage(new NetworkDamageEvent
            {
                attackerId = SupabaseSession.UserId,
                targetId = targetId,
                damage = damage,
                hitPoint = point,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
        }
    }
}
