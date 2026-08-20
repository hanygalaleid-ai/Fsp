using System;
using System.Collections.Generic;
using Fsp.AI;
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
        private readonly Dictionary<BotCombat, string> botCombatIds = new();

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
                BindBotCombat(bot, id);
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
                transport.DamageReceived += HandleDamage;
                return;
            }
        }

        private void HandleAuthority(NetworkBotAuthorityEvent value)
        {
            isAuthority = value != null && value.playerId == SupabaseSession.UserId;
            if (!isAuthority)
            {
                UnbindBotCombats();
                bots.Clear();
            }
            else RefreshBots();
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

        private void HandleBotNetworkHit(BotCombat combat, string targetId, float damage, Vector3 hitPoint)
        {
            if (!isAuthority || transport == null || !transport.IsConnected || combat == null || string.IsNullOrWhiteSpace(targetId)) return;
            if (!botCombatIds.TryGetValue(combat, out string botId) || string.IsNullOrWhiteSpace(botId)) return;
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
            RefreshBots();
            int index = ordinal - 1;
            if (index < 0 || index >= bots.Count) return false;
            bot = bots[index];
            return bot != null;
        }

        private void RefreshBots()
        {
            bots.Clear();
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsBot) bots.Add(participant);
            bots.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
        }

        private void UnbindBotCombats()
        {
            foreach (BotCombat combat in botCombatIds.Keys)
                if (combat != null) combat.NetworkPlayerHit -= HandleBotNetworkHit;
            botCombatIds.Clear();
        }

        private void OnDestroy()
        {
            UnbindBotCombats();
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
