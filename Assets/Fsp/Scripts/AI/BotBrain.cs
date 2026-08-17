using Fsp.BattleRoyale;
using Fsp.Inventory;
using Fsp.Player;
using UnityEngine;

namespace Fsp.AI
{
    public sealed class BotBrain : MonoBehaviour
    {
        private enum State { Loot, Rotate, Investigate, Fight, Cover }

        [SerializeField] private BotMotor motor;
        [SerializeField] private BotPerception perception;
        [SerializeField] private BotCombat combat;
        [SerializeField] private SafeZoneController safeZone;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private BotDifficultyProfile difficulty;
        [SerializeField] private LayerMask lootMask;
        [SerializeField] private LayerMask coverMask;
        [SerializeField] private float decisionInterval = 0.45f;
        [SerializeField] private float lootSearchRadius = 28f;
        [SerializeField] private float pickupDistance = 1.8f;
        [SerializeField] private float coverSearchRadius = 12f;
        [SerializeField] private float fightDistance = 45f;
        [SerializeField] private float healBelowHealth = 48f;

        private float nextDecision;
        private State state;
        private Transform target;
        private LootPickup lootTarget;

        private void Awake()
        {
            if (motor == null) motor = GetComponent<BotMotor>();
            if (perception == null) perception = GetComponent<BotPerception>();
            if (combat == null) combat = GetComponent<BotCombat>();
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
            ApplyDifficulty();
        }

        private void Update()
        {
            if (vitals != null && !vitals.IsAlive)
            {
                motor?.Stop();
                enabled = false;
                return;
            }

            TryCollectCurrentLoot();

            if (Time.time >= nextDecision)
            {
                float interval = difficulty != null ? difficulty.decisionInterval : decisionInterval;
                nextDecision = Time.time + interval + Random.Range(0f, 0.12f);
                Think();
            }

            if (state == State.Fight && target != null)
            {
                Vector3 flat = target.position - transform.position;
                flat.y = 0f;
                if (flat.sqrMagnitude <= fightDistance * fightDistance)
                    combat?.TryAttack(target);
            }
        }

        private void ApplyDifficulty()
        {
            if (difficulty == null) return;
            decisionInterval = difficulty.decisionInterval;
            combat?.ConfigureDifficulty(difficulty);
        }

        private void Think()
        {
            if (TryHeal()) return;

            if (safeZone != null && !safeZone.IsInside(transform.position))
            {
                state = State.Rotate;
                lootTarget = null;
                motor?.SetDestination(safeZone.Center, true);
                return;
            }

            target = perception != null ? perception.CurrentTarget : null;
            if (target != null)
            {
                lootTarget = null;
                float sqr = (target.position - transform.position).sqrMagnitude;
                float coverChance = difficulty != null ? difficulty.useCoverChance : 0.7f;
                if (sqr < 18f * 18f && Random.value <= coverChance && TryFindCover(out Vector3 cover))
                {
                    state = State.Cover;
                    motor?.SetDestination(cover, true);
                    return;
                }

                MaybeSwitchWeapon(sqr);
                state = State.Fight;
                if (sqr > fightDistance * fightDistance)
                    motor?.SetDestination(target.position, true);
                else
                    motor?.Stop();
                return;
            }

            if (TryFindLoot(out LootPickup pickup))
            {
                lootTarget = pickup;
                state = State.Loot;
                motor?.SetDestination(pickup.transform.position, false);
                return;
            }

            lootTarget = null;
            if (perception != null && perception.LastKnownPosition != Vector3.zero)
            {
                state = State.Investigate;
                motor?.SetDestination(perception.LastKnownPosition, true);
                return;
            }

            state = State.Rotate;
            if (safeZone != null)
            {
                Vector2 jitter = Random.insideUnitCircle * Mathf.Max(12f, safeZone.CurrentRadius * 0.35f);
                motor?.SetDestination(safeZone.Center + new Vector3(jitter.x, 0f, jitter.y), false);
            }
        }

        private bool TryHeal()
        {
            if (inventory == null || vitals == null || vitals.Health > healBelowHealth || inventory.Medkits <= 0) return false;
            float chance = difficulty != null ? difficulty.healChance : 0.8f;
            if (Random.value > chance) return false;
            if (!inventory.TryUseMedkit()) return false;
            state = State.Cover;
            if (TryFindCover(out Vector3 cover)) motor?.SetDestination(cover, true);
            else motor?.Stop();
            return true;
        }

        private void MaybeSwitchWeapon(float targetSqrDistance)
        {
            if (inventory == null || inventory.SecondaryWeapon == null) return;
            float chance = difficulty != null ? difficulty.weaponSwitchChance : 0.35f;
            if (Random.value > chance) return;
            float distance = Mathf.Sqrt(targetSqrDistance);
            bool wantsSecondary = distance < 18f;
            if (wantsSecondary && inventory.ActiveWeapon != inventory.SecondaryWeapon) inventory.SwitchWeapon();
            else if (!wantsSecondary && inventory.ActiveWeapon != inventory.PrimaryWeapon) inventory.SwitchWeapon();
        }

        private void TryCollectCurrentLoot()
        {
            if (lootTarget == null) return;
            if ((lootTarget.transform.position - transform.position).sqrMagnitude > pickupDistance * pickupDistance) return;
            lootTarget.TryGiveTo(this);
            lootTarget = null;
        }

        private bool TryFindLoot(out LootPickup pickup)
        {
            pickup = null;
            Collider[] hits = Physics.OverlapSphere(transform.position, lootSearchRadius, lootMask, QueryTriggerInteraction.Collide);
            float best = float.MaxValue;
            foreach (Collider hit in hits)
            {
                LootPickup candidate = hit.GetComponentInParent<LootPickup>();
                if (candidate == null) continue;
                float sqr = (candidate.transform.position - transform.position).sqrMagnitude;
                if (sqr < best)
                {
                    best = sqr;
                    pickup = candidate;
                }
            }
            return pickup != null;
        }

        private bool TryFindCover(out Vector3 point)
        {
            point = default;
            Collider[] hits = Physics.OverlapSphere(transform.position, coverSearchRadius, coverMask, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0) return false;
            Collider chosen = hits[Random.Range(0, hits.Length)];
            Vector3 away = (transform.position - chosen.bounds.center).normalized;
            point = chosen.ClosestPoint(transform.position) + away * 1.2f;
            return true;
        }
    }
}
