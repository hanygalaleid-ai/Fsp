using System.Collections.Generic;
using Fsp.Backend;
using Fsp.BattleRoyale;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkSessionManager : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private Transform localPlayer;
        [SerializeField] private PlayerVitals localVitals;
        [SerializeField] private DropPlanePassenger planePassenger;
        [SerializeField] private ParachuteController parachute;
        [SerializeField] private GameObject remotePlayerPrefab;
        [SerializeField, Min(1f)] private float snapshotRate = 12f;

        private INetworkTransport transport;
        private readonly Dictionary<string, RemotePlayerProxy> remotes = new();
        private float nextSnapshotTime;
        private bool started;

        private void Awake()
        {
            AutoWireRuntimeDependencies();
        }

        public void ConfigureRuntime(MonoBehaviour transportSource, Transform player, GameObject remotePrefab)
        {
            if (transportSource != null) transportBehaviour = transportSource;
            if (player != null) localPlayer = player;
            if (remotePrefab != null) remotePlayerPrefab = remotePrefab;
            AutoWireRuntimeDependencies();
            TryStartOnlineSession();
        }

        private void AutoWireRuntimeDependencies()
        {
            transport = transportBehaviour as INetworkTransport;
            if (transport == null)
            {
                var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is INetworkTransport candidate)
                    {
                        transportBehaviour = behaviour;
                        transport = candidate;
                        break;
                    }
                }
            }

            if (localPlayer == null)
            {
                foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                {
                    if (participant != null && participant.IsLocalPlayer)
                    {
                        localPlayer = participant.transform;
                        break;
                    }
                }
            }

            if (localPlayer != null)
            {
                if (localVitals == null) localVitals = localPlayer.GetComponent<PlayerVitals>();
                if (planePassenger == null) planePassenger = localPlayer.GetComponent<DropPlanePassenger>();
                if (parachute == null) parachute = localPlayer.GetComponent<ParachuteController>();
            }
        }

        private void Start()
        {
            TryStartOnlineSession();
        }

        private void TryStartOnlineSession()
        {
            if (started) return;
            if (transport == null)
            {
                Debug.LogError("FSP Network: no INetworkTransport found in Match scene.");
                return;
            }
            if (localPlayer == null)
            {
                Debug.LogError("FSP Network: no local MatchParticipant found in Match scene.");
                return;
            }
            if (!SupabaseSession.IsSignedIn)
            {
                Debug.LogWarning("FSP Network: Supabase session is not signed in; online session will not start.");
                return;
            }
            if (!MatchRoomState.HasMatch)
            {
                Debug.LogWarning("FSP Network: no active MatchRoomState; online session will not start.");
                return;
            }
            if (remotePlayerPrefab == null)
                Debug.LogWarning("FSP Network: remotePlayerPrefab is missing; snapshots will connect but remote players cannot be rendered.");

            started = true;
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
                dropState = ResolveDropState(),
                sentAt = Time.realtimeSinceStartupAsDouble
            });
        }

        private NetworkDropState ResolveDropState()
        {
            if (planePassenger != null && planePassenger.IsAboard) return NetworkDropState.AboardPlane;
            if (parachute != null && parachute.IsActive)
                return parachute.IsOpen ? NetworkDropState.Parachute : NetworkDropState.Freefall;
            return NetworkDropState.Grounded;
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
