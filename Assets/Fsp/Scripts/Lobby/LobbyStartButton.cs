using UnityEngine;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    [RequireComponent(typeof(Button))]
    public sealed class LobbyStartButton : MonoBehaviour
    {
        private void Awake()
        {
            Button button = GetComponent<Button>();
            button.onClick.RemoveListener(StartMatch);
            button.onClick.AddListener(StartMatch);
        }

        private void StartMatch()
        {
            LobbyState state = LobbyState.Instance;
            if (state != null)
            {
                if (string.IsNullOrWhiteSpace(state.DisplayName)) state.SetDisplayName("Player");
                state.RequestStartMatch();
                return;
            }

            if (Application.CanStreamedLevelBeLoaded("Match"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
        }
    }
}
