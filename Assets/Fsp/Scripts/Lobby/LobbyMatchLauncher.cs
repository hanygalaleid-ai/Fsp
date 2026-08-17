using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    public sealed class LobbyMatchLauncher : MonoBehaviour
    {
        [SerializeField] private string battleSceneName = "BattleRoyale";
        private bool subscribed;

        private void Update()
        {
            if (!subscribed && LobbyState.Instance != null)
            {
                LobbyState.Instance.StartRequested += HandleStartRequested;
                subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (subscribed && LobbyState.Instance != null)
                LobbyState.Instance.StartRequested -= HandleStartRequested;
            subscribed = false;
        }

        private void HandleStartRequested()
        {
            if (string.IsNullOrWhiteSpace(battleSceneName)) return;
            SceneManager.LoadScene(battleSceneName);
        }
    }
}
