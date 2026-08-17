using Fsp.Backend;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(Collider))]
    public sealed class DeathLootCrate : MonoBehaviour
    {
        [SerializeField] private string crateId;
        [SerializeField] private MonoBehaviour transportBehaviour;

        private INetworkTransport transport;
        private DeathInventorySnapshot contents;
        private PlayerInventory pendingInventory;
        private bool claimPending;

        public void Initialize(string id, DeathInventorySnapshot snapshot)
        {
            crateId = id;
            contents = snapshot;
        }

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (string.IsNullOrWhiteSpace(crateId)) crateId = gameObject.name;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnEnable()
        {
            if (transport != null) transport.LootClaimReceived += HandleClaim;
        }

        private void OnDisable()
        {
            if (transport != null) transport.LootClaimReceived -= HandleClaim;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryLoot(other);
        }

        public bool TryLoot(Component target)
        {
            if (claimPending || contents.IsEmpty || target == null) return false;
            var inventory = target.GetComponentInParent<PlayerInventory>();
            if (inventory == null) return false;

            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn)
            {
                inventory.AddDeathLoot(contents);
                Destroy(gameObject);
                return true;
            }

            pendingInventory = inventory;
            claimPending = true;
            transport.SendLootClaim(new NetworkLootClaimEvent
            {
                playerId = SupabaseSession.UserId,
                lootId = "crate:" + crateId,
                accepted = false,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
            return true;
        }

        private void HandleClaim(NetworkLootClaimEvent result)
        {
            if (result == null || result.lootId != "crate:" + crateId) return;
            if (result.accepted && result.playerId == SupabaseSession.UserId && pendingInventory != null)
                pendingInventory.AddDeathLoot(contents);

            pendingInventory = null;
            claimPending = false;
            Destroy(gameObject);
        }
    }
}
