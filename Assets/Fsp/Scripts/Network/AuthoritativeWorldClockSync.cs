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
        private MatchManager matchManager;
        private bool hasClock;
        private double elapsedAtSync;
        private double localRealtimeAtSync;
        private float countdownSeconds = 8f;

        private void Start()
        {
            safeZone = FindFirstObjectByType<SafeZoneController>();
            dropPlane = FindFirstObjectByType<DropPlaneController>();
            airdrops = FindFirstObjectByType<AirdropController>();
            matchManager = FindFirstObjectByType<MatchManager>();
            TryBindTransport();
            if (NetworkWorldStateCache.TryGet(out NetworkWorldState cached)) HandleWorldState(cached);
        }

        private void Update()
        {
            if (transport == null) TryBindTransport();
            if (!hasClock) return;

            double elapsed = elapsedAtSync + (Time.realtimeSinceStartupAsDouble - localRealtimeAtSync);
            float worldElapsed = Mathf.Max(0f, (float)elapsed);
            float gameplayElapsed = Mathf.Max(0f, worldElapsed - countdownSeconds);

            matchManager?.ApplyAuthoritativeClock(worldElapsed, countdownSeconds);
            safeZone?.ApplyAuthoritativeElapsed(gameplayElapsed);
            dropPlane?.ApplyAuthoritativeElapsed(gameplayElapsed);
            airdrops?.ApplyAuthoritativeElapsed(gameplayElapsed);
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
            countdownSeconds = Mathf.Max(0f, state.countdownSeconds);
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
