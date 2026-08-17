using UnityEngine;
using Fsp.World;

namespace Fsp.AI
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class BotMotor : MonoBehaviour
    {
        [SerializeField] private BotNavigationGrid navigationGrid;
        [SerializeField] private float moveSpeed = 4.2f;
        [SerializeField] private float sprintSpeed = 6.2f;
        [SerializeField] private float rotationSharpness = 10f;
        [SerializeField] private float gravity = 22f;
        [SerializeField] private float arriveDistance = 1.4f;

        private CharacterController controller;
        private Vector3 destination;
        private bool hasDestination;
        private bool sprint;
        private float verticalVelocity;

        public bool HasDestination => hasDestination;
        public Vector3 Destination => destination;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public void SetDestination(Vector3 worldPoint, bool run = false)
        {
            destination = navigationGrid != null ? navigationGrid.GetNearestWalkable(worldPoint) : worldPoint;
            sprint = run;
            hasDestination = true;
        }

        public void Stop() => hasDestination = false;

        private void Update()
        {
            Vector3 planar = Vector3.zero;
            if (hasDestination)
            {
                Vector3 delta = destination - transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude <= arriveDistance * arriveDistance)
                {
                    hasDestination = false;
                }
                else
                {
                    planar = delta.normalized;
                    Quaternion target = Quaternion.LookRotation(planar, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSharpness * Time.deltaTime);
                }
            }

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity -= gravity * Time.deltaTime;
            float speed = sprint ? sprintSpeed : moveSpeed;
            controller.Move((planar * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }
}
