using System;
using Fsp.Backend;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Networking
{
    [RequireComponent(typeof(SimpleVehicleController))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkVehicleSync : MonoBehaviour
    {
        [SerializeField] private string vehicleId;
        [SerializeField, Min(1f)] private float snapshotRate = 10f;
        [SerializeField, Min(1f)] private float remotePositionLerp = 12f;
        [SerializeField, Min(1f)] private float remoteRotationLerp = 14f;

        private SimpleVehicleController controller;
        private Rigidbody body;
        private INetworkTransport transport;
        private bool subscribed;
        private bool localDriving;
        private bool remotelyOccupied;
        private float nextSnapshot;
        private Vector3 remotePosition;
        private Quaternion remoteRotation;
        private Action<bool> pendingSeatResult;

        public string VehicleId => vehicleId;
        public bool LocalDriving => localDriving;
        public bool RemotelyOccupied => remotelyOccupied;

        private void Awake()
        {
            controller = GetComponent<SimpleVehicleController>();
            body = GetComponent<Rigidbody>();
            if (string.IsNullOrWhiteSpace(vehicleId)) vehicleId = BuildStableVehicleId(transform);
            remotePosition = transform.position;
            remoteRotation = transform.rotation;
        }

        private void OnEnable()
        {
            TryWireTransport();
        }

        private void Update()
        {
            if (!subscribed) TryWireTransport();

            if (!localDriving && remotelyOccupied)
            {
                transform.position = Vector3.Lerp(transform.position, remotePosition, 1f - Mathf.Exp(-remotePositionLerp * Time.deltaTime));
                transform.rotation = Quaternion.Slerp(transform.rotation, remoteRotation, 1f - Mathf.Exp(-remoteRotationLerp * Time.deltaTime));
            }

            if (!localDriving || transport == null || !transport.IsConnected || Time.time < nextSnapshot) return;
            nextSnapshot = Time.time + 1f / Mathf.Max(1f, snapshotRate);
            transport.SendVehicle(new NetworkVehicleSnapshot
            {
                vehicleId = vehicleId,
                driverId = SupabaseSession.UserId,
                position = transform.position,
                rotation = transform.rotation,
                velocity = body != null ? body.linearVelocity : Vector3.zero,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            });
        }

        public bool RequestDriverSeat(Action<bool> completed)
        {
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch)
            {
                localDriving = true;
                remotelyOccupied = false;
                completed?.Invoke(true);
                return true;
            }

            TryWireTransport();
            if (transport == null || !transport.IsConnected || pendingSeatResult != null) return false;
            pendingSeatResult = completed;
            transport.SendSeat(new NetworkSeatEvent
            {
                playerId = SupabaseSession.UserId,
                vehicleId = vehicleId,
                seated = true,
                accepted = false,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            });
            return true;
        }

        public void ReleaseDriverSeat()
        {
            if (!localDriving) return;
            localDriving = false;
            controller?.SetDriverPresent(false);

            if (body != null) body.isKinematic = false;
            if (transport != null && transport.IsConnected && SupabaseSession.IsSignedIn && MatchRoomState.HasMatch)
            {
                transport.SendSeat(new NetworkSeatEvent
                {
                    playerId = SupabaseSession.UserId,
                    vehicleId = vehicleId,
                    seated = false,
                    accepted = false,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
                });
            }
        }

        public void MarkLocalDriverActive()
        {
            localDriving = true;
            remotelyOccupied = false;
            if (body != null) body.isKinematic = false;
            controller?.SetDriverPresent(true);
        }

        private void TryWireTransport()
        {
            if (subscribed) return;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transport = candidate;
                transport.VehicleReceived += HandleVehicle;
                transport.SeatReceived += HandleSeat;
                subscribed = true;
                return;
            }
        }

        private void HandleSeat(NetworkSeatEvent seat)
        {
            if (seat == null || seat.vehicleId != vehicleId) return;

            if (seat.playerId == SupabaseSession.UserId)
            {
                if (seat.seated)
                {
                    bool accepted = seat.accepted;
                    localDriving = accepted;
                    remotelyOccupied = false;
                    if (accepted && body != null) body.isKinematic = false;
                    Action<bool> callback = pendingSeatResult;
                    pendingSeatResult = null;
                    callback?.Invoke(accepted);
                }
                else if (seat.accepted)
                {
                    localDriving = false;
                }
                return;
            }

            if (!seat.accepted) return;
            remotelyOccupied = seat.seated;
            if (remotelyOccupied)
            {
                controller?.SetDriverPresent(false);
                if (body != null)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.isKinematic = true;
                }
            }
            else if (!localDriving && body != null)
            {
                body.isKinematic = false;
            }
        }

        private void HandleVehicle(NetworkVehicleSnapshot snapshot)
        {
            if (snapshot == null || snapshot.vehicleId != vehicleId || localDriving) return;
            remotelyOccupied = true;
            remotePosition = snapshot.position;
            remoteRotation = snapshot.rotation;
            if (body != null && !body.isKinematic) body.isKinematic = true;
        }

        private void OnDisable()
        {
            if (subscribed && transport != null)
            {
                transport.VehicleReceived -= HandleVehicle;
                transport.SeatReceived -= HandleSeat;
            }
            subscribed = false;
            pendingSeatResult = null;
        }

        private static string BuildStableVehicleId(Transform target)
        {
            string id = target.name;
            Transform cursor = target.parent;
            while (cursor != null)
            {
                id = cursor.name + "/" + id;
                cursor = cursor.parent;
            }
            return id.Length <= 64 ? id : id.Substring(id.Length - 64, 64);
        }
    }

    public static class NetworkVehicleSyncInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!SupabaseSession.IsSignedIn || !MatchRoomState.HasMatch) return;

            foreach (SimpleVehicleController vehicle in UnityEngine.Object.FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None))
                if (vehicle != null && vehicle.GetComponent<NetworkVehicleSync>() == null)
                    vehicle.gameObject.AddComponent<NetworkVehicleSync>();
        }
    }
}
