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

    [Serializable]
    public sealed class NetworkVehicleSnapshot
    {
        public string vehicleId;
        public string driverId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public double timestamp;
    }
}
