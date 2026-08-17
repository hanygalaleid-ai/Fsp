using System;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkSeatEvent
    {
        public string playerId;
        public string vehicleId;
        public bool seated;
        public double timestamp;
    }

    [Serializable]
    public sealed class NetworkLootClaimEvent
    {
        public string playerId;
        public string lootId;
        public bool accepted;
        public double timestamp;
    }
}
