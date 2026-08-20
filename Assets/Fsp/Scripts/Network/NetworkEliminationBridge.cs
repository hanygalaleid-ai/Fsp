using Fsp.Backend;
using Fsp.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    public sealed class NetworkEliminationBridge : MonoBehaviour
    {
        private INetworkTransport transport;
        private bool subscribed;

        private void Update()
        {
            if (subscribed) return;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transport = candidate;
                transport.EliminationReceived += HandleElimination;
                subscribed = true;
                break;
            }
        }

        private void HandleElimination(NetworkEliminationEvent evt)
        {
            if (evt == null) return;
            KillFeedBus.ReportNetworkElimination(evt.killerId, evt.victimId, SupabaseSession.UserId);
        }

        private void OnDestroy()
        {
            if (subscribed && transport != null)
                transport.EliminationReceived -= HandleElimination;
        }
    }

    public static class NetworkEliminationInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad() => EnsureInstalled();

        public static bool EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", System.StringComparison.OrdinalIgnoreCase)) return false;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return false;
            if (UnityEngine.Object.FindFirstObjectByType<NetworkEliminationBridge>() == null)
                new GameObject("NetworkEliminationBridge").AddComponent<NetworkEliminationBridge>();
            return true;
        }
    }
}
