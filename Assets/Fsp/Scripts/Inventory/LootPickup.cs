using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(Collider))]
    public sealed class LootPickup : MonoBehaviour
    {
        [SerializeField] private InventoryItem item;
        [SerializeField] private bool autoPickup = true;

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!autoPickup) return;
            TryGiveTo(other);
        }

        public bool TryGiveTo(Component target)
        {
            if (target == null || item == null) return false;
            PlayerInventory inventory = target.GetComponentInParent<PlayerInventory>();
            if (inventory == null || !inventory.TryPickup(item)) return false;
            Destroy(gameObject);
            return true;
        }
    }
}
