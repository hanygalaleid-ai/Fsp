using System;
using Fsp.Backend;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    public sealed class AuthoritativeWorldClockSync : MonoBehaviour
    {
        private INetworkTransport transport;
        private SafeZoneController safeZone;
        private DropPlaneController dropPlane;
        private AirdropController airdrops;
        private bool hasClock;
        private double elapsedAtSync;
        private double localRealtimeAtSync;

        private void Start()
        {
            safeZone = FindFirstObjectByType<SafeZoneController>();
            dropPlane = FindFirstObjectByType<DropPlaneController>();
            airdrops = FindFirstObjectByType<AirdropController>();
            TryBindTransport();
        }

        private void Update()
        {
            if (transport == null) TryBindTransport();
            if (!hasClock) return;
            double elapsed = elapsedAtSync + (Time.realtimeSinceStartupAsDouble - localRealtimeAtSync);
            float seconds = Mathf.Max(0f, (float)elapsed);
            safeZone?.ApplyAuthoritativeElapsed(seconds);
            dropPlane?.ApplyAuthoritativeElapsed(seconds);
            airdrops?.ApplyAuthoritativeElapsed(seconds);
        }

        private void TryBindTransport()
        {
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                if (transport != null) transport.WorldStateReceived -= HandleWorldState;
                transport = candidate;
                transport.WorldStateReceived += HandleWorldState;
                return;
            }
        }

        private void HandleWorldState(NetworkWorldState state)
        {
            if (state == null || state.startedAt <= 0 || state.serverNow < state.startedAt) return;
            elapsedAtSync = state.serverNow - state.startedAt;
            localRealtimeAtSync = Time.realtimeSinceStartupAsDouble;
            hasClock = true;
        }

        private void OnDestroy()
        {
            if (transport != null) transport.WorldStateReceived -= HandleWorldState;
        }
    }

    public static class AuthoritativeWorldClockInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!MatchRoomState.HasMatch || !SupabaseSession.IsSignedIn) return;
            if (UnityEngine.Object.FindFirstObjectByType<AuthoritativeWorldClockSync>() != null) return;
            new GameObject("AuthoritativeWorldClockSync").AddComponent<AuthoritativeWorldClockSync>();
        }
    }
}
