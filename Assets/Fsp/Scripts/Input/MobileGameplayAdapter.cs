using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Inventory;
using Fsp.Player;
using Fsp.Presentation;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Input
{
    public sealed class MobileGameplayAdapter : MonoBehaviour
    {
        [SerializeField] private ThirdPersonMotor motor;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private DropPlanePassenger planePassenger;
        [SerializeField] private ParachuteController parachute;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float lookSensitivity = 0.16f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 2.35f, -6.8f);
        [SerializeField] private float cameraFollow = 12f;
        [SerializeField, Min(0.05f)] private float cameraCollisionRadius = 0.28f;
        [SerializeField, Min(0.05f)] private float cameraSurfacePadding = 0.18f;
        [SerializeField, Min(0.1f)] private float minimumCameraDistance = 0.8f;
        [SerializeField] private LayerMask cameraCollisionMask = ~0;
        [SerializeField] private float vehicleInteractRadius = 3f;
        [SerializeField] private LayerMask vehicleMask = ~0;

        private float yaw;
        private float pitch;
        private VehicleSeat activeSeat;
        private bool cameraInitialized;
        private bool wasAboard;
        private bool wasParachuting;

        private void Awake()
        {
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (planePassenger == null) planePassenger = GetComponent<DropPlanePassenger>();
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (cameraPivot == null && Camera.main != null) cameraPivot = Camera.main.transform;

            if (GetComponent<AimDownSightsController>() == null)
                gameObject.AddComponent<AimDownSightsController>();

            if (cameraPivot != null)
            {
                yaw = transform.eulerAngles.y;
                pitch = 9f;
                motor?.SetCamera(cameraPivot);
            }
        }

        private void Update()
        {
            MobileInputBridge input = MobileInputBridge.Instance;
            if (input == null) return;

            UpdateLook(input.Look);

            if (planePassenger != null && planePassenger.IsAboard)
            {
                if (input.JumpPressed) planePassenger.Jump();
                return;
            }

            if (parachute != null && parachute.IsActive)
            {
                parachute.SetSteer(input.Move);
                if (input.JumpPressed && !parachute.IsOpen) parachute.OpenParachute();
                return;
            }

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
            if (input.ReloadPressed) inventory?.TryReloadActiveWeapon();
            if (input.HealPressed) inventory?.TryUseMedkit();

            if (input.InteractPressed)
                TryEnterNearestVehicle();
        }

        private void LateUpdate()
        {
            if (cameraPivot == null)
            {
                if (Camera.main == null) return;
                cameraPivot = Camera.main.transform;
                motor?.SetCamera(cameraPivot);
                cameraInitialized = false;
            }

            bool aboard = planePassenger != null && planePassenger.IsAboard && planePassenger.Plane != null;
            bool parachuting = parachute != null && parachute.IsActive;
            Vector3 lookPoint;
            Vector3 desired;

            if (aboard)
            {
                Transform planeTransform = planePassenger.Plane.transform;
                lookPoint = planeTransform.position + planeTransform.forward * 5f;
                desired = planeTransform.TransformPoint(new Vector3(10f, 7.5f, -25f));
            }
            else
            {
                lookPoint = transform.position + Vector3.up * (parachuting ? 2.1f : 1.52f);
                Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
                Vector3 stateOffset = parachuting ? new Vector3(0f, 3.8f, -8.2f) : followOffset;
                desired = transform.position + orbit * stateOffset;
                desired = ResolveCameraCollision(lookPoint, desired);
            }

            if (aboard != wasAboard || parachuting != wasParachuting)
                cameraInitialized = false;
            wasAboard = aboard;
            wasParachuting = parachuting;

            if (!cameraInitialized)
            {
                cameraPivot.position = desired;
                cameraInitialized = true;
            }
            else
            {
                float t = 1f - Mathf.Exp(-cameraFollow * Time.deltaTime);
                cameraPivot.position = Vector3.Lerp(cameraPivot.position, desired, t);
            }

            Vector3 direction = lookPoint - cameraPivot.position;
            if (direction.sqrMagnitude > 0.01f)
                cameraPivot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Vector3 ResolveCameraCollision(Vector3 origin, Vector3 desired)
        {
            Vector3 delta = desired - origin;
            float fullDistance = delta.magnitude;
            if (fullDistance <= 0.001f) return desired;

            Vector3 direction = delta / fullDistance;
            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                cameraCollisionRadius,
                direction,
                fullDistance,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore);

            float allowedDistance = fullDistance;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;
                Transform hitTransform = hit.collider.transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;

                float candidate = Mathf.Max(minimumCameraDistance, hit.distance - cameraSurfacePadding);
                if (candidate < allowedDistance) allowedDistance = candidate;
            }

            return origin + direction * allowedDistance;
        }

        private void UpdateLook(Vector2 look)
        {
            if (look.sqrMagnitude <= 0f) return;
            yaw += look.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, minPitch, maxPitch);
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
