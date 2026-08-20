using System;
using Fsp.Backend;
using Fsp.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>
    /// Release bootstrap for the responsive lobby canvas and start interaction.
    /// Disables the obsolete world-space artwork so it cannot crop or stretch on wide phones.
    /// </summary>
    public static class LobbyRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAfterInitialSceneLoad() => EnsureInstalled();

        public static void EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;

            ResetTransientMatchState();
            EnsureState();
            DisableLegacyLobbyArtwork();
            if (!ProductionLobbyUiInstaller.EnsureInstalled())
                Debug.LogError("FSP lobby UI recovery failed: ProductionLobbyCanvas was not created.");

            GameObject host = GameObject.Find("LobbyRuntime") ?? new GameObject("LobbyRuntime");
            if (host.GetComponent<LobbyController>() == null) host.AddComponent<LobbyController>();
            if (host.GetComponent<LobbyMatchLauncher>() == null) host.AddComponent<LobbyMatchLauncher>();

            // LobbyBackendRuntimeInstaller's original RuntimeInitialize callback is startup-order
            // sensitive and does not by itself guarantee a reinstall after Match -> Lobby. Make
            // the lobby re-entry boundary authoritative for matchmaking/profile/squad services too.
            LobbyBackendRuntimeInstaller.EnsureInstalled();
        }

        private static void ResetTransientMatchState()
        {
            if (MatchRoomState.Instance != null && MatchRoomState.HasMatch)
                MatchRoomState.Instance.Clear();

            MobileInputBridge.Instance?.ResetAll();
        }

        private static void EnsureState()
        {
            if (LobbyState.Instance != null) return;
            GameObject stateObject = GameObject.Find("LobbyState") ?? new GameObject("LobbyState");
            if (stateObject.GetComponent<LobbyState>() == null) stateObject.AddComponent<LobbyState>();
        }

        private static void DisableLegacyLobbyArtwork()
        {
            // Old builds used a world-space SpriteRenderer whose baked START artwork could
            // stretch and crop differently on each phone. The responsive overlay canvas is
            // now the sole owner of the lobby background and controls.
            GameObject artwork = GameObject.Find("FSP_FIXED_LOBBY_ART");
            if (artwork == null) return;
            SpriteRenderer renderer = artwork.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
        }

    }
}
