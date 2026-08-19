using System;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkBotAuthorityEvent
    {
        public string playerId;
        public double timestamp;
    }
}
