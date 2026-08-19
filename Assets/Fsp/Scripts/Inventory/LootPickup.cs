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
        private bool subscribed;

        public string LootId => lootId;
        public InventoryItem Item => item;

        public void Configure(InventoryItem inventoryItem, string id, bool pickupAutomatically = true)
        {
            item = inventoryItem;
            autoPickup = pickupAutomatically;
            SetLootId(id);
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        public void SetLootId(string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) lootId = value.Trim();
        }

        private void Awake()
        {
            TryResolveTransport();
            if (string.IsNullOrWhiteSpace(lootId)) lootId = gameObject.name;
        }

        private void OnEnable()
        {
            TryResolveTransport();
            TrySubscribe();
        }

        private void Update()
        {
            if (subscribed) return;
            TryResolveTransport();
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (subscribed && transport != null) transport.LootClaimReceived -= HandleLootClaim;
            subscribed = false;
        }

        private void TryResolveTransport()
        {
            transport = transportBehaviour as INetworkTransport;
            if (transport != null) return;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transportBehaviour = behaviour;
                transport = candidate;
                return;
            }
        }

        private void TrySubscribe()
        {
            if (subscribed || transport == null) return;
            transport.LootClaimReceived += HandleLootClaim;
            subscribed = true;
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

            bool onlineMatch = SupabaseSession.IsSignedIn && MatchRoomState.HasMatch;
            if (!onlineMatch)
            {
                if (!inventory.TryPickup(item)) return false;
                Destroy(gameObject);
                return true;
            }

            TryResolveTransport();
            TrySubscribe();
            if (transport == null || !transport.IsConnected) return false;

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
