using Fsp.BattleRoyale;
using Fsp.Inventory;
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
        [SerializeField] private LayerMask lootMask;
        [SerializeField] private LayerMask coverMask;
        [SerializeField] private float decisionInterval = 0.45f;
        [SerializeField] private float lootSearchRadius = 28f;
        [SerializeField] private float coverSearchRadius = 12f;
        [SerializeField] private float fightDistance = 45f;

        private float nextDecision;
        private State state;
        private Transform target;

        private void Awake()
        {
            if (motor == null) motor = GetComponent<BotMotor>();
            if (perception == null) perception = GetComponent<BotPerception>();
            if (combat == null) combat = GetComponent<BotCombat>();
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if (Time.time >= nextDecision)
            {
                nextDecision = Time.time + decisionInterval + Random.Range(0f, 0.12f);
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

        private void Think()
        {
            if (safeZone != null && !safeZone.IsInside(transform.position))
            {
                state = State.Rotate;
                motor?.SetDestination(safeZone.Center, true);
                return;
            }

            target = perception != null ? perception.CurrentTarget : null;
            if (target != null)
            {
                float sqr = (target.position - transform.position).sqrMagnitude;
                if (sqr < 18f * 18f && TryFindCover(out Vector3 cover))
                {
                    state = State.Cover;
                    motor?.SetDestination(cover, true);
                    return;
                }

                state = State.Fight;
                if (sqr > fightDistance * fightDistance)
                    motor?.SetDestination(target.position, true);
                else
                    motor?.Stop();
                return;
            }

            if (TryFindLoot(out Vector3 lootPoint))
            {
                state = State.Loot;
                motor?.SetDestination(lootPoint, false);
                return;
            }

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

        private bool TryFindLoot(out Vector3 point)
        {
            point = default;
            Collider[] hits = Physics.OverlapSphere(transform.position, lootSearchRadius, lootMask, QueryTriggerInteraction.Collide);
            float best = float.MaxValue;
            foreach (Collider hit in hits)
            {
                LootPickup pickup = hit.GetComponentInParent<LootPickup>();
                if (pickup == null) continue;
                float sqr = (pickup.transform.position - transform.position).sqrMagnitude;
                if (sqr < best)
                {
                    best = sqr;
                    point = pickup.transform.position;
                }
            }
            return best < float.MaxValue;
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
