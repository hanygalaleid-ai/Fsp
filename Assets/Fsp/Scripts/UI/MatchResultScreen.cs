using Fsp.Backend;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.UI;

namespace Fsp.UI
{
    public sealed class MatchResultScreen : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchParticipant localPlayer;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;

        private void Awake()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (matchManager == null) return;
            matchManager.MatchWon += HandleMatchWon;
            matchManager.NetworkWinnerDeclared += HandleNetworkWinner;
        }

        private void OnDisable()
        {
            if (matchManager == null) return;
            matchManager.MatchWon -= HandleMatchWon;
            matchManager.NetworkWinnerDeclared -= HandleNetworkWinner;
        }

        private void HandleMatchWon(MatchParticipant winner)
        {
            if (matchManager != null && matchManager.NetworkAuthoritative) return;
            ShowResult(winner != null && winner == localPlayer, winner != null ? winner.DisplayName : string.Empty);
        }

        private void HandleNetworkWinner(string winnerId)
        {
            bool localWon = SupabaseSession.IsSignedIn && !string.IsNullOrWhiteSpace(winnerId) && winnerId == SupabaseSession.UserId;
            string winnerName = localWon ? "أنت" : ShortName(winnerId);
            ShowResult(localWon, winnerName);
        }

        private void ShowResult(bool localWon, string winnerName)
        {
            if (panel != null) panel.SetActive(true);
            if (titleText != null) titleText.text = localWon ? "الفوز لك!" : "انتهت المباراة";
            if (subtitleText == null) return;
            if (localWon) subtitleText.text = "أنت آخر لاعب على قيد الحياة";
            else if (!string.IsNullOrWhiteSpace(winnerName)) subtitleText.text = $"الفائز: {winnerName}";
            else subtitleText.text = "لم يتبق لاعب حي";
        }

        private static string ShortName(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;
            return id.Length <= 8 ? id : id.Substring(0, 8);
        }
    }
}
