using System.Collections.Generic;
using Fsp.Backend;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkSessionManager : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private Transform localPlayer;
        [SerializeField] private PlayerVitals localVitals;
        [SerializeField] private GameObject remotePlayerPrefab;
        [SerializeField, Min(1f)] private float snapshotRate = 12f;

        private INetworkTransport transport;
        private readonly Dictionary<string, RemotePlayerProxy> remotes = new();
        private float nextSnapshotTime;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (localPlayer != null && localVitals == null)
                localVitals = localPlayer.GetComponent<PlayerVitals>();
        }

        private void Start()
        {
            if (transport == null || !SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return;
            transport.SnapshotReceived += HandleSnapshot;
            transport.Connect(MatchRoomState.MatchId, SupabaseSession.UserId);
        }

        private void Update()
        {
            if (transport == null || !transport.IsConnected || localPlayer == null || Time.time < nextSnapshotTime) return;
            nextSnapshotTime = Time.time + 1f / Mathf.Max(1f, snapshotRate);

            transport.SendSnapshot(new NetworkPlayerSnapshot
            {
                playerId = SupabaseSession.UserId,
                matchId = MatchRoomState.MatchId,
                position = localPlayer.position,
                rotation = localPlayer.rotation,
                health = localVitals != null ? localVitals.Health : 100f,
                armor = localVitals != null ? localVitals.Armor : 0f,
                alive = localVitals == null || localVitals.IsAlive,
                sentAt = Time.realtimeSinceStartupAsDouble
            });
        }

        private void HandleSnapshot(NetworkPlayerSnapshot snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot.playerId) || snapshot.playerId == SupabaseSession.UserId) return;
            if (!remotes.TryGetValue(snapshot.playerId, out var proxy) || proxy == null)
            {
                if (remotePlayerPrefab == null) return;
                var go = Instantiate(remotePlayerPrefab, snapshot.position, snapshot.rotation);
                proxy = go.GetComponent<RemotePlayerProxy>();
                if (proxy == null) proxy = go.AddComponent<RemotePlayerProxy>();
                proxy.Initialize(snapshot.playerId);
                remotes[snapshot.playerId] = proxy;
            }
            proxy.Apply(snapshot);
        }

        private void OnDestroy()
        {
            if (transport != null)
            {
                transport.SnapshotReceived -= HandleSnapshot;
                transport.Disconnect();
            }
        }
    }
}
