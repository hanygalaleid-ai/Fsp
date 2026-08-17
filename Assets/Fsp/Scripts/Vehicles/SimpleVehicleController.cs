using UnityEngine;

namespace Fsp.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SimpleVehicleController : MonoBehaviour
    {
        [SerializeField] private float acceleration = 28f;
        [SerializeField] private float maxSpeed = 30f;
        [SerializeField] private float steeringPower = 90f;
        [SerializeField] private float brakeDamping = 3f;
        [SerializeField] private float normalDamping = 0.25f;

        private Rigidbody body;
        private float throttle;
        private float steering;
        private bool braking;
        private bool driverPresent;

        public bool DriverPresent => driverPresent;
        public float Speed => body != null ? body.linearVelocity.magnitude : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.linearDamping = normalDamping;
        }

        private void FixedUpdate()
        {
            if (!driverPresent || body == null) return;

            if (body.linearVelocity.magnitude < maxSpeed || throttle < 0f)
                body.AddForce(transform.forward * (throttle * acceleration), ForceMode.Acceleration);

            float speedFactor = Mathf.Clamp01(body.linearVelocity.magnitude / 4f);
            float yaw = steering * steeringPower * speedFactor * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw, 0f));
            body.linearDamping = braking ? brakeDamping : normalDamping;
        }

        public void SetDriverPresent(bool present)
        {
            driverPresent = present;
            if (!present)
            {
                throttle = 0f;
                steering = 0f;
                braking = true;
            }
            else
            {
                braking = false;
            }
        }

        public void SetInput(float throttleInput, float steeringInput, bool brake)
        {
            throttle = Mathf.Clamp(throttleInput, -1f, 1f);
            steering = Mathf.Clamp(steeringInput, -1f, 1f);
            braking = brake;
        }
    }
}
