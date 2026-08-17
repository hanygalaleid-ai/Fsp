using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    public sealed class MatchResultsPanel : MonoBehaviour
    {
        [SerializeField] private LocalMatchResultController results;
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Text placementText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text statusText;
        [SerializeField] private string lobbySceneName = "Lobby";

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (results != null) results.ResultReady += Refresh;
        }

        private void OnDisable()
        {
            if (results != null) results.ResultReady -= Refresh;
        }

        private void Refresh()
        {
            if (results == null) return;
            if (placementText != null) placementText.text = "#" + Mathf.Max(1, results.Placement);
            if (killsText != null) killsText.text = results.LocalKills.ToString();
            if (statusText != null) statusText.text = results.Won ? "VICTORY" : "MATCH COMPLETE";
            SetVisible(true);
        }

        public void ReturnToLobby()
        {
            if (!string.IsNullOrWhiteSpace(lobbySceneName)) SceneManager.LoadScene(lobbySceneName);
        }

        private void SetVisible(bool value)
        {
            if (panel == null) return;
            panel.alpha = value ? 1f : 0f;
            panel.interactable = value;
            panel.blocksRaycasts = value;
        }
    }
}
