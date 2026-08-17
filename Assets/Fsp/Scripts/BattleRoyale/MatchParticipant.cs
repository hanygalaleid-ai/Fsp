using UnityEngine;
using Fsp.Player;

namespace Fsp.BattleRoyale
{
    [RequireComponent(typeof(PlayerVitals))]
    public sealed class MatchParticipant : MonoBehaviour
    {
        [SerializeField] private bool isBot;
        [SerializeField] private bool isLocalPlayer;
        [SerializeField] private string displayName = "Player";

        private PlayerVitals vitals;

        public bool IsBot => isBot;
        public bool IsLocalPlayer => isLocalPlayer;
        public string DisplayName => displayName;
        public bool IsAlive => vitals != null && vitals.IsAlive;
        public int Placement { get; private set; }

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

        public void ConfigureAsBot(string name)
        {
            isBot = true;
            isLocalPlayer = false;
            displayName = string.IsNullOrWhiteSpace(name) ? "Bot" : name.Trim();
        }

        public void ConfigureAsLocalPlayer(string name)
        {
            isBot = false;
            isLocalPlayer = true;
            displayName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        }

        public void SetPlacement(int value) => Placement = Mathf.Max(1, value);

        private void HandleDeath()
        {
            MatchManager.NotifyDeath(this);
        }
    }
}
