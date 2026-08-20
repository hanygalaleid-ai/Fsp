using System.Collections.Generic;
using Fsp.Backend;
using Fsp.BattleRoyale;
using Fsp.Bots;
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
        [SerializeField, Min(3f)] private float connectTimeoutSeconds = 10f;

        private INetworkTransport transport;
        private readonly Dictionary<string, RemotePlayerProxy> remotes = new();
        private float nextSnapshotTime;
        private float connectStartedAt;
        private bool started;
        private bool connectionObserved;
        private bool fellBackOffline;

        private void Awake() => AutoWireRuntimeDependencies();

        public void ConfigureRuntime(MonoBehaviour transportSource, Transform player, GameObject remotePrefab)
        {
            if (transportSource != null) transportBehaviour = transportSource;
            if (player != null) localPlayer = player;
            if (remotePrefab != null) remotePlayerPrefab = remotePrefab;
            AutoWireRuntimeDependencies();
            TryStartOnlineSession();
        }

        public void RetryStartOnlineSession()
        {
            AutoWireRuntimeDependencies();
            TryStartOnlineSession();
        }

        public void FallbackOffline(string reason)
        {
            AutoWireRuntimeDependencies();
            FallBackToOffline(string.IsNullOrWhiteSpace(reason) ? "online session unavailable" : reason);
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

        private void Start() => TryStartOnlineSession();

        private void TryStartOnlineSession()
        {
            if (started || fellBackOffline) return;
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
            if (transport is CloudflareWebSocketTransport cloudflare && !cloudflare.IsConfigured)
            {
                Debug.Log("FSP Network: waiting for Cloudflare relay runtime configuration.");
                return;
            }
            if (remotePlayerPrefab == null)
                Debug.Log("FSP Network: dedicated remote prefab missing; authored local character visuals will be reused for remote players.");

            started = true;
            connectionObserved = false;
            connectStartedAt = Time.unscaledTime;
            transport.SnapshotReceived -= HandleSnapshot;
            transport.SnapshotReceived += HandleSnapshot;
            transport.EliminationReceived -= HandleElimination;
            transport.EliminationReceived += HandleElimination;
            transport.Connect(MatchRoomState.MatchId, SupabaseSession.UserId);
        }

        private void Update()
        {
            if (started && !connectionObserved && transport != null)
            {
                if (transport.IsConnected)
                {
                    connectionObserved = true;
                    Debug.Log("FSP Network: match relay connected.");
                }
                else if (Time.unscaledTime - connectStartedAt >= connectTimeoutSeconds)
                {
                    FallBackToOffline("match relay connection timed out");
                    return;
                }
            }

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

        private void FallBackToOffline(string reason)
        {
            if (fellBackOffline) return;
            fellBackOffline = true;
            started = false;
            if (transport != null)
            {
                transport.SnapshotReceived -= HandleSnapshot;
                transport.EliminationReceived -= HandleElimination;
                transport.Disconnect();
            }

            ClearRemotePlayers();
            MatchRoomState.Instance?.Clear();
            EnsureOfflineOpponent();

            // NetworkMatchStateBridge turns MatchManager authoritative as soon as an online room is
            // present. Returning to offline mode must also return authority to the local manager,
            // otherwise the fallback bot is registered but countdown/death/end conditions stay disabled.
            MatchManager manager = MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
            manager?.SetNetworkAuthoritative(false);

            Debug.LogWarning("FSP Network: " + reason + "; continuing as an offline playable match.");
        }

        private void EnsureOfflineOpponent()
        {
            if (localPlayer == null) return;
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsBot) return;

            GameObject spawnerObject = GameObject.Find("RuntimeOfflineBotSpawner") ?? new GameObject("RuntimeOfflineBotSpawner");
            BotSpawner spawner = spawnerObject.GetComponent<BotSpawner>();
            if (spawner == null) spawner = spawnerObject.AddComponent<BotSpawner>();
            GameObject spawnObject = GameObject.Find("RuntimeOfflineBotSpawn") ?? new GameObject("RuntimeOfflineBotSpawn");
            Vector3 spawn = localPlayer.position + new Vector3(18f, 0f, 22f);
            spawn.y = Mathf.Max(1f, localPlayer.position.y);
            spawnObject.transform.position = spawn;
            spawner.ConfigureSpawnPoints(new[] { spawnObject.transform });
            spawner.TrySpawnOne();
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
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.playerId) || snapshot.playerId == SupabaseSession.UserId) return;
            if (!remotes.TryGetValue(snapshot.playerId, out var proxy) || proxy == null)
            {
                if (remotePlayerPrefab != null)
                {
                    var go = Instantiate(remotePlayerPrefab, snapshot.position, snapshot.rotation);
                    proxy = go.GetComponent<RemotePlayerProxy>();
                    if (proxy == null) proxy = go.AddComponent<RemotePlayerProxy>();
                }
                else
                {
                    proxy = RemotePlayerRuntimeFactory.CreateFromLocalVisual(localPlayer, snapshot.position, snapshot.rotation);
                }
                if (proxy == null)
                {
                    Debug.LogError("FSP Network: failed to create remote player visual for " + snapshot.playerId);
                    return;
                }
                proxy.Initialize(snapshot.playerId);
                remotes[snapshot.playerId] = proxy;
            }
            proxy.Apply(snapshot);
        }

        private void HandleElimination(NetworkEliminationEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.victimId) || evt.victimId == SupabaseSession.UserId) return;
            if (!remotes.TryGetValue(evt.victimId, out RemotePlayerProxy proxy) || proxy == null) return;
            proxy.MarkEliminated();
        }

        private void ClearRemotePlayers()
        {
            foreach (RemotePlayerProxy proxy in remotes.Values)
                if (proxy != null) Destroy(proxy.gameObject);
            remotes.Clear();
        }

        private void OnDestroy()
        {
            if (transport != null)
            {
                transport.SnapshotReceived -= HandleSnapshot;
                transport.EliminationReceived -= HandleElimination;
                transport.Disconnect();
            }
            ClearRemotePlayers();
        }
    }
}
