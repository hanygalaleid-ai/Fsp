using Fsp.Backend;
using Fsp.Networking;
using Fsp.Player;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class SafeZoneDamageApplier : MonoBehaviour
    {
        [SerializeField] private SafeZoneController zone;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private DropPlanePassenger planePassenger;
        [SerializeField] private float tickSeconds = 0.5f;

        private INetworkTransport transport;
        private float nextTick;

        private void Awake()
        {
            ResolveRuntimeReferences();
            TryResolveTransport();
        }

        private void ResolveRuntimeReferences()
        {
            if (zone == null) zone = FindFirstObjectByType<SafeZoneController>();
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (planePassenger == null) planePassenger = GetComponent<DropPlanePassenger>();
        }

        private void Update()
        {
            ResolveRuntimeReferences();
            if (zone == null || vitals == null || !vitals.IsAlive) return;
            if (matchManager != null && matchManager.Phase != MatchManager.MatchPhase.Active) return;
            if (planePassenger != null && planePassenger.IsAboard) return;
            if (Time.time < nextTick) return;

            nextTick = Time.time + Mathf.Max(0.1f, tickSeconds);
            bool online = SupabaseSession.IsSignedIn && MatchRoomState.HasMatch;
            if (online)
            {
                TryResolveTransport();
                if (transport == null || !transport.IsConnected) return;
                transport.SendZoneProbe(new NetworkZoneProbe
                {
                    playerId = SupabaseSession.UserId,
                    timestamp = Time.realtimeSinceStartupAsDouble
                });
                return;
            }

            float dps = zone.OutsideDamagePerSecond(transform.position);
            if (dps > 0f) vitals.ApplyDamage(dps * tickSeconds);
        }

        private void TryResolveTransport()
        {
            if (transport != null) return;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is INetworkTransport candidate)
                {
                    transport = candidate;
                    return;
                }
            }
        }
    }
}
