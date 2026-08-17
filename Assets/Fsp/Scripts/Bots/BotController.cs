using Fsp.Combat;
using Fsp.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Fsp.Bots
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class BotController : MonoBehaviour
    {
        [SerializeField] private float detectionRange = 45f;
        [SerializeField] private float attackRange = 22f;
        [SerializeField] private float repathInterval = 0.25f;
        [SerializeField] private float aimTurnSpeed = 8f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private PlayerVitals vitals;

        private NavMeshAgent agent;
        private Transform target;
        private float nextRepathTime;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
        }

        private void OnEnable()
        {
            if (vitals != null) vitals.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (vitals != null) vitals.Died -= HandleDeath;
        }

        private void Update()
        {
            if (vitals != null && !vitals.IsAlive) return;

            if (target == null || !TargetStillValid())
                AcquireTarget();

            if (target == null)
            {
                agent.isStopped = true;
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > attackRange)
            {
                agent.isStopped = false;
                if (Time.time >= nextRepathTime)
                {
                    agent.SetDestination(target.position);
                    nextRepathTime = Time.time + repathInterval;
                }
            }
            else
            {
                agent.isStopped = true;
                FaceTarget();
                weapon?.TryFire();
            }
        }

        private bool TargetStillValid()
        {
            if (target == null) return false;
            if (Vector3.Distance(transform.position, target.position) > detectionRange * 1.25f) return false;
            var targetVitals = target.GetComponentInParent<PlayerVitals>();
            return targetVitals == null || targetVitals.IsAlive;
        }

        private void AcquireTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, targetMask, QueryTriggerInteraction.Ignore);
            float bestDistance = float.MaxValue;
            Transform best = null;

            foreach (var hit in hits)
            {
                if (hit.transform.root == transform.root) continue;
                var pv = hit.GetComponentInParent<PlayerVitals>();
                if (pv != null && !pv.IsAlive) continue;

                float d = (hit.transform.position - transform.position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = hit.transform;
                }
            }

            target = best;
        }

        private void FaceTarget()
        {
            Vector3 flat = target.position - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < 0.001f) return;

            Quaternion desired = Quaternion.LookRotation(flat.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, aimTurnSpeed * Time.deltaTime);
        }

        private void HandleDeath()
        {
            agent.isStopped = true;
            enabled = false;
        }
    }
}
