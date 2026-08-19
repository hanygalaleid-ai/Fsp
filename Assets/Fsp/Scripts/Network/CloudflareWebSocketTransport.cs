using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class CloudflareWebSocketTransport : MonoBehaviour, INetworkTransport
    {
        [Serializable] private sealed class Envelope { public string type; public string payload; }
        [SerializeField] private string relayBaseUrl = "wss://YOUR_MATCH_RELAY.workers.dev/ws";

        private readonly ConcurrentQueue<Action> mainThread = new();
        private readonly SemaphoreSlim sendLock = new(1, 1);
        private ClientWebSocket socket;
        private CancellationTokenSource lifetime;

        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(relayBaseUrl) &&
            relayBaseUrl.StartsWith("wss://", StringComparison.OrdinalIgnoreCase) &&
            !relayBaseUrl.Contains("YOUR_MATCH_RELAY", StringComparison.OrdinalIgnoreCase);
        public string RelayBaseUrl => relayBaseUrl;

        public event Action<NetworkPlayerSnapshot> SnapshotReceived;
        public event Action<NetworkFireEvent> FireReceived;
        public event Action<NetworkDamageEvent> DamageReceived;
        public event Action<NetworkVehicleSnapshot> VehicleReceived;
        public event Action<NetworkSeatEvent> SeatReceived;
        public event Action<NetworkLootClaimEvent> LootClaimReceived;
        public event Action<NetworkAppearanceEvent> AppearanceReceived;

        private void Update() { while (mainThread.TryDequeue(out var action)) action?.Invoke(); }

        public async void Connect(string matchId, string playerId)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.LogError("CloudflareWebSocketTransport: WebGL is not supported by ClientWebSocket in this build.");
            return;
#else
            Disconnect();
            if (!IsConfigured)
            {
                Debug.LogError("CloudflareWebSocketTransport: relay URL is not configured. Set the deployed fsp-match-relay wss:// URL before online testing.");
                return;
            }
            if (string.IsNullOrWhiteSpace(matchId) || !SupabaseSession.IsSignedIn) return;
            lifetime = new CancellationTokenSource();
            socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            string url = relayBaseUrl.TrimEnd('/') + "?matchId=" + Uri.EscapeDataString(matchId);
            try { await socket.ConnectAsync(new Uri(url), lifetime.Token); _ = ReceiveLoop(lifetime.Token); }
            catch (Exception e) { mainThread.Enqueue(() => Debug.LogError("Match relay connection failed: " + e.Message)); Disconnect(); }
#endif
        }

        public async void Disconnect()
        {
            try
            {
                lifetime?.Cancel();
                if (socket != null && socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "leave", CancellationToken.None);
            }
            catch { }
            finally
            {
                socket?.Dispose(); socket = null;
                lifetime?.Dispose(); lifetime = null;
            }
        }

        public void SendSnapshot(NetworkPlayerSnapshot v) => Send("snapshot", JsonUtility.ToJson(v));
        public void SendFire(NetworkFireEvent v) => Send("fire", JsonUtility.ToJson(v));
        public void SendDamage(NetworkDamageEvent v) => Send("damage", JsonUtility.ToJson(v));
        public void SendVehicle(NetworkVehicleSnapshot v) => Send("vehicle", JsonUtility.ToJson(v));
        public void SendSeat(NetworkSeatEvent v) => Send("seat", JsonUtility.ToJson(v));
        public void SendLootClaim(NetworkLootClaimEvent v) => Send("loot_claim", JsonUtility.ToJson(v));
        public void SendAppearance(NetworkAppearanceEvent v) => Send("appearance", JsonUtility.ToJson(v));

        private async void Send(string type, string payload)
        {
            if (!IsConnected) return;
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Envelope { type = type, payload = payload }));
            if (bytes.Length > 12 * 1024) return;
            await sendLock.WaitAsync();
            try { if (IsConnected) await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, lifetime.Token); }
            catch (Exception e) { mainThread.Enqueue(() => Debug.LogWarning("Match relay send failed: " + e.Message)); }
            finally { sendLock.Release(); }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[16 * 1024];
            var builder = new StringBuilder();
            try
            {
                while (!token.IsCancellationRequested && socket != null && socket.State == WebSocketState.Open)
                {
                    builder.Clear();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close) return;
                        builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        if (builder.Length > 16 * 1024) return;
                    } while (!result.EndOfMessage);
                    string json = builder.ToString();
                    mainThread.Enqueue(() => Dispatch(json));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { mainThread.Enqueue(() => Debug.LogWarning("Match relay receive stopped: " + e.Message)); }
        }

        private void Dispatch(string json)
        {
            var envelope = JsonUtility.FromJson<Envelope>(json);
            if (envelope == null || string.IsNullOrWhiteSpace(envelope.type)) return;
            switch (envelope.type)
            {
                case "snapshot": SnapshotReceived?.Invoke(JsonUtility.FromJson<NetworkPlayerSnapshot>(envelope.payload)); break;
                case "fire": FireReceived?.Invoke(JsonUtility.FromJson<NetworkFireEvent>(envelope.payload)); break;
                case "damage": DamageReceived?.Invoke(JsonUtility.FromJson<NetworkDamageEvent>(envelope.payload)); break;
                case "vehicle": VehicleReceived?.Invoke(JsonUtility.FromJson<NetworkVehicleSnapshot>(envelope.payload)); break;
                case "seat": SeatReceived?.Invoke(JsonUtility.FromJson<NetworkSeatEvent>(envelope.payload)); break;
                case "loot_claimed": LootClaimReceived?.Invoke(JsonUtility.FromJson<NetworkLootClaimEvent>(envelope.payload)); break;
                case "appearance": AppearanceReceived?.Invoke(JsonUtility.FromJson<NetworkAppearanceEvent>(envelope.payload)); break;
            }
        }

        private void OnDestroy() => Disconnect();
    }
}
