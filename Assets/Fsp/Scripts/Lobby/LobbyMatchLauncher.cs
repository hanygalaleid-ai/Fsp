using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    public sealed class LobbyMatchLauncher : MonoBehaviour
    {
        [SerializeField] private string battleSceneName = "BattleRoyale";

        private void OnEnable()
        {
            if (LobbyState.Instance != null)
                LobbyState.Instance.StartRequested += HandleStartRequested;
        }

        private void Start()
        {
            if (LobbyState.Instance != null)
                LobbyState.Instance.StartRequested += HandleStartRequested;
        }

        private void OnDisable()
        {
            if (LobbyState.Instance != null)
                LobbyState.Instance.StartRequested -= HandleStartRequested;
        }

        private void HandleStartRequested()
        {
            if (string.IsNullOrWhiteSpace(battleSceneName)) return;
            SceneManager.LoadScene(battleSceneName);
        }
    }
}
