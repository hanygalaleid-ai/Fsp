using System;
using UnityEngine;

namespace Fsp.Lobby
{
    public enum MatchMode
    {
        Solo = 0,
        Squad = 1
    }

    public sealed class LobbyState : MonoBehaviour
    {
        public static LobbyState Instance { get; private set; }

        [SerializeField] private MatchMode selectedMode = MatchMode.Solo;
        [SerializeField] private string selectedCharacterId = "soldier_01";
        [SerializeField] private string displayName = "Player";

        public MatchMode SelectedMode => selectedMode;
        public string SelectedCharacterId => selectedCharacterId;
        public string DisplayName => displayName;

        public event Action Changed;
        public event Action StartRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetMode(MatchMode mode)
        {
            selectedMode = mode;
            Changed?.Invoke();
        }

        public void SetDisplayName(string value)
        {
            string trimmed = string.IsNullOrWhiteSpace(value) ? "Player" : value.Trim();
            displayName = trimmed.Length > 18 ? trimmed.Substring(0, 18) : trimmed;
            Changed?.Invoke();
        }

        public void SetCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)) return;
            selectedCharacterId = characterId;
            Changed?.Invoke();
        }

        public void RequestStartMatch()
        {
            StartRequested?.Invoke();
        }
    }
}
