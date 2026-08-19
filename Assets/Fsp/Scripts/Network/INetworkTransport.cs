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
        event Action<NetworkSeatEvent> SeatReceived;
        event Action<NetworkLootClaimEvent> LootClaimReceived;
        event Action<NetworkAppearanceEvent> AppearanceReceived;
        event Action<NetworkMatchState> MatchStateReceived;
        event Action<NetworkEliminationEvent> EliminationReceived;
        event Action<NetworkBotAuthorityEvent> BotAuthorityReceived;
        event Action<NetworkWorldState> WorldStateReceived;
        void Connect(string matchId, string playerId);
        void Disconnect();
        void SendSnapshot(NetworkPlayerSnapshot snapshot);
        void SendBotSnapshot(NetworkPlayerSnapshot snapshot);
        void SendFire(NetworkFireEvent fireEvent);
        void SendDamage(NetworkDamageEvent damageEvent);
        void SendBotDamage(NetworkDamageEvent damageEvent);
        void SendZoneProbe(NetworkZoneProbe probe);
        void SendVehicle(NetworkVehicleSnapshot vehicleSnapshot);
        void SendSeat(NetworkSeatEvent seatEvent);
        void SendLootClaim(NetworkLootClaimEvent lootClaim);
        void SendAppearance(NetworkAppearanceEvent appearanceEvent);
    }
}
