using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.UI;
using UnityEngine;

namespace Fsp.Player
{
    [RequireComponent(typeof(PlayerVitals))]
    public sealed class PlayerDamageable : MonoBehaviour, IDamageable
    {
        private PlayerVitals vitals;
        private MatchParticipant victimParticipant;
        private MatchParticipant lastAttacker;

        private void Awake()
        {
            vitals = GetComponent<PlayerVitals>();
            victimParticipant = GetComponent<MatchParticipant>();
        }

        private void OnEnable()
        {
            if (vitals != null) vitals.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (vitals != null) vitals.Died -= HandleDied;
        }

        public void ApplyDamage(float amount, Vector3 point, Vector3 normal, GameObject source)
        {
            if (source != null)
            {
                var attacker = source.GetComponentInParent<MatchParticipant>();
                if (attacker != null && attacker != victimParticipant)
                    lastAttacker = attacker;
            }

            vitals.ApplyDamage(amount);
        }

        private void HandleDied()
        {
            KillFeedBus.Report(lastAttacker, victimParticipant);
        }
    }
}
