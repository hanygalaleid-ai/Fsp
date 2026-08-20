using System;
using Fsp.Backend;
using Fsp.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>
    /// Release bootstrap for the fixed, checked-in lobby artwork and start interaction.
    /// Keeps the approved artwork intact while providing runtime recovery if a serialized
    /// sprite reference is missing in a device build.
    /// </summary>
    public static class LobbyRuntimeGuard
    {
        private const string FixedLobbyObjectName = "FSP_FIXED_LOBBY_ART";
        private const string FixedLobbyResourcePath = "Lobby/fsp_lobby_final";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLobbyRuntime()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;

            ResetTransientMatchState();
            EnsureState();
            EnsureFixedLobbyArtwork();

            GameObject host = GameObject.Find("LobbyRuntime") ?? new GameObject("LobbyRuntime");
            if (host.GetComponent<LobbyController>() == null) host.AddComponent<LobbyController>();
            if (host.GetComponent<LobbyMatchLauncher>() == null) host.AddComponent<LobbyMatchLauncher>();
            if (host.GetComponent<FixedLobbyStartHitRegion>() == null) host.AddComponent<FixedLobbyStartHitRegion>();
        }

        private static void ResetTransientMatchState()
        {
            // A match can end through victory, death, Android back navigation or an interrupted
            // scene transition. Lobby must always be a clean re-entry point so START works on the
            // second and later matches exactly like it did on the first one.
            if (MatchRoomState.Instance != null && MatchRoomState.HasMatch)
                MatchRoomState.Instance.Clear();

            MobileInputBridge input = MobileInputBridge.Instance;
            if (input != null)
            {
                input.SetMove(Vector2.zero);
                input.SetFire(false);
                input.SetAim(false);
                input.SetSprint(false);
            }
        }

        private static void EnsureState()
        {
            if (LobbyState.Instance != null) return;
            GameObject stateObject = GameObject.Find("LobbyState") ?? new GameObject("LobbyState");
            if (stateObject.GetComponent<LobbyState>() == null) stateObject.AddComponent<LobbyState>();
        }

        private static void EnsureFixedLobbyArtwork()
        {
            GameObject artwork = GameObject.Find(FixedLobbyObjectName);
            SpriteRenderer renderer = artwork != null ? artwork.GetComponent<SpriteRenderer>() : null;

            if (artwork == null)
            {
                artwork = new GameObject(FixedLobbyObjectName);
                artwork.transform.position = Vector3.zero;
                artwork.transform.localScale = new Vector3(1.05f, 1f, 1f);
                renderer = artwork.AddComponent<SpriteRenderer>();
            }
            else if (renderer == null)
            {
                renderer = artwork.AddComponent<SpriteRenderer>();
            }

            if (renderer.sprite != null) return;

            Sprite fallback = Resources.Load<Sprite>(FixedLobbyResourcePath);
            if (fallback == null)
            {
                Debug.LogError("FSP lobby recovery failed: Resources/" + FixedLobbyResourcePath + " sprite was not found in the player build.");
                return;
            }

            renderer.sprite = fallback;
            renderer.color = Color.white;
            renderer.sortingOrder = 0;
            Debug.Log("FSP lobby artwork recovered from Resources at runtime.");
        }

        private sealed class FixedLobbyStartHitRegion : MonoBehaviour
        {
            private bool loading;

            private void Update()
            {
                if (loading || Screen.width <= 0 || Screen.height <= 0) return;

                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    Touch touch = UnityEngine.Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended) TryStart(touch.position);
                }

#if UNITY_EDITOR || UNITY_STANDALONE
                if (UnityEngine.Input.GetMouseButtonUp(0)) TryStart(UnityEngine.Input.mousePosition);
#endif
            }

            private void TryStart(Vector2 pixelPosition)
            {
                float x = pixelPosition.x / Screen.width;
                float y = pixelPosition.y / Screen.height;
                if (x < 0.805f || x > 0.995f || y < 0.025f || y > 0.135f) return;

                loading = true;
                LobbyState state = LobbyState.Instance;
                if (state == null)
                {
                    Debug.LogError("FSP lobby start blocked: LobbyState is unavailable.");
                    loading = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(state.DisplayName)) state.SetDisplayName("Player");

                // Route every START path through LobbyMatchLauncher instead of loading Match here
                // as well. This prevents two independent handlers from attempting LoadScene on the
                // same touch and makes repeated Lobby -> Match -> Lobby cycles deterministic.
                state.RequestStartMatch();
            }
        }
    }
}
