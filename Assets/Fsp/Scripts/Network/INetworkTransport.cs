using System;

namespace Fsp.Networking
{
    public interface INetworkTransport
    {
        bool IsConnected { get; }
        event Action<NetworkPlayerSnapshot> SnapshotReceived;
        void Connect(string matchId, string playerId);
        void Disconnect();
        void SendSnapshot(NetworkPlayerSnapshot snapshot);
    }
}
