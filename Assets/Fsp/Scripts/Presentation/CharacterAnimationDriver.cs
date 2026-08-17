using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Inventory;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Presentation
{
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterAnimationDriver : MonoBehaviour
    {
        [SerializeField] private ThirdPersonMotor motor;
        [SerializeField] private CharacterController controller;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private ParachuteController parachute;
        [SerializeField] private float dampTime = 0.1f;

        private Animator animator;
        private Vector3 previousPosition;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int Parachute = Animator.StringToHash("Parachute");
        private static readonly int Freefall = Animator.StringToHash("Freefall");
        private static readonly int Armed = Animator.StringToHash("Armed");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (motor == null) motor = GetComponentInParent<ThirdPersonMotor>();
            if (controller == null) controller = GetComponentInParent<CharacterController>();
            if (inventory == null) inventory = GetComponentInParent<PlayerInventory>();
            if (parachute == null) parachute = GetComponentInParent<ParachuteController>();
            previousPosition = transform.root.position;
        }

        private void Update()
        {
            Vector3 rootPosition = transform.root.position;
            Vector3 delta = rootPosition - previousPosition;
            previousPosition = rootPosition;
            Vector3 planar = Vector3.ProjectOnPlane(delta, Vector3.up);
            float speed = Time.deltaTime > 0f ? planar.magnitude / Time.deltaTime : 0f;

            bool grounded = controller != null && controller.isGrounded;
            bool dropping = parachute != null && parachute.IsActive;
            bool chuteOpen = dropping && parachute.IsOpen;

            animator.SetFloat(Speed, speed, dampTime, Time.deltaTime);
            animator.SetBool(Grounded, grounded);
            animator.SetFloat(VerticalSpeed, Time.deltaTime > 0f ? delta.y / Time.deltaTime : 0f);
            animator.SetBool(Parachute, chuteOpen);
            animator.SetBool(Freefall, dropping && !chuteOpen);
            animator.SetBool(Armed, inventory != null && inventory.ActiveWeapon != null);
        }
    }
}
