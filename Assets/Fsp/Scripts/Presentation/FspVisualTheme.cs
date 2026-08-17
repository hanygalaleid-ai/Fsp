using UnityEngine;

namespace Fsp.Presentation
{
    [CreateAssetMenu(menuName = "Fsp/Presentation/Visual Theme", fileName = "FspVisualTheme")]
    public sealed class FspVisualTheme : ScriptableObject
    {
        [Header("Core palette")]
        public Color deepNavy = new(0.035f, 0.07f, 0.11f, 1f);
        public Color panelNavy = new(0.06f, 0.11f, 0.16f, 0.94f);
        public Color bronze = new(0.72f, 0.47f, 0.22f, 1f);
        public Color sand = new(0.72f, 0.64f, 0.50f, 1f);
        public Color warmWhite = new(0.94f, 0.92f, 0.86f, 1f);
        public Color danger = new(0.75f, 0.18f, 0.14f, 1f);
        public Color success = new(0.25f, 0.58f, 0.42f, 1f);

        [Header("UI")]
        [Range(0f, 1f)] public float hudPanelOpacity = 0.72f;
        [Min(24f)] public float minimumTouchTarget = 56f;
        [Min(0f)] public float cornerRadiusReference = 18f;

        [Header("World")]
        public Color sunColor = new(1f, 0.86f, 0.68f, 1f);
        public Color ambientColor = new(0.38f, 0.43f, 0.46f, 1f);
        [Range(0f, 2f)] public float sunIntensity = 1.05f;
        [Range(0f, 2f)] public float saturation = 0.92f;
    }
}
