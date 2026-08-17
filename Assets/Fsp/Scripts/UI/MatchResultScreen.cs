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
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (matchManager != null) matchManager.MatchWon += HandleMatchWon;
        }

        private void OnDisable()
        {
            if (matchManager != null) matchManager.MatchWon -= HandleMatchWon;
        }

        private void HandleMatchWon(MatchParticipant winner)
        {
            if (panel != null) panel.SetActive(true);

            bool localWon = winner != null && winner == localPlayer;
            if (titleText != null)
                titleText.text = localWon ? "الفوز لك!" : "انتهت المباراة";

            if (subtitleText != null)
            {
                if (localWon)
                    subtitleText.text = "أنت آخر لاعب على قيد الحياة";
                else if (winner != null)
                    subtitleText.text = $"الفائز: {winner.DisplayName}";
                else
                    subtitleText.text = "لم يتبق لاعب حي";
            }
        }
    }
}
