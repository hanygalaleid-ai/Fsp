using UnityEngine;

namespace Fsp.World
{
    public sealed class SafeSpawnResolver : MonoBehaviour
    {
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private LayerMask blockingMask = ~0;
        [SerializeField] private float probeHeight = 80f;
        [SerializeField] private float playerRadius = 0.55f;
        [SerializeField] private float playerHeight = 1.85f;
        [SerializeField] private int attempts = 24;
        [SerializeField] private float searchRadius = 18f;

        public bool TryResolve(Vector3 desired, out Vector3 resolved)
        {
            if (TryPoint(desired, out resolved)) return true;

            for (int i = 0; i < attempts; i++)
            {
                float angle = i * 137.50776f * Mathf.Deg2Rad;
                float radius = searchRadius * Mathf.Sqrt((i + 1f) / Mathf.Max(1f, attempts));
                Vector3 candidate = desired + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (TryPoint(candidate, out resolved)) return true;
            }

            resolved = desired;
            return false;
        }

        private bool TryPoint(Vector3 point, out Vector3 grounded)
        {
            Vector3 origin = point + Vector3.up * probeHeight;
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, probeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                grounded = point;
                return false;
            }

            Vector3 feet = hit.point + Vector3.up * 0.05f;
            Vector3 bottom = feet + Vector3.up * playerRadius;
            Vector3 top = feet + Vector3.up * Mathf.Max(playerRadius, playerHeight - playerRadius);
            bool blocked = Physics.CheckCapsule(bottom, top, playerRadius, blockingMask, QueryTriggerInteraction.Ignore);
            grounded = feet;
            return !blocked;
        }
    }
}
