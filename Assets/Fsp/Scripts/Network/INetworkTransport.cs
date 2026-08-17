using System;

namespace Fsp.Networking
{
    public interface INetworkTransport
    {
        bool IsConnected { get; }
        event Action<NetworkPlayerSnapshot> SnapshotReceived;
        event Action<NetworkFireEvent> FireReceived;
        event Action<NetworkDamageEvent> DamageReceived;
        event Action<NetworkVehicleSnapshot> VehicleReceived;
        void Connect(string matchId, string playerId);
        void Disconnect();
        void SendSnapshot(NetworkPlayerSnapshot snapshot);
        void SendFire(NetworkFireEvent fireEvent);
        void SendDamage(NetworkDamageEvent damageEvent);
        void SendVehicle(NetworkVehicleSnapshot vehicleSnapshot);
    }
}
