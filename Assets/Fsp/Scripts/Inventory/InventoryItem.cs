using UnityEngine;

namespace Fsp.Inventory
{
    [CreateAssetMenu(menuName = "Fsp/Inventory/Item")]
    public sealed class InventoryItem : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public InventoryItemType type;
        public int maxStack = 1;
        public int ammoAmount = 30;
        public float healAmount = 50f;
        public float armorAmount = 50f;
    }
}
