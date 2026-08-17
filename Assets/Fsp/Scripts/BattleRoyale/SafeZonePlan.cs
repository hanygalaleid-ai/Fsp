using System;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    [CreateAssetMenu(menuName = "Fsp/Battle Royale/Safe Zone Plan", fileName = "SafeZonePlan")]
    public sealed class SafeZonePlan : ScriptableObject
    {
        [Serializable]
        public struct Phase
        {
            [Min(0f)] public float waitSeconds;
            [Min(1f)] public float shrinkSeconds;
            [Range(0.05f, 1f)] public float radiusFactor;
            [Range(0f, 1f)] public float centerShiftFactor;
            [Min(0f)] public float damagePerSecond;
        }

        [Min(50f)] public float initialRadius = 1100f;
        public Phase[] phases = new Phase[]
        {
            new Phase { waitSeconds = 90f, shrinkSeconds = 70f, radiusFactor = 0.72f, centerShiftFactor = 0.35f, damagePerSecond = 1f },
            new Phase { waitSeconds = 65f, shrinkSeconds = 55f, radiusFactor = 0.55f, centerShiftFactor = 0.45f, damagePerSecond = 2f },
            new Phase { waitSeconds = 50f, shrinkSeconds = 45f, radiusFactor = 0.42f, centerShiftFactor = 0.55f, damagePerSecond = 4f },
            new Phase { waitSeconds = 35f, shrinkSeconds = 35f, radiusFactor = 0.30f, centerShiftFactor = 0.65f, damagePerSecond = 7f },
            new Phase { waitSeconds = 25f, shrinkSeconds = 28f, radiusFactor = 0.18f, centerShiftFactor = 0.75f, damagePerSecond = 11f },
            new Phase { waitSeconds = 15f, shrinkSeconds = 22f, radiusFactor = 0.08f, centerShiftFactor = 0.85f, damagePerSecond = 16f }
        };
    }
}
