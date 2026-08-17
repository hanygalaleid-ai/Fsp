using Fsp.Combat;
using Fsp.Inventory;
using Fsp.Player;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Input
{
    public sealed class MobileGameplayAdapter : MonoBehaviour
    {
        [SerializeField] private ThirdPersonMotor motor;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float lookSensitivity = 1f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float vehicleInteractRadius = 3f;
        [SerializeField] private LayerMask vehicleMask = ~0;

        private float yaw;
        private float pitch;
        private VehicleSeat activeSeat;

        private void Awake()
        {
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (cameraPivot != null)
            {
                Vector3 euler = cameraPivot.eulerAngles;
                yaw = euler.y;
                pitch = euler.x > 180f ? euler.x - 360f : euler.x;
                motor?.SetCamera(cameraPivot);
            }
        }

        private void Update()
        {
            MobileInputBridge input = MobileInputBridge.Instance;
            if (input == null) return;

            UpdateLook(input.Look);

            if (activeSeat != null && activeSeat.IsDriver(motor))
            {
                DriveVehicle(input);
                if (input.InteractPressed)
                {
                    activeSeat.Exit();
                    activeSeat = null;
                }
                return;
            }

            if (motor != null)
            {
                motor.SetMoveInput(input.Move);
                motor.SetSprint(input.SprintHeld);
                if (input.JumpPressed) motor.RequestJump();
            }

            HitscanWeapon weapon = inventory != null ? inventory.ActiveWeapon : null;
            if (input.FireHeld && weapon != null) weapon.TryFire();
            if (input.SwitchWeaponPressed) inventory?.SwitchWeapon();

            if (input.InteractPressed)
                TryEnterNearestVehicle();
        }

        private void UpdateLook(Vector2 look)
        {
            if (cameraPivot == null || look.sqrMagnitude <= 0f) return;
            yaw += look.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, minPitch, maxPitch);
            cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void DriveVehicle(MobileInputBridge input)
        {
            if (activeSeat == null || activeSeat.Vehicle == null) return;
            bool brake = input.SprintHeld;
            activeSeat.Vehicle.SetInput(input.Move.y, input.Move.x, brake);
        }

        private void TryEnterNearestVehicle()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, vehicleInteractRadius, vehicleMask, QueryTriggerInteraction.Collide);
            VehicleSeat bestSeat = null;
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                VehicleSeat seat = hit.GetComponentInParent<VehicleSeat>();
                if (seat == null || seat.Occupied) continue;

                float sqr = (seat.transform.position - transform.position).sqrMagnitude;
                if (sqr >= bestDistance) continue;
                bestDistance = sqr;
                bestSeat = seat;
            }

            if (bestSeat != null && bestSeat.TryEnter(motor))
                activeSeat = bestSeat;
        }
    }
}
