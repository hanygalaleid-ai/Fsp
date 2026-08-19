using System;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkWorldState
    {
        public double startedAt;
        public double serverNow;
        public double timestamp;
    }

    public static class NetworkWorldClockCache
    {
        public static NetworkWorldState Latest { get; private set; }
        public static bool HasValue => Latest != null && Latest.startedAt > 0 && Latest.serverNow >= Latest.startedAt;

        public static void Set(NetworkWorldState state)
        {
            if (state == null || state.startedAt <= 0 || state.serverNow < state.startedAt) return;
            Latest = state;
        }

        public static void Clear() => Latest = null;
    }
}
