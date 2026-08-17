using Fsp.Player;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class CharacterCombatAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private int upperBodyLayer = 1;
        [SerializeField] private float layerBlendSpeed = 8f;
        private bool aiming;
        private bool dead;
        private float previousHealth;

        private static readonly int Aim = Animator.StringToHash("Aim");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Die = Animator.StringToHash("Die");

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (vitals == null) vitals = GetComponentInParent<PlayerVitals>();
            previousHealth = vitals != null ? vitals.Health : 100f;
        }

        private void OnEnable()
        {
            if (vitals != null)
            {
                vitals.Changed += OnVitalsChanged;
                vitals.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (vitals != null)
            {
                vitals.Changed -= OnVitalsChanged;
                vitals.Died -= OnDied;
            }
        }

        private void Update()
        {
            if (animator == null || upperBodyLayer < 0 || upperBodyLayer >= animator.layerCount) return;
            float target = aiming && !dead ? 1f : 0f;
            animator.SetLayerWeight(upperBodyLayer, Mathf.MoveTowards(animator.GetLayerWeight(upperBodyLayer), target, layerBlendSpeed * Time.deltaTime));
        }

        public void SetAim(bool value)
        {
            aiming = value;
            if (animator != null) animator.SetBool(Aim, value && !dead);
        }

        private void OnVitalsChanged(float health, float armor)
        {
            if (!dead && health < previousHealth && health > 0f && animator != null)
                animator.SetTrigger(Hit);
            previousHealth = health;
        }

        private void OnDied()
        {
            if (dead) return;
            dead = true;
            aiming = false;
            if (animator != null)
            {
                animator.SetBool(Aim, false);
                animator.SetTrigger(Die);
            }
        }
    }
}
