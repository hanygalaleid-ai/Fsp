using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>
    /// Logic-only release bootstrap for the fixed lobby scene.
    /// It never creates, replaces, recolors, or restyles lobby visuals.
    /// </summary>
    public static class LobbyRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLobbyRuntime()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;

            EnsureState();
            GameObject host = GameObject.Find("LobbyRuntime") ?? new GameObject("LobbyRuntime");
            if (host.GetComponent<LobbyController>() == null) host.AddComponent<LobbyController>();
            if (host.GetComponent<LobbyMatchLauncher>() == null) host.AddComponent<LobbyMatchLauncher>();
            if (host.GetComponent<FixedLobbyStartHitRegion>() == null) host.AddComponent<FixedLobbyStartHitRegion>();
        }

        private static void EnsureState()
        {
            if (LobbyState.Instance != null) return;
            GameObject stateObject = GameObject.Find("LobbyState") ?? new GameObject("LobbyState");
            if (stateObject.GetComponent<LobbyState>() == null) stateObject.AddComponent<LobbyState>();
        }

        /// <summary>
        /// Makes the START area of the baked lobby artwork functional without generating UI.
        /// Coordinates are normalized screen coordinates, origin at bottom-left.
        /// </summary>
        private sealed class FixedLobbyStartHitRegion : MonoBehaviour
        {
            private bool loading;

            private void Update()
            {
                if (loading || Screen.width <= 0 || Screen.height <= 0) return;

                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended) TryStart(touch.position);
                }

#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetMouseButtonUp(0)) TryStart(Input.mousePosition);
#endif
            }

            private void TryStart(Vector2 pixelPosition)
            {
                float x = pixelPosition.x / Screen.width;
                float y = pixelPosition.y / Screen.height;

                // The gold START button in the checked-in lobby artwork.
                if (x < 0.79f || x > 0.985f || y < 0.055f || y > 0.205f) return;

                loading = true;
                LobbyState state = LobbyState.Instance;
                if (state != null)
                {
                    if (string.IsNullOrWhiteSpace(state.DisplayName)) state.SetDisplayName("Player");
                    state.RequestStartMatch();
                    return;
                }

                if (Application.CanStreamedLevelBeLoaded("Match"))
                    SceneManager.LoadScene("Match", LoadSceneMode.Single);
                else
                    loading = false;
            }
        }
    }
}
