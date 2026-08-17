using Fsp.Player;
using UnityEngine;

namespace Fsp.Vehicles
{
    /// <summary>
    /// Desktop-only fallback vehicle interaction for the starter scene.
    /// E enters/exits the nearest vehicle, WASD drives, Space brakes.
    /// </summary>
    public sealed class StarterVehicleInput : MonoBehaviour
    {
        [SerializeField] private float enterDistance = 4f;

        private SimpleVehicleController activeVehicle;
        private ThirdPersonMotor motor;
        private CharacterController characterController;
        private Renderer[] renderers;
        private Transform originalParent;

        public bool IsDriving => activeVehicle != null;

        private void Awake()
        {
            motor = GetComponent<ThirdPersonMotor>();
            characterController = GetComponent<CharacterController>();
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (activeVehicle == null) TryEnterNearestVehicle();
                else ExitVehicle();
            }

            if (activeVehicle == null) return;

            float throttle = 0f;
            float steering = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) throttle += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) throttle -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) steering -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steering += 1f;

            activeVehicle.SetInput(throttle, steering, Input.GetKey(KeyCode.Space));
        }

        private void TryEnterNearestVehicle()
        {
            SimpleVehicleController[] vehicles = FindObjectsOfType<SimpleVehicleController>();
            SimpleVehicleController best = null;
            float bestSqr = enterDistance * enterDistance;

            foreach (SimpleVehicleController vehicle in vehicles)
            {
                if (vehicle == null || vehicle.DriverPresent) continue;
                float sqr = (vehicle.transform.position - transform.position).sqrMagnitude;
                if (sqr > bestSqr) continue;
                bestSqr = sqr;
                best = vehicle;
            }

            if (best == null) return;
            activeVehicle = best;
            activeVehicle.SetDriverPresent(true);
            originalParent = transform.parent;

            if (motor != null) motor.enabled = false;
            if (characterController != null) characterController.enabled = false;
            SetRenderers(false);

            transform.SetParent(activeVehicle.transform, false);
            transform.localPosition = new Vector3(0f, 0.9f, 0f);
            transform.localRotation = Quaternion.identity;
        }

        private void ExitVehicle()
        {
            if (activeVehicle == null) return;
            SimpleVehicleController vehicle = activeVehicle;
            vehicle.SetInput(0f, 0f, true);
            vehicle.SetDriverPresent(false);
            activeVehicle = null;

            transform.SetParent(originalParent, true);
            transform.position = vehicle.transform.position + vehicle.transform.right * 2.4f + Vector3.up * 0.2f;
            SetRenderers(true);
            if (characterController != null) characterController.enabled = true;
            if (motor != null) motor.enabled = true;
        }

        private void SetRenderers(bool visible)
        {
            if (renderers == null) return;
            foreach (Renderer renderer in renderers)
                if (renderer != null) renderer.enabled = visible;
        }
    }
}
