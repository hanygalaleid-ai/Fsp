using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Single source of truth for the shipped FSP visual identity. Keeping the palette here makes
    /// cloud-generated scenes deterministic and prevents individual runtime installers drifting apart.
    /// </summary>
    public static class FspFixedTheme
    {
        public static readonly Color Background = new Color(0.018f, 0.035f, 0.055f, 1f);
        public static readonly Color Panel = new Color(0.025f, 0.050f, 0.082f, 0.90f);
        public static readonly Color PanelSoft = new Color(0.035f, 0.065f, 0.098f, 0.80f);
        public static readonly Color Accent = new Color(0.90f, 0.42f, 0.04f, 1f);
        public static readonly Color Bronze = new Color(0.67f, 0.47f, 0.25f, 1f);
        public static readonly Color Text = new Color(0.96f, 0.94f, 0.89f, 1f);
        public static readonly Color MutedText = new Color(0.73f, 0.69f, 0.61f, 1f);
        public static readonly Color Danger = new Color(0.78f, 0.16f, 0.11f, 1f);
        public static readonly Color Sand = new Color(0.48f, 0.40f, 0.28f, 1f);
        public static readonly Color Rock = new Color(0.38f, 0.36f, 0.32f, 1f);
        public static readonly Color Foliage = new Color(0.24f, 0.31f, 0.20f, 1f);
        public static readonly Color Water = new Color(0.16f, 0.38f, 0.48f, 1f);

        public const float LobbyFieldOfView = 58f;
        public const float MatchFieldOfView = 65f;
    }
}
