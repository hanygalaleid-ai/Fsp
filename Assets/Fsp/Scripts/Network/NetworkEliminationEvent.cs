using System;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkEliminationEvent
    {
        public string killerId;
        public string victimId;
        public double timestamp;
    }
}
