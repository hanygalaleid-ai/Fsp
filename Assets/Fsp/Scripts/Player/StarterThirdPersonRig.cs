using Fsp.BattleRoyale;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Player
{
    /// <summary>
    /// Lightweight editor/desktop fallback rig. Final mobile input can replace it without touching ThirdPersonMotor.
    /// </summary>
    public sealed class StarterThirdPersonRig : MonoBehaviour
    {
        [SerializeField] private ThirdPersonMotor motor;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3.2f, -5.8f);
        [SerializeField] private float cameraFollow = 10f;

        private DropPlanePassenger passenger;
        private ParachuteController parachute;
        private StarterVehicleInput vehicleInput;

        private void Awake()
        {
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
            passenger = GetComponent<DropPlanePassenger>();
            parachute = GetComponent<ParachuteController>();
            vehicleInput = GetComponent<StarterVehicleInput>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            if (motor != null && cameraTransform != null) motor.SetCamera(cameraTransform);
        }

        private void Update()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            Vector2 input = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

            if (passenger == null) passenger = GetComponent<DropPlanePassenger>();
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (vehicleInput == null) vehicleInput = GetComponent<StarterVehicleInput>();

            if (passenger != null && passenger.IsAboard)
            {
                if (motor != null) motor.SetMoveInput(Vector2.zero);
                if (Input.GetKeyDown(KeyCode.Space)) passenger.Jump();
                return;
            }

            if (parachute != null && parachute.IsActive)
            {
                if (motor != null) motor.SetMoveInput(Vector2.zero);
                parachute.SetSteer(input);
                if (Input.GetKeyDown(KeyCode.Space) && !parachute.IsOpen)
                    parachute.OpenParachute();
                return;
            }

            if (vehicleInput != null && vehicleInput.IsDriving)
            {
                if (motor != null) motor.SetMoveInput(Vector2.zero);
                return;
            }

            if (motor == null || !motor.enabled) return;
            motor.SetMoveInput(input);
            motor.SetSprint(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            if (Input.GetKeyDown(KeyCode.Space)) motor.RequestJump();
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;
            Vector3 desired = transform.position + transform.rotation * cameraOffset;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desired, 1f - Mathf.Exp(-cameraFollow * Time.deltaTime));
            Vector3 lookPoint = transform.position + Vector3.up * 1.35f;
            Vector3 direction = lookPoint - cameraTransform.position;
            if (direction.sqrMagnitude > 0.01f)
                cameraTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
