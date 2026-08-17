using UnityEngine;

namespace Fsp.Combat
{
    [CreateAssetMenu(menuName = "FSP/Combat/Weapon Config", fileName = "WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("Identity")]
        public string weaponId = "rifle";
        public string displayName = "Rifle";

        [Header("Ballistics")]
        [Min(1f)] public float damage = 28f;
        [Min(1f)] public float range = 180f;
        [Min(0.01f)] public float roundsPerSecond = 10f;
        [Min(1)] public int magazineSize = 30;
        [Min(0.1f)] public float reloadSeconds = 2.1f;
        [Range(0f, 10f)] public float spreadDegrees = 0.75f;

        [Header("Feel")]
        [Range(0f, 5f)] public float verticalRecoil = 1.25f;
        [Range(0f, 5f)] public float horizontalRecoil = 0.45f;
    }
}
