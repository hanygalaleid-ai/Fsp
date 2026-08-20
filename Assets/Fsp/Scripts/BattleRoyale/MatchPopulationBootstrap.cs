using System.Collections;
using Fsp.Backend;
using Fsp.Bots;
using Fsp.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.BattleRoyale
{
    public sealed class MatchPopulationBootstrap : MonoBehaviour
    {
        [SerializeField] private BotSpawner botSpawner;
        [SerializeField, Min(1)] private int fallbackHumanPlayers = 1;
        [SerializeField, Min(1f)] private float authorityWaitSeconds = 12f;
        [SerializeField, Min(1)] private int fallbackBotsPerFrame = 2;

        private INetworkTransport transport;
        private string authorityPlayerId = string.Empty;
        private bool spawned;

        public static bool EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", System.StringComparison.OrdinalIgnoreCase)) return false;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return false;

            MatchPopulationBootstrap existing = FindFirstObjectByType<MatchPopulationBootstrap>();
            if (existing != null) return true;

            GameObject host = GameObject.Find("MatchPopulationBootstrap") ?? new GameObject("MatchPopulationBootstrap");
            BotSpawner spawner = FindFirstObjectByType<BotSpawner>();
            if (spawner == null)
            {
                GameObject spawnerObject = GameObject.Find("RuntimeOnlineBotSpawner") ?? new GameObject("RuntimeOnlineBotSpawner");
                spawner = spawnerObject.GetComponent<BotSpawner>();
                if (spawner == null) spawner = spawnerObject.AddComponent<BotSpawner>();
            }

            MatchPopulationBootstrap bootstrap = host.AddComponent<MatchPopulationBootstrap>();
            bootstrap.botSpawner = spawner;
            return true;
        }

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
            if (transport is CloudflareWebSocketTransport cloudflare && cloudflare.LastBotAuthority != null)
                HandleAuthority(cloudflare.LastBotAuthority);

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
            spawned = true;
            StartCoroutine(SpawnAuthorityPopulation(humans));
        }

        private IEnumerator SpawnAuthorityPopulation(int humans)
        {
            if (botSpawner == null) yield break;
            botSpawner.RemoveDestroyedBots();
            int botsNeeded = Mathf.Max(0, botSpawner.EffectiveTargetPopulation - Mathf.Max(0, humans));
            int createdThisFrame = 0;

            while (botSpawner.SpawnedCount < botsNeeded)
            {
                if (!botSpawner.TrySpawnOne()) break;
                createdThisFrame++;
                if (createdThisFrame >= Mathf.Max(1, fallbackBotsPerFrame))
                {
                    createdThisFrame = 0;
                    yield return null;
                }
            }

            Debug.Log($"FSP Bots: this client is bot authority; population ready for {humans} human player(s), {botSpawner.SpawnedCount} bot(s), target {botSpawner.EffectiveTargetPopulation} total actors.");
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
