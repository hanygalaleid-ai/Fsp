using UnityEngine;

namespace Fsp.Presentation
{
    public enum CosmeticSlot
    {
        Head,
        Face,
        Torso,
        Legs,
        Backpack,
        Parachute
    }

    [System.Flags]
    public enum BodyRegionMask
    {
        None = 0,
        Head = 1 << 0,
        Hair = 1 << 1,
        Torso = 1 << 2,
        Arms = 1 << 3,
        Legs = 1 << 4
    }

    [CreateAssetMenu(menuName = "Fsp/Presentation/Cosmetic Item", fileName = "CosmeticItem")]
    public sealed class CosmeticItemDefinition : ScriptableObject
    {
        public string itemId = "cosmetic_001";
        public CosmeticSlot slot;
        public GameObject prefab;
        public BodyRegionMask hiddenBodyRegions;
        public Vector3 localPosition;
        public Vector3 localEuler;
        public Vector3 localScale = Vector3.one;
    }
}
