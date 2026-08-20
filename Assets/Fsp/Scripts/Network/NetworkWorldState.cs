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

    public static class NetworkWorldStateCache
    {
        private static NetworkWorldState current;

        public static bool TryGet(out NetworkWorldState state)
        {
            state = current;
            return state != null;
        }

        public static void Set(NetworkWorldState state)
        {
            current = state;
        }

        public static void Clear()
        {
            current = null;
        }
    }

    // Backward-compatible alias retained for older transport code.
    public static class NetworkWorldClockCache
    {
        public static bool TryGet(out NetworkWorldState state) => NetworkWorldStateCache.TryGet(out state);
        public static void Set(NetworkWorldState state) => NetworkWorldStateCache.Set(state);
        public static void Clear() => NetworkWorldStateCache.Clear();
    }
}
