using Fsp.Backend;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(Collider))]
    public sealed class LootPickup : MonoBehaviour
    {
        [SerializeField] private InventoryItem item;
        [SerializeField] private bool autoPickup = true;
        [SerializeField] private string lootId;
        [SerializeField] private MonoBehaviour transportBehaviour;

        private INetworkTransport transport;
        private PlayerInventory pendingInventory;
        private bool claimPending;

        public string LootId => lootId;

        public void SetLootId(string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) lootId = value.Trim();
        }

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (string.IsNullOrWhiteSpace(lootId)) lootId = gameObject.name;
        }

        private void OnEnable()
        {
            if (transport != null) transport.LootClaimReceived += HandleLootClaim;
        }

        private void OnDisable()
        {
            if (transport != null) transport.LootClaimReceived -= HandleLootClaim;
        }

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
            if (target == null || item == null || claimPending) return false;
            PlayerInventory inventory = target.GetComponentInParent<PlayerInventory>();
            if (inventory == null || !inventory.CanPickup(item)) return false;

            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn)
            {
                if (!inventory.TryPickup(item)) return false;
                Destroy(gameObject);
                return true;
            }

            pendingInventory = inventory;
            claimPending = true;
            transport.SendLootClaim(new NetworkLootClaimEvent
            {
                playerId = SupabaseSession.UserId,
                lootId = lootId,
                accepted = false,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
            return true;
        }

        private void HandleLootClaim(NetworkLootClaimEvent result)
        {
            if (result == null || result.lootId != lootId) return;

            if (result.accepted && result.playerId == SupabaseSession.UserId && pendingInventory != null)
                pendingInventory.TryPickup(item);

            pendingInventory = null;
            claimPending = false;
            Destroy(gameObject);
        }
    }
}
