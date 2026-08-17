using Fsp.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    public sealed class WeaponLoadoutCard : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text classText;
        [SerializeField] private Slider damageBar;
        [SerializeField] private Slider rangeBar;
        [SerializeField] private Slider fireRateBar;
        [SerializeField] private Slider controlBar;

        public WeaponConfig Config { get; private set; }

        public void Bind(WeaponConfig config)
        {
            Config = config;
            if (config == null) return;
            if (nameText != null) nameText.text = config.displayName;
            if (classText != null) classText.text = config.weaponClass.ToString();
            if (damageBar != null) damageBar.value = Mathf.Clamp01(config.damage / 60f);
            if (rangeBar != null) rangeBar.value = Mathf.Clamp01(config.range / 320f);
            if (fireRateBar != null) fireRateBar.value = Mathf.Clamp01(config.roundsPerSecond / 14f);
            if (controlBar != null)
            {
                float recoil = config.verticalRecoil + config.horizontalRecoil;
                controlBar.value = 1f - Mathf.Clamp01(recoil / 4f);
            }
        }
    }
}
