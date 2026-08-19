using UnityEngine;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>
    /// Author-time binding for the approved Sunscar in-match HUD.
    /// This component never creates UI. All widgets must be authored in Match.unity
    /// and assigned here so the release scene remains visually deterministic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MatchHudBinding : MonoBehaviour
    {
        [Header("HUD Controller")]
        [SerializeField] private BattleRoyaleHud hud;

        [Header("Approved HUD Widgets")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider armorBar;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text aliveText;
        [SerializeField] private Text medkitText;
        [SerializeField] private Text safeZoneWarningText;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button healButton;

        [Header("Visual Contract Anchors")]
        [SerializeField] private RectTransform compassRoot;
        [SerializeField] private RectTransform minimapRoot;
        [SerializeField] private RectTransform movementJoystickRoot;
        [SerializeField] private RectTransform actionButtonsRoot;
        [SerializeField] private RectTransform weaponPanelRoot;

        public bool IsComplete =>
            hud != null &&
            healthBar != null && armorBar != null &&
            ammoText != null && aliveText != null && medkitText != null && safeZoneWarningText != null &&
            reloadButton != null && healButton != null &&
            compassRoot != null && minimapRoot != null && movementJoystickRoot != null &&
            actionButtonsRoot != null && weaponPanelRoot != null;

        private void Awake()
        {
            if (!IsComplete)
            {
                Debug.LogError("FSP Match HUD binding is incomplete. Runtime HUD generation is disabled; wire all approved HUD widgets in Match.unity.", this);
                enabled = false;
                return;
            }

            hud.ConfigureWidgets(
                healthBar,
                armorBar,
                ammoText,
                aliveText,
                medkitText,
                safeZoneWarningText,
                reloadButton,
                healButton);
        }
    }
}
