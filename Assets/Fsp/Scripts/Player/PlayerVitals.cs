using System;
using UnityEngine;

namespace Fsp.Player
{
    public sealed class PlayerVitals : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float maxArmor = 100f;

        public float Health { get; private set; }
        public float Armor { get; private set; }
        public bool IsAlive => Health > 0f;

        public event Action<float, float> Changed;
        public event Action Died;

        private void Awake()
        {
            Health = maxHealth;
            Armor = 0f;
        }

        public void ApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            float armorDamage = Mathf.Min(Armor, amount * 0.6f);
            Armor -= armorDamage;
            float healthDamage = amount - armorDamage;
            Health = Mathf.Max(0f, Health - healthDamage);

            Changed?.Invoke(Health, Armor);
            if (Health <= 0f) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Health = Mathf.Min(maxHealth, Health + amount);
            Changed?.Invoke(Health, Armor);
        }

        public void AddArmor(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Armor = Mathf.Min(maxArmor, Armor + amount);
            Changed?.Invoke(Health, Armor);
        }
    }
}
