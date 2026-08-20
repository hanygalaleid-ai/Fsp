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
            if (!opened && Physics.Raycast(transform.position, Vector3.down, out _, autoOpenHeight, groundMask, QueryTriggerInteraction.Ignore))
                OpenParachute();

            float fallSpeed = opened ? parachuteFallSpeed : freeFallSpeed;
            Vector3 horizontal = (transform.forward * steerInput.y + transform.right * steerInput.x) * steerSpeed;
            controller.Move((horizontal + Vector3.down * fallSpeed) * Time.deltaTime);

            if (controller.isGrounded)
            {
                active = false;
                opened = false;
                if (parachuteVisual != null) parachuteVisual.SetActive(false);
            }
        }
    }
}
