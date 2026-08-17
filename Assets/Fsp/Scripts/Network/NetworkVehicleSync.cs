using Fsp.Backend;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Networking
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkVehicleSync : MonoBehaviour
    {
        [SerializeField] private string vehicleId = "vehicle_01";
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private SimpleVehicleController controller;
        [SerializeField, Min(1f)] private float sendRate = 10f;
        [SerializeField, Min(1f)] private float positionLerp = 12f;
        [SerializeField, Min(1f)] private float rotationLerp = 12f;

        private INetworkTransport transport;
        private Rigidbody body;
        private float nextSend;
        private NetworkVehicleSnapshot remoteTarget;
        private bool hasRemoteTarget;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            body = GetComponent<Rigidbody>();
            if (controller == null) controller = GetComponent<SimpleVehicleController>();
            if (string.IsNullOrWhiteSpace(vehicleId)) vehicleId = gameObject.name;
        }

        private void OnEnable()
        {
            if (transport != null) transport.VehicleReceived += HandleVehicle;
        }

        private void OnDisable()
        {
            if (transport != null) transport.VehicleReceived -= HandleVehicle;
        }

        private void Update()
        {
            bool locallyDriven = controller != null && controller.DriverPresent && SupabaseSession.IsSignedIn;
            if (locallyDriven)
            {
                hasRemoteTarget = false;
                if (transport == null || !transport.IsConnected || Time.time < nextSend) return;
                nextSend = Time.time + 1f / Mathf.Max(1f, sendRate);
                transport.SendVehicle(new NetworkVehicleSnapshot
                {
                    vehicleId = vehicleId,
                    driverId = SupabaseSession.UserId,
                    position = transform.position,
                    rotation = transform.rotation,
                    velocity = body != null ? body.velocity : Vector3.zero,
                    timestamp = Time.realtimeSinceStartupAsDouble
                });
                return;
            }

            if (!hasRemoteTarget) return;
            transform.position = Vector3.Lerp(transform.position, remoteTarget.position, positionLerp * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, remoteTarget.rotation, rotationLerp * Time.deltaTime);
            if (body != null) body.velocity = remoteTarget.velocity;
        }

        private void HandleVehicle(NetworkVehicleSnapshot snapshot)
        {
            if (snapshot == null || snapshot.vehicleId != vehicleId) return;
            if (snapshot.driverId == SupabaseSession.UserId) return;
            if (controller != null && controller.DriverPresent) return;
            remoteTarget = snapshot;
            hasRemoteTarget = true;
        }
    }
}
