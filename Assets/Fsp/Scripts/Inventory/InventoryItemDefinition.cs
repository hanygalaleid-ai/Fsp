using UnityEngine;

namespace Fsp.Inventory
{
    public enum InventoryItemType
    {
        Weapon,
        Ammo,
        Medkit,
        Armor
    }

    [CreateAssetMenu(menuName = "FSP/Inventory/Item Definition", fileName = "Item_")]
    public sealed class InventoryItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = "item";
        [SerializeField] private string displayName = "Item";
        [SerializeField] private InventoryItemType itemType;
        [SerializeField, Min(1)] private int maxStack = 1;
        [SerializeField, Min(0f)] private float effectValue = 0f;
        [SerializeField] private GameObject weaponPrefab;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public InventoryItemType ItemType => itemType;
        public int MaxStack => maxStack;
        public float EffectValue => effectValue;
        public GameObject WeaponPrefab => weaponPrefab;
    }
}
