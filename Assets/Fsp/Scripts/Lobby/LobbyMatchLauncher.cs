using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    public sealed class LobbyMatchLauncher : MonoBehaviour
    {
        [SerializeField] private string battleSceneName = "Match";
        private bool subscribed;
        private bool loading;

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Update()
        {
            TrySubscribe();
        }

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

            string target = string.IsNullOrWhiteSpace(battleSceneName) ? "Match" : battleSceneName.Trim();
            if (!Application.CanStreamedLevelBeLoaded(target))
            {
                Debug.LogError("FSP release launch blocked: Match scene is not present in Build Settings: " + target);
                loading = false;
                return;
            }

            Debug.Log("FSP loading battle scene: " + target);
            SceneManager.LoadScene(target, LoadSceneMode.Single);
        }
    }
}
