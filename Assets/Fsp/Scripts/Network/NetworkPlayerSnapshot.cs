using System;
using UnityEngine;

namespace Fsp.Networking
{
    public enum NetworkDropState
    {
        Grounded = 0,
        AboardPlane = 1,
        Freefall = 2,
        Parachute = 3
    }

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
        public NetworkDropState dropState;
        public double sentAt;
    }
}
