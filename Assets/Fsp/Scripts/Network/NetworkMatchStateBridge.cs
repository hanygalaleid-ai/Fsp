using System;
using Fsp.Backend;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    public sealed class NetworkMatchStateBridge : MonoBehaviour
    {
        private INetworkTransport transport;
        private MatchManager manager;
        private bool subscribed;

        private void Awake()
        {
            manager = MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
            if (manager != null) manager.SetNetworkAuthoritative(true);
        }

        private void Update()
        {
            if (subscribed) return;
            if (manager == null)
            {
                manager = MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
                if (manager != null) manager.SetNetworkAuthoritative(true);
            }

            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transport = candidate;
                transport.MatchStateReceived += HandleMatchState;
                subscribed = true;
                break;
            }
        }

        private void HandleMatchState(NetworkMatchState state)
        {
            if (state == null || manager == null) return;
            manager.ApplyAuthoritativeState(state.aliveCount, state.totalCount, state.winnerId, state.finished);
        }

        private void OnDestroy()
        {
            if (subscribed && transport != null)
                transport.MatchStateReceived -= HandleMatchState;
        }
    }

    public static class NetworkMatchStateInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return;
            MatchManager manager = MatchManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<MatchManager>();
            manager?.SetNetworkAuthoritative(true);
            if (UnityEngine.Object.FindFirstObjectByType<NetworkMatchStateBridge>() == null)
                new GameObject("NetworkMatchStateBridge").AddComponent<NetworkMatchStateBridge>();
        }
    }
}
