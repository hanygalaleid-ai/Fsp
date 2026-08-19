using System;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkMatchState
    {
        public int aliveCount;
        public int totalCount;
        public string winnerId;
        public bool finished;
        public double timestamp;
    }
}
