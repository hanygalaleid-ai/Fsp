using System;
using Fsp.Presentation;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkAppearanceEvent
    {
        public string playerId;
        public CosmeticLoadout loadout;
        public double timestamp;
    }
}
