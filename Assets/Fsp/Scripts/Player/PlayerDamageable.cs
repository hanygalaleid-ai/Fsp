using Fsp.Combat;
using UnityEngine;

namespace Fsp.Player
{
    [RequireComponent(typeof(PlayerVitals))]
    public sealed class PlayerDamageable : MonoBehaviour, IDamageable
    {
        private PlayerVitals vitals;

        private void Awake()
        {
            vitals = GetComponent<PlayerVitals>();
        }

        public void ApplyDamage(float amount, Vector3 point, Vector3 normal, GameObject source)
        {
            vitals.ApplyDamage(amount);
        }
    }
}
