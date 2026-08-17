using UnityEngine;

namespace Fsp.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonMotor : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Min(0.1f)] private float walkSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0.1f)] private float rotationSharpness = 12f;
        [SerializeField, Min(0.1f)] private float gravity = 22f;
        [SerializeField, Min(0.1f)] private float jumpHeight = 1.25f;

        private CharacterController controller;
        private Vector2 moveInput;
        private bool sprint;
        private bool jumpRequested;
        private float verticalVelocity;

        public bool IsGrounded => controller != null && controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public void SetMoveInput(Vector2 input) => moveInput = Vector2.ClampMagnitude(input, 1f);
        public void SetSprint(bool value) => sprint = value;
        public void SetCamera(Transform value) => cameraTransform = value;
        public void RequestJump() => jumpRequested = true;

        private void Update()
        {
            Vector3 forward = cameraTransform ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 planar = forward * moveInput.y + right * moveInput.x;
            if (planar.sqrMagnitude > 1f) planar.Normalize();

            if (planar.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(planar, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSharpness * Time.deltaTime);
            }

            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -2f;
                if (jumpRequested)
                    verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
            }

            jumpRequested = false;
            verticalVelocity -= gravity * Time.deltaTime;

            float speed = sprint ? sprintSpeed : walkSpeed;
            Vector3 velocity = planar * speed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
