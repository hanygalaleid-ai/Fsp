using System;
using Fsp.Backend;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>Ensures Lobby scenes have auth, squad, matchmaking, match-room and profile backend services on every entry.</summary>
    public static class LobbyBackendRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad() => EnsureInstalled();

        public static bool EnsureInstalled()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", StringComparison.OrdinalIgnoreCase)) return false;

            EnsurePersistentMatchRoomState();

            GameObject host = GameObject.Find("LobbyBackendRuntime");
            if (host == null) host = new GameObject("LobbyBackendRuntime");

            SupabaseAuthClient auth = host.GetComponent<SupabaseAuthClient>();
            if (auth == null) auth = host.AddComponent<SupabaseAuthClient>();

            SupabaseSessionRuntimeBootstrap sessionBootstrap = host.GetComponent<SupabaseSessionRuntimeBootstrap>();
            if (sessionBootstrap == null) sessionBootstrap = host.AddComponent<SupabaseSessionRuntimeBootstrap>();
            sessionBootstrap.Configure(auth);
            sessionBootstrap.EnsureRestoreStarted();

            SupabaseSquadClient squad = host.GetComponent<SupabaseSquadClient>();
            if (squad == null) squad = host.AddComponent<SupabaseSquadClient>();

            SupabaseMatchmakingClient matchmaking = host.GetComponent<SupabaseMatchmakingClient>();
            if (matchmaking == null) matchmaking = host.AddComponent<SupabaseMatchmakingClient>();

            SupabaseMatchRoomClient matchRoom = host.GetComponent<SupabaseMatchRoomClient>();
            if (matchRoom == null) matchRoom = host.AddComponent<SupabaseMatchRoomClient>();

            SquadLobbyController controller = host.GetComponent<SquadLobbyController>();
            if (controller == null) controller = host.AddComponent<SquadLobbyController>();
            controller.ConfigureRuntime(squad, matchmaking);

            SupabaseProfileStore profileStore = host.GetComponent<SupabaseProfileStore>();
            if (profileStore == null) profileStore = host.AddComponent<SupabaseProfileStore>();

            LobbyProfileSync profileSync = host.GetComponent<LobbyProfileSync>();
            if (profileSync == null) profileSync = host.AddComponent<LobbyProfileSync>();
            profileSync.ConfigureRuntime(profileStore);
            profileSync.LoadRuntimeProfile();

            if (SquadLobbyState.Instance == null)
            {
                GameObject state = new GameObject("SquadLobbyState");
                state.AddComponent<SquadLobbyState>();
            }

            return true;
        }

        private static void EnsurePersistentMatchRoomState()
        {
            if (MatchRoomState.Instance != null) return;
            GameObject state = GameObject.Find("MatchRoomState") ?? new GameObject("MatchRoomState");
            if (state.GetComponent<MatchRoomState>() == null)
                state.AddComponent<MatchRoomState>();
        }
    }
}
