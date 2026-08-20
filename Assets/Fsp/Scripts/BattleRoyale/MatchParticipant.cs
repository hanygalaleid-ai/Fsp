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
        private bool deathSubscribed;

        public bool IsBot => isBot;
        public bool IsLocalPlayer => isLocalPlayer;
        public string DisplayName => displayName;
        public bool IsAlive
        {
            get
            {
                EnsureVitals();
                return vitals != null && vitals.IsAlive;
            }
        }
        public int Placement { get; private set; }

        private void Awake() => EnsureVitals();

        private void OnEnable()
        {
            EnsureVitals();
            SubscribeDeath();
            MatchManager.Register(this);
        }

        private void OnDisable()
        {
            UnsubscribeDeath();
            MatchManager.Unregister(this);
        }

        public void ConfigureAsBot(string name)
        {
            EnsureVitals();
            SubscribeDeath();
            isBot = true;
            isLocalPlayer = false;
            displayName = string.IsNullOrWhiteSpace(name) ? "Bot" : name.Trim();
            MatchManager.Register(this);
        }

        public void ConfigureAsLocalPlayer(string name)
        {
            EnsureVitals();
            SubscribeDeath();
            isBot = false;
            isLocalPlayer = true;
            displayName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
            MatchManager.Register(this);
        }

        public void SetPlacement(int value) => Placement = Mathf.Max(1, value);

        private void EnsureVitals()
        {
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
        }

        private void SubscribeDeath()
        {
            EnsureVitals();
            if (vitals == null || deathSubscribed) return;
            vitals.Died += HandleDeath;
            deathSubscribed = true;
        }

        private void UnsubscribeDeath()
        {
            if (vitals == null || !deathSubscribed) return;
            vitals.Died -= HandleDeath;
            deathSubscribed = false;
        }

        private void HandleDeath() => MatchManager.NotifyDeath(this);
    }
}
