using Fsp.Backend;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(Collider))]
    public sealed class SharedLootChest : MonoBehaviour
    {
        [SerializeField] private string chestId = "chest_01";
        [SerializeField] private GameObject[] lootPrefabs;
        [SerializeField] private Transform[] dropPoints;
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open";
        [SerializeField] private MonoBehaviour transportBehaviour;

        private INetworkTransport transport;
        private bool opened;
        private bool claimPending;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            GetComponent<Collider>().isTrigger = true;
            if (string.IsNullOrWhiteSpace(chestId)) chestId = gameObject.name;
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
            if (other.GetComponentInParent<PlayerInventory>() != null) TryOpen();
        }

        public void TryOpen()
        {
            if (opened || claimPending) return;

            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn)
            {
                OpenForEveryone();
                return;
            }

            claimPending = true;
            transport.SendLootClaim(new NetworkLootClaimEvent
            {
                playerId = SupabaseSession.UserId,
                lootId = "chest-open:" + chestId,
                accepted = false,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
        }

        private void HandleClaim(NetworkLootClaimEvent result)
        {
            if (result == null || result.lootId != "chest-open:" + chestId) return;
            claimPending = false;
            if (result.accepted) OpenForEveryone();
        }

        private void OpenForEveryone()
        {
            if (opened) return;
            opened = true;
            if (animator != null && !string.IsNullOrWhiteSpace(openTrigger)) animator.SetTrigger(openTrigger);

            int count = Mathf.Min(lootPrefabs != null ? lootPrefabs.Length : 0, dropPoints != null ? dropPoints.Length : 0);
            for (int i = 0; i < count; i++)
            {
                if (lootPrefabs[i] == null || dropPoints[i] == null) continue;
                GameObject go = Instantiate(lootPrefabs[i], dropPoints[i].position, dropPoints[i].rotation);
                LootPickup pickup = go.GetComponent<LootPickup>();
                if (pickup != null) pickup.SetLootId($"chest:{chestId}:{i}");
            }
        }
    }
}
