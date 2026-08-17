using UnityEngine;
using Fsp.Player;

namespace Fsp.BattleRoyale
{
    [RequireComponent(typeof(PlayerVitals))]
    public sealed class MatchParticipant : MonoBehaviour
    {
        [SerializeField] private bool isBot;
        [SerializeField] private string displayName = "Player";

        private PlayerVitals vitals;

        public bool IsBot => isBot;
        public string DisplayName => displayName;
        public bool IsAlive => vitals != null && vitals.IsAlive;

        private void Awake()
        {
            vitals = GetComponent<PlayerVitals>();
        }

        private void OnEnable()
        {
            MatchManager.Register(this);
            if (vitals != null) vitals.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (vitals != null) vitals.Died -= HandleDeath;
            MatchManager.Unregister(this);
        }

        private void HandleDeath()
        {
            MatchManager.NotifyDeath(this);
        }
    }
}
