using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    public sealed class LobbyController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Text modeLabel;
        [SerializeField] private TMP_Text characterLabel;
        [SerializeField] private Button startButton;
        [SerializeField] private string[] characterIds = { "soldier_01", "soldier_02", "soldier_03" };

        private int characterIndex;

        private void Start()
        {
            var state = LobbyState.Instance;
            if (state == null) return;

            if (nameInput != null)
            {
                nameInput.text = state.DisplayName;
                nameInput.onValueChanged.AddListener(state.SetDisplayName);
            }

            characterIndex = FindCharacterIndex(state.SelectedCharacterId);
            Refresh();
            state.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (LobbyState.Instance != null)
                LobbyState.Instance.Changed -= Refresh;
        }

        public void SelectSolo()
        {
            LobbyState.Instance?.SetMode(MatchMode.Solo);
        }

        public void SelectSquad()
        {
            LobbyState.Instance?.SetMode(MatchMode.Squad);
        }

        public void NextCharacter()
        {
            if (characterIds == null || characterIds.Length == 0) return;
            characterIndex = (characterIndex + 1) % characterIds.Length;
            LobbyState.Instance?.SetCharacter(characterIds[characterIndex]);
        }

        public void PreviousCharacter()
        {
            if (characterIds == null || characterIds.Length == 0) return;
            characterIndex = (characterIndex - 1 + characterIds.Length) % characterIds.Length;
            LobbyState.Instance?.SetCharacter(characterIds[characterIndex]);
        }

        public void StartMatch()
        {
            var state = LobbyState.Instance;
            if (state == null || string.IsNullOrWhiteSpace(state.DisplayName)) return;
            state.RequestStartMatch();
        }

        private int FindCharacterIndex(string id)
        {
            if (characterIds == null) return 0;
            for (int i = 0; i < characterIds.Length; i++)
                if (characterIds[i] == id) return i;
            return 0;
        }

        private void Refresh()
        {
            var state = LobbyState.Instance;
            if (state == null) return;

            if (modeLabel != null)
                modeLabel.text = state.SelectedMode == MatchMode.Solo ? "Solo" : "Squad";

            if (characterLabel != null)
                characterLabel.text = state.SelectedCharacterId;

            if (startButton != null)
                startButton.interactable = !string.IsNullOrWhiteSpace(state.DisplayName);
        }
    }
}
