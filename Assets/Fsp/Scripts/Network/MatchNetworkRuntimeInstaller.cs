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
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return;

            MatchParticipant local = FindLocalParticipant();
            if (local == null)
            {
                Debug.LogError("FSP Network installer: no local MatchParticipant found.");
                return;
            }

            CloudflareWebSocketTransport transport = UnityEngine.Object.FindFirstObjectByType<CloudflareWebSocketTransport>();
            if (transport == null)
                transport = new GameObject("CloudflareMatchTransport").AddComponent<CloudflareWebSocketTransport>();

            NetworkSessionManager session = UnityEngine.Object.FindFirstObjectByType<NetworkSessionManager>();
            if (session == null)
                session = new GameObject("NetworkSessionManager").AddComponent<NetworkSessionManager>();

            GameObject remotePrefab = Resources.Load<GameObject>("Network/RemotePlayer");
            session.ConfigureRuntime(transport, local.transform, remotePrefab);

            if (remotePrefab == null)
                Debug.LogWarning("FSP Network installer: Resources/Network/RemotePlayer prefab is not present yet. Connection can start, but remote avatars will not render.");
        }

        private static MatchParticipant FindLocalParticipant()
        {
            foreach (MatchParticipant participant in UnityEngine.Object.FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsLocalPlayer) return participant;
            return null;
        }
    }
}
