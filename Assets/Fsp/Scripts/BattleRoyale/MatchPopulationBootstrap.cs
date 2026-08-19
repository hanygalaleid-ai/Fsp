using System.Collections;
using Fsp.Backend;
using Fsp.Bots;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class MatchPopulationBootstrap : MonoBehaviour
    {
        [SerializeField] private BotSpawner botSpawner;
        [SerializeField, Min(1)] private int fallbackHumanPlayers = 1;
        [SerializeField, Min(1f)] private float authorityWaitSeconds = 12f;

        private INetworkTransport transport;
        private string authorityPlayerId = string.Empty;
        private bool spawned;

        private IEnumerator Start()
        {
            if (botSpawner == null) botSpawner = FindFirstObjectByType<BotSpawner>();
            if (botSpawner == null) yield break;

            if (!MatchRoomState.HasMatch)
            {
                botSpawner.FillToTarget(fallbackHumanPlayers);
                spawned = true;
                yield break;
            }

            float deadline = Time.realtimeSinceStartup + authorityWaitSeconds;
            while (transport == null && Time.realtimeSinceStartup < deadline)
            {
                transport = FindTransport();
                if (transport == null) yield return null;
            }

            if (transport == null)
            {
                Debug.LogWarning("FSP Bots: online match has no network transport; bot spawning remains disabled to avoid divergent simulations.");
                yield break;
            }

            transport.BotAuthorityReceived += HandleAuthority;
            while (!spawned && Time.realtimeSinceStartup < deadline)
            {
                TrySpawnIfAuthority();
                yield return null;
            }

            if (!spawned && string.IsNullOrWhiteSpace(authorityPlayerId))
                Debug.LogWarning("FSP Bots: bot authority was not received; no local online bots were spawned.");
        }

        private void HandleAuthority(NetworkBotAuthorityEvent value)
        {
            authorityPlayerId = value != null ? value.playerId ?? string.Empty : string.Empty;
            TrySpawnIfAuthority();
        }

        private void TrySpawnIfAuthority()
        {
            if (spawned || string.IsNullOrWhiteSpace(authorityPlayerId) || !SupabaseSession.IsSignedIn) return;
            if (authorityPlayerId != SupabaseSession.UserId) return;

            int humans = Mathf.Max(1, MatchRoomState.MemberCount);
            botSpawner.FillToTarget(humans);
            spawned = true;
            Debug.Log($"FSP Bots: this client is bot authority; spawned population for {humans} human player(s).");
        }

        private static INetworkTransport FindTransport()
        {
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (behaviour is INetworkTransport candidate) return candidate;
            return null;
        }

        private void OnDestroy()
        {
            if (transport != null) transport.BotAuthorityReceived -= HandleAuthority;
        }
    }
}
