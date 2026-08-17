using Fsp.Backend;
using Fsp.Player;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkVehicleSeatSync : MonoBehaviour
    {
        [SerializeField] private string vehicleId = "vehicle_01";
        [SerializeField] private VehicleSeat seat;
        [SerializeField] private MonoBehaviour transportBehaviour;

        private INetworkTransport transport;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (seat == null) seat = GetComponent<VehicleSeat>();
            if (string.IsNullOrWhiteSpace(vehicleId)) vehicleId = gameObject.name;
        }

        private void OnEnable()
        {
            if (seat != null)
            {
                seat.DriverEntered += HandleLocalEntered;
                seat.DriverExited += HandleLocalExited;
            }
            if (transport != null) transport.SeatReceived += HandleRemoteSeat;
        }

        private void OnDisable()
        {
            if (seat != null)
            {
                seat.DriverEntered -= HandleLocalEntered;
                seat.DriverExited -= HandleLocalExited;
            }
            if (transport != null) transport.SeatReceived -= HandleRemoteSeat;
        }

        private void HandleLocalEntered(ThirdPersonMotor driver) => SendSeat(driver, true);
        private void HandleLocalExited(ThirdPersonMotor driver) => SendSeat(driver, false);

        private void SendSeat(ThirdPersonMotor driver, bool seated)
        {
            if (driver == null || transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn) return;
            var identity = driver.GetComponentInParent<NetworkPlayerIdentity>();
            if (identity != null && !identity.IsLocalPlayer) return;

            transport.SendSeat(new NetworkSeatEvent
            {
                playerId = SupabaseSession.UserId,
                vehicleId = vehicleId,
                seated = seated,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
        }

        private void HandleRemoteSeat(NetworkSeatEvent evt)
        {
            if (evt == null || evt.vehicleId != vehicleId || evt.playerId == SupabaseSession.UserId) return;
            if (!RemotePlayerProxy.TryFind(evt.playerId, out var remote)) return;
            remote.SetVehicleSeat(seat != null ? seat.SeatPoint : null, evt.seated);
        }
    }
}
