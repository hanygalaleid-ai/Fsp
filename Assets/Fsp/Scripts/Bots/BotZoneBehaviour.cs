using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.AI;

namespace Fsp.Bots
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class BotZoneBehaviour : MonoBehaviour
    {
        [SerializeField] private SafeZoneController safeZone;
        [SerializeField, Min(0.1f)] private float repathInterval = 0.5f;
        [SerializeField, Min(0f)] private float centerInset = 8f;

        private NavMeshAgent agent;
        private float nextRepathTime;

        public bool IsReturningToZone { get; private set; }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (safeZone == null || agent == null || !agent.enabled) return;

            bool outside = !safeZone.IsInside(transform.position);
            IsReturningToZone = outside;
            if (!outside || Time.time < nextRepathTime) return;

            nextRepathTime = Time.time + repathInterval;
            Vector3 toCenter = safeZone.Center - transform.position;
            toCenter.y = 0f;
            Vector3 desired = safeZone.Center;
            if (toCenter.sqrMagnitude > 0.01f)
            {
                float radius = Mathf.Max(0f, safeZone.CurrentRadius - centerInset);
                desired = safeZone.Center - toCenter.normalized * Mathf.Min(radius * 0.25f, toCenter.magnitude * 0.2f);
            }

            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 20f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }
}
