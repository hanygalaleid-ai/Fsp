using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class MobileQualityTier : MonoBehaviour
    {
        public enum Tier { Low, Balanced, High }

        [SerializeField] private Tier defaultTier = Tier.Balanced;
        [SerializeField] private bool autoSelectByMemory = true;

        public Tier ActiveTier { get; private set; }

        private void Awake()
        {
            ActiveTier = autoSelectByMemory ? DetectTier() : defaultTier;
            Apply(ActiveTier);
        }

        private static Tier DetectTier()
        {
            int ram = SystemInfo.systemMemorySize;
            if (ram > 0 && ram <= 3500) return Tier.Low;
            if (ram > 0 && ram >= 7000) return Tier.High;
            return Tier.Balanced;
        }

        public void Apply(Tier tier)
        {
            ActiveTier = tier;
            switch (tier)
            {
                case Tier.Low:
                    Application.targetFrameRate = 30;
                    QualitySettings.shadowDistance = 28f;
                    QualitySettings.lodBias = 0.7f;
                    QualitySettings.maximumLODLevel = 1;
                    break;
                case Tier.High:
                    Application.targetFrameRate = 60;
                    QualitySettings.shadowDistance = 65f;
                    QualitySettings.lodBias = 1.35f;
                    QualitySettings.maximumLODLevel = 0;
                    break;
                default:
                    Application.targetFrameRate = 45;
                    QualitySettings.shadowDistance = 42f;
                    QualitySettings.lodBias = 1f;
                    QualitySettings.maximumLODLevel = 0;
                    break;
            }
        }
    }
}
