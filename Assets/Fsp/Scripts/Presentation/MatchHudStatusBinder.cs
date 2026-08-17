using Fsp.BattleRoyale;
using TMPro;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class MatchHudStatusBinder : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private TMP_Text aliveText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private TMP_Text phaseText;

        private void OnEnable()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (matchManager == null) return;
            matchManager.AliveCountChanged += OnAlive;
            matchManager.CountdownChanged += OnCountdown;
            matchManager.PhaseChanged += OnPhase;
            OnAlive(matchManager.AliveCount);
            OnCountdown(matchManager.CountdownRemaining);
            OnPhase(matchManager.Phase);
        }

        private void OnDisable()
        {
            if (matchManager == null) return;
            matchManager.AliveCountChanged -= OnAlive;
            matchManager.CountdownChanged -= OnCountdown;
            matchManager.PhaseChanged -= OnPhase;
        }

        private void OnAlive(int value)
        {
            if (aliveText != null) aliveText.text = value.ToString();
        }

        private void OnCountdown(float value)
        {
            if (countdownText != null)
            {
                bool show = matchManager != null && matchManager.Phase == MatchManager.MatchPhase.Countdown;
                countdownText.gameObject.SetActive(show);
                if (show) countdownText.text = Mathf.CeilToInt(value).ToString();
            }
        }

        private void OnPhase(MatchManager.MatchPhase value)
        {
            if (phaseText != null)
                phaseText.text = value == MatchManager.MatchPhase.Active ? "LIVE" : value.ToString().ToUpperInvariant();
            OnCountdown(matchManager != null ? matchManager.CountdownRemaining : 0f);
        }
    }
}
