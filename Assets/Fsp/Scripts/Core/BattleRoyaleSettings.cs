using UnityEngine;

namespace Fsp.Core
{
    [CreateAssetMenu(fileName = "BattleRoyaleSettings", menuName = "FSP/Battle Royale Settings")]
    public sealed class BattleRoyaleSettings : ScriptableObject
    {
        [Header("Match")]
        [Min(2)] public int maxPlayers = 32;
        [Min(0)] public int minHumanPlayers = 8;
        public bool fillWithBots = true;
        [Min(1f)] public float preMatchCountdownSeconds = 20f;

        [Header("Zone")]
        [Min(10f)] public float initialZoneRadius = 1200f;
        [Min(5f)] public float finalZoneRadius = 20f;
        [Min(1)] public int zonePhases = 7;
        [Min(5f)] public float firstZoneDelaySeconds = 120f;

        [Header("Player")]
        [Min(1f)] public float maxHealth = 100f;
        [Min(1f)] public float maxArmor = 100f;
        [Min(1)] public int backpackSlots = 20;
        [Min(1)] public int squadSize = 4;

        [Header("Platforms")]
        public bool androidEnabled = true;
        public bool iosEnabled = true;
        public bool windowsEnabled = true;

        private void OnValidate()
        {
            minHumanPlayers = Mathf.Clamp(minHumanPlayers, 0, maxPlayers);
            finalZoneRadius = Mathf.Min(finalZoneRadius, initialZoneRadius);
        }
    }
}
