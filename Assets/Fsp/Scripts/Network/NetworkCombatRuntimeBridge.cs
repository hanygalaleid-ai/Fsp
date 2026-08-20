using System;
using Fsp.Backend;
using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Inventory;
using Fsp.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    public sealed class NetworkCombatRuntimeBridge : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerVitals vitals;

        private INetworkTransport transport;
        private HitscanWeapon primary;
        private HitscanWeapon secondary;
        private bool wired;

        private void Awake()
        {
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
            EnsureLocalIdentity();
            if (GetComponent<NetworkDamageReceiver>() == null)
                gameObject.AddComponent<NetworkDamageReceiver>();
        }

        private void Start() => TryWire();

        private void Update()
        {
            if (!wired) TryWire();
        }

        private void TryWire()
        {
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch || inventory == null) return;
            transport = FindTransport();
            if (transport == null) return;

            RebindWeapons();
            wired = primary != null || secondary != null;
        }

        private INetworkTransport FindTransport()
        {
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (behaviour is INetworkTransport candidate) return candidate;
            return null;
        }

        private void RebindWeapons()
        {
            HitscanWeapon nextPrimary = inventory.PrimaryWeapon;
            HitscanWeapon nextSecondary = inventory.SecondaryWeapon;
            if (nextPrimary == primary && nextSecondary == secondary) return;

            UnbindWeapon(primary);
            if (secondary != primary) UnbindWeapon(secondary);

            primary = nextPrimary;
            secondary = nextSecondary;

            BindWeapon(primary);
            if (secondary != primary) BindWeapon(secondary);
        }

        private void BindWeapon(HitscanWeapon weapon)
        {
            if (weapon == null) return;
            weapon.ShotFired += OnShotFired;
            weapon.NetworkPlayerHit += OnNetworkPlayerHit;
        }

        private void UnbindWeapon(HitscanWeapon weapon)
        {
            if (weapon == null) return;
            weapon.ShotFired -= OnShotFired;
            weapon.NetworkPlayerHit -= OnNetworkPlayerHit;
        }

        private void OnShotFired(Vector3 origin, Vector3 direction)
        {
            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return;
            int slot = inventory != null && inventory.ActiveWeapon == inventory.SecondaryWeapon ? 1 : 0;
            transport.SendFire(new NetworkFireEvent
            {
                playerId = SupabaseSession.UserId,
                origin = origin,
                direction = direction.normalized,
                weaponSlot = slot,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            });
        }

        private void OnNetworkPlayerHit(string targetId, float damage, Vector3 hitPoint)
        {
            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return;
            if (string.IsNullOrWhiteSpace(targetId) || targetId == SupabaseSession.UserId) return;

            transport.SendDamage(new NetworkDamageEvent
            {
                attackerId = SupabaseSession.UserId,
                targetId = targetId,
                damage = Mathf.Clamp(damage, 0f, 200f),
                hitPoint = hitPoint,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            });
        }

        private void EnsureLocalIdentity()
        {
            NetworkPlayerIdentity identity = GetComponent<NetworkPlayerIdentity>();
            if (identity == null) identity = gameObject.AddComponent<NetworkPlayerIdentity>();
            identity.Initialize(SupabaseSession.UserId, true);
        }

        private void OnDestroy()
        {
            UnbindWeapon(primary);
            if (secondary != primary) UnbindWeapon(secondary);
        }
    }

    public static class NetworkCombatRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad() => EnsureInstalled();

        public static bool EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return false;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return false;

            foreach (MatchParticipant participant in UnityEngine.Object.FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                if (participant.GetComponent<NetworkCombatRuntimeBridge>() == null)
                    participant.gameObject.AddComponent<NetworkCombatRuntimeBridge>();
                return true;
            }

            return false;
        }
    }
}
