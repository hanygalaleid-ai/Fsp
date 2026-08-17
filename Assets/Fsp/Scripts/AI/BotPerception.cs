using UnityEngine;
using Fsp.Networking;

namespace Fsp.AI
{
    public sealed class BotPerception : MonoBehaviour
    {
        [SerializeField] private Transform eye;
        [SerializeField] private float sightRange = 90f;
        [SerializeField, Range(10f, 180f)] private float fieldOfView = 105f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask occlusionMask = ~0;
        [SerializeField] private float scanInterval = 0.35f;

        private float nextScan;
        public Transform CurrentTarget { get; private set; }
        public Vector3 LastKnownPosition { get; private set; }

        private void Update()
        {
            if (Time.time < nextScan) return;
            nextScan = Time.time + scanInterval + Random.Range(0f, 0.08f);
            Scan();
        }

        private void Scan()
        {
            Vector3 origin = eye != null ? eye.position : transform.position + Vector3.up * 1.5f;
            Collider[] hits = Physics.OverlapSphere(origin, sightRange, targetMask, QueryTriggerInteraction.Ignore);
            Transform best = null;
            float bestSqr = float.MaxValue;

            foreach (Collider hit in hits)
            {
                var identity = hit.GetComponentInParent<NetworkPlayerIdentity>();
                if (identity == null || identity.IsLocalPlayer == false && string.IsNullOrWhiteSpace(identity.PlayerId)) continue;
                Transform candidate = identity.transform;
                Vector3 to = candidate.position + Vector3.up * 1.2f - origin;
                float sqr = to.sqrMagnitude;
                if (sqr >= bestSqr) continue;
                Vector3 dir = to.normalized;
                if (Vector3.Angle(transform.forward, dir) > fieldOfView * 0.5f) continue;
                if (Physics.Raycast(origin, dir, out RaycastHit block, Mathf.Sqrt(sqr), occlusionMask, QueryTriggerInteraction.Ignore))
                {
                    if (block.collider.GetComponentInParent<NetworkPlayerIdentity>() != identity) continue;
                }
                best = candidate;
                bestSqr = sqr;
            }

            CurrentTarget = best;
            if (best != null) LastKnownPosition = best.position;
        }

        public void Hear(Vector3 worldPosition, float loudness = 1f)
        {
            if (loudness <= 0f) return;
            float max = 35f * Mathf.Clamp(loudness, 0.25f, 2f);
            if ((worldPosition - transform.position).sqrMagnitude <= max * max)
                LastKnownPosition = worldPosition;
        }
    }
}
