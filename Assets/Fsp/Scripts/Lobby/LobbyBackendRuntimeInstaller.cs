using Fsp.Backend;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>Ensures generated Lobby scenes have squad and profile backend services.</summary>
    public static class LobbyBackendRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;

            GameObject host = GameObject.Find("LobbyBackendRuntime");
            if (host == null) host = new GameObject("LobbyBackendRuntime");

            SupabaseSquadClient squad = host.GetComponent<SupabaseSquadClient>();
            if (squad == null) squad = host.AddComponent<SupabaseSquadClient>();

            SupabaseMatchmakingClient matchmaking = host.GetComponent<SupabaseMatchmakingClient>();
            if (matchmaking == null) matchmaking = host.AddComponent<SupabaseMatchmakingClient>();

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
        }
    }
}
