using System;
using System.Collections.Generic;
using Fsp.AI;
using Fsp.Backend;
using Fsp.BattleRoyale;
using Fsp.Bots;
using Fsp.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    public sealed class NetworkBotSnapshotPublisher : MonoBehaviour
    {
        [SerializeField, Min(2f)] private float sendRate = 10f;
        [SerializeField, Min(0.1f)] private float botRegistryRefreshSeconds = 0.5f;

        private INetworkTransport transport;
        private bool isAuthority;
        private float nextSend;
        private float nextBotRegistryRefresh;
        private readonly List<MatchParticipant> bots = new();
        private readonly Dictionary<BotCombat, string> botCombatIds = new();
        private readonly Dictionary<FallbackBotAgent, string> fallbackAgentIds = new();

        private void Start() => TryBind();

        private void Update()
        {
            if (transport == null) TryBind();
            if (!isAuthority || transport == null || !transport.IsConnected || Time.unscaledTime < nextSend) return;

            nextSend = Time.unscaledTime + 1f / Mathf.Max(2f, sendRate);
            RefreshBotsIfNeeded();
            for (int i = 0; i < bots.Count; i++)
            {
                MatchParticipant bot = bots[i];
                if (bot == null) continue;
                PlayerVitals vitals = bot.GetComponent<PlayerVitals>();
                string id = $"bot:{i + 1:000}";
                BindBotCombat(bot, id);
                BindFallbackAgent(bot, id);
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
                transport.BotAuthorityReceived -= HandleAuthority;
                transport.BotAuthorityReceived += HandleAuthority;
                transport.DamageReceived -= HandleDamage;
                transport.DamageReceived += HandleDamage;
                if (transport is CloudflareWebSocketTransport cloudflare && cloudflare.LastBotAuthority != null)
                    HandleAuthority(cloudflare.LastBotAuthority);
                return;
            }
        }

        private void HandleAuthority(NetworkBotAuthorityEvent value)
        {
            isAuthority = value != null && value.playerId == SupabaseSession.UserId;
            if (!isAuthority)
            {
                UnbindBotAttackers();
                bots.Clear();
                nextBotRegistryRefresh = 0f;
            }
            else RefreshBotsIfNeeded(true);
        }

        private void HandleDamage(NetworkDamageEvent value)
        {
            if (!isAuthority || value == null || string.IsNullOrWhiteSpace(value.targetId) || !value.targetId.StartsWith("bot:", StringComparison.Ordinal)) return;
            if (!TryResolveBot(value.targetId, out MatchParticipant bot) || bot == null) return;
            PlayerVitals vitals = bot.GetComponent<PlayerVitals>();
            if (vitals == null || !vitals.IsAlive) return;
            vitals.ApplyDamage(Mathf.Clamp(value.damage, 0f, 35f));
        }

        private void BindBotCombat(MatchParticipant bot, string botId)
        {
            BotCombat combat = bot != null ? bot.GetComponent<BotCombat>() : null;
            if (combat == null) return;
            if (!botCombatIds.ContainsKey(combat)) combat.NetworkPlayerHit += HandleBotNetworkHit;
            botCombatIds[combat] = botId;
        }

        private void BindFallbackAgent(MatchParticipant bot, string botId)
        {
            FallbackBotAgent agent = bot != null ? bot.GetComponent<FallbackBotAgent>() : null;
            if (agent == null) return;
            if (!fallbackAgentIds.ContainsKey(agent)) agent.NetworkPlayerHit += HandleFallbackNetworkHit;
            fallbackAgentIds[agent] = botId;
        }

        private void HandleBotNetworkHit(BotCombat combat, string targetId, float damage, Vector3 hitPoint)
        {
            if (combat == null || !botCombatIds.TryGetValue(combat, out string botId)) return;
            SendBotDamage(botId, targetId, damage, hitPoint);
        }

        private void HandleFallbackNetworkHit(FallbackBotAgent agent, string targetId, float damage, Vector3 hitPoint)
        {
            if (agent == null || !fallbackAgentIds.TryGetValue(agent, out string botId)) return;
            SendBotDamage(botId, targetId, damage, hitPoint);
        }

        private void SendBotDamage(string botId, string targetId, float damage, Vector3 hitPoint)
        {
            if (!isAuthority || transport == null || !transport.IsConnected || string.IsNullOrWhiteSpace(botId) || string.IsNullOrWhiteSpace(targetId)) return;
            transport.SendBotDamage(new NetworkDamageEvent
            {
                attackerId = botId,
                targetId = targetId,
                damage = Mathf.Clamp(damage, 0f, 35f),
                hitPoint = hitPoint,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            });
        }

        private bool TryResolveBot(string id, out MatchParticipant bot)
        {
            bot = null;
            if (string.IsNullOrWhiteSpace(id) || id.Length < 5 || !int.TryParse(id.Substring(4), out int ordinal)) return false;
            RefreshBotsIfNeeded();
            int index = ordinal - 1;
            if (index < 0 || index >= bots.Count)
            {
                RefreshBotsIfNeeded(true);
                if (index < 0 || index >= bots.Count) return false;
            }
            bot = bots[index];
            return bot != null;
        }

        private void RefreshBotsIfNeeded(bool force = false)
        {
            if (!force && Time.unscaledTime < nextBotRegistryRefresh) return;
            nextBotRegistryRefresh = Time.unscaledTime + Mathf.Max(0.1f, botRegistryRefreshSeconds);
            bots.Clear();
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsBot) bots.Add(participant);
            bots.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
        }

        private void UnbindBotAttackers()
        {
            foreach (BotCombat combat in botCombatIds.Keys)
                if (combat != null) combat.NetworkPlayerHit -= HandleBotNetworkHit;
            botCombatIds.Clear();

            foreach (FallbackBotAgent agent in fallbackAgentIds.Keys)
                if (agent != null) agent.NetworkPlayerHit -= HandleFallbackNetworkHit;
            fallbackAgentIds.Clear();
        }

        private void OnDestroy()
        {
            UnbindBotAttackers();
            if (transport != null)
            {
                transport.BotAuthorityReceived -= HandleAuthority;
                transport.DamageReceived -= HandleDamage;
            }
        }
    }

    public static class NetworkBotSnapshotPublisherInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad() => EnsureInstalled();

        public static bool EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return false;
            if (!MatchRoomState.HasMatch || !SupabaseSession.IsSignedIn) return false;
            if (UnityEngine.Object.FindFirstObjectByType<NetworkBotSnapshotPublisher>() != null) return true;
            new GameObject("NetworkBotSnapshotPublisher").AddComponent<NetworkBotSnapshotPublisher>();
            return true;
        }
    }
}
