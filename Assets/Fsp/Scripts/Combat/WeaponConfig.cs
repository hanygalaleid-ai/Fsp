using UnityEngine;

namespace Fsp.Combat
{
    public enum WeaponClass { Assault, SMG, Marksman, Shotgun }
    public enum AmmoClass { Light, Medium, Heavy, Shell }

    [CreateAssetMenu(menuName = "FSP/Combat/Weapon Config", fileName = "WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("Identity")]
        public string weaponId = "rifle";
        public string displayName = "Rifle";
        public WeaponClass weaponClass = WeaponClass.Assault;
        public AmmoClass ammoClass = AmmoClass.Medium;

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
        [Range(0.2f, 1f)] public float adsMoveMultiplier = 0.72f;
        [Range(20f, 75f)] public float adsFov = 52f;

        [Header("Attachment support")]
        public bool supportsOptic = true;
        public bool supportsMuzzle = true;
        public bool supportsMagazine = true;
        public bool supportsGrip = true;
    }
}
