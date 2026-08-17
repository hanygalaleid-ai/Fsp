using UnityEngine;

namespace Fsp.AI
{
    public enum BotDifficulty { Easy, Normal, Hard }

    [CreateAssetMenu(menuName = "Fsp/AI/Bot Difficulty", fileName = "BotDifficulty")]
    public sealed class BotDifficultyProfile : ScriptableObject
    {
        public BotDifficulty difficulty = BotDifficulty.Normal;
        [Range(0.15f, 1.5f)] public float decisionInterval = 0.45f;
        [Range(0.25f, 6f)] public float aimErrorDegrees = 2.2f;
        [Range(0.5f, 1.5f)] public float aggression = 1f;
        [Range(0.4f, 1.5f)] public float perceptionMultiplier = 1f;
        [Range(0f, 1f)] public float useCoverChance = 0.7f;
        [Range(0f, 1f)] public float healChance = 0.8f;
        [Range(0f, 1f)] public float weaponSwitchChance = 0.35f;

        public static void ApplyPreset(BotDifficultyProfile profile, BotDifficulty value)
        {
            if (profile == null) return;
            profile.difficulty = value;
            switch (value)
            {
                case BotDifficulty.Easy:
                    profile.decisionInterval = 0.75f;
                    profile.aimErrorDegrees = 4.4f;
                    profile.aggression = 0.75f;
                    profile.perceptionMultiplier = 0.8f;
                    profile.useCoverChance = 0.35f;
                    profile.healChance = 0.55f;
                    profile.weaponSwitchChance = 0.15f;
                    break;
                case BotDifficulty.Hard:
                    profile.decisionInterval = 0.28f;
                    profile.aimErrorDegrees = 1.15f;
                    profile.aggression = 1.2f;
                    profile.perceptionMultiplier = 1.15f;
                    profile.useCoverChance = 0.9f;
                    profile.healChance = 0.95f;
                    profile.weaponSwitchChance = 0.55f;
                    break;
                default:
                    profile.decisionInterval = 0.45f;
                    profile.aimErrorDegrees = 2.2f;
                    profile.aggression = 1f;
                    profile.perceptionMultiplier = 1f;
                    profile.useCoverChance = 0.7f;
                    profile.healChance = 0.8f;
                    profile.weaponSwitchChance = 0.35f;
                    break;
            }
        }
    }
}
