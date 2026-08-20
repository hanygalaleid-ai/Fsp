using System.Collections;
using Fsp.Backend;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    public sealed class LobbyMatchLauncher : MonoBehaviour
    {
        [SerializeField] private string battleSceneName = "Match";
        [SerializeField, Min(1f)] private float onlineWaitSeconds = 8f;
        [SerializeField, Min(0.25f)] private float pollIntervalSeconds = 1.5f;
        private bool subscribed;
        private bool loading;

        private void OnEnable() => TrySubscribe();
        private void Update() => TrySubscribe();

        private void TrySubscribe()
        {
            if (subscribed || LobbyState.Instance == null) return;
            LobbyState.Instance.StartRequested += HandleStartRequested;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (subscribed && LobbyState.Instance != null)
                LobbyState.Instance.StartRequested -= HandleStartRequested;
            subscribed = false;
        }

        private void HandleStartRequested()
        {
            if (loading) return;
            loading = true;
            StartCoroutine(StartMatchFlow());
        }

        private IEnumerator StartMatchFlow()
        {
            string target = string.IsNullOrWhiteSpace(battleSceneName) ? "Match" : battleSceneName.Trim();
            if (!Application.CanStreamedLevelBeLoaded(target))
            {
                Debug.LogError("FSP release launch blocked: Match scene is not present in Build Settings: " + target);
                loading = false;
                yield break;
            }

            // Guests and devices without a restored Supabase session must remain fully playable.
            // Signed-in players get a short best-effort online matchmaking window; any backend or
            // connectivity failure falls back to the local match instead of leaving START stuck.
            if (!SupabaseSession.IsSignedIn)
            {
                LoadTarget(target, "offline/guest");
                yield break;
            }

            SupabaseMatchRoomClient client = FindFirstObjectByType<SupabaseMatchRoomClient>();
            if (client == null) client = gameObject.AddComponent<SupabaseMatchRoomClient>();

            bool requestFinished = false;
            bool requestOk = false;
            string requestStatus = string.Empty;
            yield return client.TryMatchmake((ok, status) =>
            {
                requestFinished = true;
                requestOk = ok;
                requestStatus = status ?? string.Empty;
            });

            if (requestFinished && requestOk && MatchRoomState.HasMatch)
            {
                LoadTarget(target, "online matched");
                yield break;
            }

            if (requestFinished && requestOk && requestStatus.StartsWith("waiting", System.StringComparison.OrdinalIgnoreCase))
            {
                float deadline = Time.unscaledTime + Mathf.Max(1f, onlineWaitSeconds);
                while (Time.unscaledTime < deadline)
                {
                    yield return new WaitForSecondsRealtime(Mathf.Max(0.25f, pollIntervalSeconds));

                    bool pollFinished = false;
                    bool pollOk = false;
                    string pollStatus = string.Empty;
                    yield return client.LoadMyMatch((ok, status) =>
                    {
                        pollFinished = true;
                        pollOk = ok;
                        pollStatus = status ?? string.Empty;
                    });

                    if (pollFinished && pollOk && MatchRoomState.HasMatch)
                    {
                        LoadTarget(target, "online matched after wait");
                        yield break;
                    }

                    if (!pollOk && !string.IsNullOrWhiteSpace(pollStatus))
                    {
                        Debug.LogWarning("FSP matchmaking poll failed; continuing toward safe offline fallback: " + pollStatus);
                        break;
                    }
                }
            }
            else if (!requestOk && !string.IsNullOrWhiteSpace(requestStatus))
            {
                Debug.LogWarning("FSP matchmaking unavailable; starting safe offline match instead: " + requestStatus);
            }

            MatchRoomState.Instance?.Clear();
            LoadTarget(target, "offline fallback");
        }

        private void LoadTarget(string target, string mode)
        {
            Debug.Log("FSP loading battle scene: " + target + " (" + mode + ")");
            SceneManager.LoadScene(target, LoadSceneMode.Single);
        }
    }
}
