using System;
using System.Collections.Generic;
using Fsp.Backend;
using Fsp.BattleRoyale;
using Fsp.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    public sealed class NetworkBotSnapshotPublisher : MonoBehaviour
    {
        [SerializeField, Min(2f)] private float sendRate = 10f;

        private INetworkTransport transport;
        private bool isAuthority;
        private float nextSend;
        private readonly List<MatchParticipant> bots = new();

        private void Start() => TryBind();

        private void Update()
        {
            if (transport == null) TryBind();
            if (!isAuthority || transport == null || !transport.IsConnected || Time.unscaledTime < nextSend) return;

            nextSend = Time.unscaledTime + 1f / Mathf.Max(2f, sendRate);
            RefreshBots();
            for (int i = 0; i < bots.Count; i++)
            {
                MatchParticipant bot = bots[i];
                if (bot == null) continue;
                PlayerVitals vitals = bot.GetComponent<PlayerVitals>();
                string id = $"bot:{i + 1:000}";
                transport.SendBotSnapshot(new NetworkPlayerSnapshot
                {
                    playerId = id,
                    matchId = MatchRoomState.MatchId,
                    position = bot.transform.position,
                    rotation = bot.transform.rotation,
                    health = vitals != null ? vitals.Health : 100f,
                    armor = vitals != null ? vitals.Armor : 0f,
                    alive = vitals == null || vitals.IsAlive,
                    dropState = NetworkDropState.Grounded,
                    sentAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
                });
            }
        }

        private void TryBind()
        {
            if (!MatchRoomState.HasMatch || !SupabaseSession.IsSignedIn) return;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transport = candidate;
                transport.BotAuthorityReceived += HandleAuthority;
                return;
            }
        }

        private void HandleAuthority(NetworkBotAuthorityEvent value)
        {
            isAuthority = value != null && value.playerId == SupabaseSession.UserId;
            if (!isAuthority) bots.Clear();
        }

        private void RefreshBots()
        {
            bots.Clear();
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsBot) bots.Add(participant);
            bots.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
        }

        private void OnDestroy()
        {
            if (transport != null) transport.BotAuthorityReceived -= HandleAuthority;
        }
    }

    public static class NetworkBotSnapshotPublisherInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!MatchRoomState.HasMatch || !SupabaseSession.IsSignedIn) return;
            if (UnityEngine.Object.FindFirstObjectByType<NetworkBotSnapshotPublisher>() != null) return;
            new GameObject("NetworkBotSnapshotPublisher").AddComponent<NetworkBotSnapshotPublisher>();
        }
    }
}
