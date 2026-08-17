using System;
using UnityEngine;

namespace Fsp.Networking
{
    [Serializable]
    public struct NetworkPlayerSnapshot
    {
        public string playerId;
        public string matchId;
        public Vector3 position;
        public Quaternion rotation;
        public float health;
        public float armor;
        public bool alive;
        public double sentAt;
    }
}
