using UnityEngine;

namespace Fsp.BattleRoyale
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ParachuteController : MonoBehaviour
    {
        [SerializeField] private GameObject parachuteVisual;
        [SerializeField, Min(1f)] private float freeFallSpeed = 38f;
        [SerializeField, Min(1f)] private float parachuteFallSpeed = 8f;
        [SerializeField, Min(1f)] private float steerSpeed = 11f;
        [SerializeField, Min(1f)] private float autoOpenHeight = 45f;
        [SerializeField] private LayerMask groundMask = ~0;

        private CharacterController controller;
        private bool active;
        private bool opened;
        private Vector2 steerInput;
        private float dropStartedAt;

        public bool IsActive => active;
        public bool IsOpen => opened;

        public void ConfigureVisual(GameObject visual)
        {
            parachuteVisual = visual;
            if (parachuteVisual != null) parachuteVisual.SetActive(opened && active);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (parachuteVisual != null) parachuteVisual.SetActive(false);
        }

        public void BeginDrop()
        {
            active = true;
            opened = false;
            dropStartedAt = Time.unscaledTime;
            steerInput = Vector2.zero;
            if (parachuteVisual != null) parachuteVisual.SetActive(false);
        }

        public void SetSteer(Vector2 input) => steerInput = Vector2.ClampMagnitude(input, 1f);

        public void OpenParachute()
        {
            if (!active || opened) return;
            opened = true;
            if (parachuteVisual != null) parachuteVisual.SetActive(true);
        }

        private void Update()
        {
            if (!active || controller == null) return;

            // Open only when the ground is inside the configured safety height. The
            // previous inverted check opened immediately while the player was still
            // high above the island.
            bool groundNear = Physics.Raycast(transform.position, Vector3.down, out _, autoOpenHeight, groundMask, QueryTriggerInteraction.Ignore);
            bool safetyTimeout = Time.unscaledTime - dropStartedAt >= 4f;
            if (!opened && (groundNear || safetyTimeout))
                OpenParachute();

            float fallSpeed = opened ? parachuteFallSpeed : freeFallSpeed;
            Vector3 horizontal = (transform.forward * steerInput.y + transform.right * steerInput.x) * steerSpeed;
            controller.Move((horizontal + Vector3.down * fallSpeed) * Time.deltaTime);

            if (controller.isGrounded)
            {
                CompleteLanding();
                return;
            }

            // Device/scene safety: a missing collision must never leave the player falling forever.
            if (transform.position.y < -15f) RecoverToIslandSurface();
        }

        private void CompleteLanding()
        {
            active = false;
            opened = false;
            steerInput = Vector2.zero;
            if (parachuteVisual != null) parachuteVisual.SetActive(false);
        }

        private void RecoverToIslandSurface()
        {
            Vector3 probe = new Vector3(transform.position.x, 320f, transform.position.z);
            Vector3 destination = new Vector3(0f, 3f, 0f);
            float nearest = float.MaxValue;
            foreach (RaycastHit hit in Physics.RaycastAll(probe, Vector3.down, 640f, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.collider == controller || hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.distance >= nearest) continue;
                nearest = hit.distance;
                destination = hit.point + Vector3.up * .15f;
            }

            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = destination;
            controller.enabled = wasEnabled;
            CompleteLanding();
            Debug.LogWarning("BMG drop recovery moved the player to a safe island surface after a missing ground collision.");
        }
    }
}
