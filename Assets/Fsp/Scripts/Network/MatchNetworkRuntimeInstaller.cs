using System;
using Fsp.Backend;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    /// <summary>
    /// Installs the online Match transport/session at runtime so the checked-in scene does not
    /// depend on manual inspector wiring. It only activates for an authenticated matched player.
    /// </summary>
    public static class MatchNetworkRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad() => EnsureInstalled();

        public static bool EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return false;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return false;

            MatchParticipant local = FindLocalParticipant();
            if (local == null)
            {
                // MatchSceneAssembler calls us again after it has guaranteed the local participant,
                // so RuntimeInitialize ordering cannot permanently skip networking.
                Debug.LogWarning("FSP Network installer: local MatchParticipant is not ready yet; install will retry after match assembly.");
                return false;
            }

            CloudflareWebSocketTransport transport = UnityEngine.Object.FindFirstObjectByType<CloudflareWebSocketTransport>();
            if (transport == null)
                transport = new GameObject("CloudflareMatchTransport").AddComponent<CloudflareWebSocketTransport>();

            NetworkSessionManager session = UnityEngine.Object.FindFirstObjectByType<NetworkSessionManager>();
            if (session == null)
                session = new GameObject("NetworkSessionManager").AddComponent<NetworkSessionManager>();

            GameObject remotePrefab = Resources.Load<GameObject>("Network/RemotePlayer");
            session.ConfigureRuntime(transport, local.transform, remotePrefab);

            MatchNetworkRuntimeConfigBootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<MatchNetworkRuntimeConfigBootstrap>();
            if (bootstrap == null)
                bootstrap = new GameObject("MatchNetworkRuntimeConfig").AddComponent<MatchNetworkRuntimeConfigBootstrap>();
            bootstrap.Configure(transport, session);

            if (remotePrefab == null)
                Debug.Log("FSP Network installer: dedicated RemotePlayer prefab not found; runtime visual clone fallback is enabled.");

            return true;
        }

        private static MatchParticipant FindLocalParticipant()
        {
            foreach (MatchParticipant participant in UnityEngine.Object.FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsLocalPlayer) return participant;
            return null;
        }
    }
}
