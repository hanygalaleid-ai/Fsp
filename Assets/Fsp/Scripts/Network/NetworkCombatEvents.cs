using System;
using UnityEngine;

namespace Fsp.Networking
{
    [Serializable]
    public sealed class NetworkFireEvent
    {
        public string playerId;
        public Vector3 origin;
        public Vector3 direction;
        public int weaponSlot;
        public double timestamp;
    }

    [Serializable]
    public sealed class NetworkDamageEvent
    {
        public string attackerId;
        public string targetId;
        public float damage;
        public Vector3 hitPoint;
        public double timestamp;
    }
}
