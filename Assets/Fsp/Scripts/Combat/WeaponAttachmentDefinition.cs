using UnityEngine;

namespace Fsp.Combat
{
    public enum AttachmentSlot { Optic, Muzzle, Magazine, Grip }

    [CreateAssetMenu(menuName = "FSP/Combat/Weapon Attachment", fileName = "WeaponAttachment")]
    public sealed class WeaponAttachmentDefinition : ScriptableObject
    {
        public string attachmentId = "attachment_001";
        public string displayName = "Attachment";
        public AttachmentSlot slot;
        public GameObject visualPrefab;

        [Header("Multipliers")]
        [Range(0.5f, 1.5f)] public float verticalRecoilMultiplier = 1f;
        [Range(0.5f, 1.5f)] public float horizontalRecoilMultiplier = 1f;
        [Range(0.5f, 1.5f)] public float spreadMultiplier = 1f;
        [Range(0.5f, 1.5f)] public float reloadMultiplier = 1f;
        public int extraMagazineCapacity;
        [Range(-20f, 20f)] public float adsFovOffset;
    }
}
