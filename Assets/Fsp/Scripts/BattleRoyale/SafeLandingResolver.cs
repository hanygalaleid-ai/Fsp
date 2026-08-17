using Fsp.World;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class SafeLandingResolver : MonoBehaviour
    {
        [SerializeField] private SafeSpawnResolver resolver;
        [SerializeField] private CharacterController controller;

        private void Awake()
        {
            if (resolver == null) resolver = FindFirstObjectByType<SafeSpawnResolver>();
            if (controller == null) controller = GetComponent<CharacterController>();
        }

        public bool ResolveLanding(Vector3 desiredWorldPosition)
        {
            if (resolver == null || !resolver.TryResolve(desiredWorldPosition, out Vector3 safe)) return false;

            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null) controller.enabled = false;
            transform.position = safe;
            if (controller != null) controller.enabled = wasEnabled;
            return true;
        }
    }
}
