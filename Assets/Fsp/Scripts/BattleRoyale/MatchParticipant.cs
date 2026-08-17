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

        public void SetPlacement(int value) => Placement = Mathf.Max(1, value);

        private void HandleDeath()
        {
            MatchManager.NotifyDeath(this);
        }
    }
}
