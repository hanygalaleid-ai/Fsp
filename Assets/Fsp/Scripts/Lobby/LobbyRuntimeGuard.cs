using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>
    /// Release lobby bootstrap. Ensures functional runtime components and the fixed Sunscar UI only.
    /// No prototype/fallback geometry is allowed in release builds.
    /// </summary>
    public static class LobbyRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLobbyRuntime()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;

            EnsureState();
            GameObject host = GetRuntimeHost();
            if (host.GetComponent<LobbyController>() == null) host.AddComponent<LobbyController>();
            if (host.GetComponent<LobbyMatchLauncher>() == null) host.AddComponent<LobbyMatchLauncher>();

            if (GameObject.Find("LobbyCanvas") == null && UnityEngine.Object.FindFirstObjectByType<StarterLobbyUiInstaller>() == null)
                host.AddComponent<StarterLobbyUiInstaller>();
        }

        private static GameObject GetRuntimeHost()
        {
            GameObject host = GameObject.Find("LobbyRuntime");
            return host != null ? host : new GameObject("LobbyRuntime");
        }

        private static void EnsureState()
        {
            if (LobbyState.Instance != null) return;
            GameObject stateObject = GameObject.Find("LobbyState") ?? new GameObject("LobbyState");
            if (stateObject.GetComponent<LobbyState>() == null) stateObject.AddComponent<LobbyState>();
        }
    }
}
