using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>
    /// Logic-only release bootstrap for the fixed, checked-in lobby artwork.
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
        /// Makes only the baked START button area functional. No UI objects are generated.
        /// Coordinates are normalized screen coordinates, origin at bottom-left, matched to
        /// Assets/Fsp/Art/Resources/Lobby/fsp_lobby_final.jpg.
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

                // Gold START button baked into the approved lobby artwork.
                if (x < 0.805f || x > 0.995f || y < 0.025f || y > 0.135f) return;

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
