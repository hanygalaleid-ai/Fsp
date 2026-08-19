using System;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkWorldState
    {
        public double startedAt;
        public double serverNow;
        public double timestamp;
        public float countdownSeconds = 8f;
    }
}
